using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct StatePayload : INetworkSerializable
{
    public int tick;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref angularVelocity);
    }

    public override String ToString()
    {
        return "StatePayload(" +
               "tick: " + tick +
               " position: " + position +
               " rotation: " + rotation +
               " velocity: " + velocity +
               " angularVelocity: " + angularVelocity +
               ")";

    }
}

public struct InputPayload : INetworkSerializable
{
    public int tick;
    public float angle;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref angle);
    }
    
    public override String ToString()
    {
        return "InputPayload(" +
               "tick: " + tick +
               " angle: " + angle +
               ")";
    }
}

public class CarController : NetworkBehaviour
{

    //MovementValues
    private Vector3 velocity;
    private Vector3 angularVelocity;

    [SerializeField] bool debugMode = true;

    [Header("CarSettings")]
    [SerializeField] private bool carForewardIsActive = true;
    [SerializeField] private float flyingHeight = 2;
    [SerializeField] private float turbineStrength = 10;
    [SerializeField] private Vector3 gravity = Vector3.down;
    private Vector3 normGravity = Vector3.down;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float rotationAccelerationY;
    [SerializeField] private float rotationAccelerationXZ;

    [Tooltip("angularVelocity -= angularVelocity * angularDamping * minTimeBetweenTicks")]
    [SerializeField] private float angularDamping;
    [Tooltip("velocity -= velocity * damping * minTimeBetweenTicks")]
    [SerializeField] private float damping;


    [Header ("CarPart")]
    [Header("Y")]
    [SerializeField] private GameObject RightFrontTurbineY;
    [SerializeField] private GameObject LeftFrontTurbineY;
    [SerializeField] private GameObject RightBackTurbineY;
    [SerializeField] private GameObject LeftBackTurbineY;
    [Header("X")]
    [SerializeField] private GameObject RightFrontTurbineX;
    [SerializeField] private GameObject LeftFrontTurbineX;
    [SerializeField] private GameObject RightBackTurbineX;
    [SerializeField] private GameObject LeftBackTurbineX;

    [Header("Needed Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraParent;
    [SerializeField] private GameObject camera;

    [Header("RaycastPoints")]
    [SerializeField] private GameObject RightFrontTurbineRaycastPoint;
    [SerializeField] private GameObject LeftFrontTurbineRaycastPoint;
    [SerializeField] private GameObject RightBackTurbineRaycastPoint;
    [SerializeField] private GameObject LeftBackTurbineRaycastPoint;

    //ClientPrediction
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 30f; //30 FPS
    private const int BUFFER_SIZE = 1024;

    private StatePayload[] stateBuffer;

    //Client specific
    private InputPayload[] inputBuffer;
    //Server specific
    private Queue<InputPayload> inputQueue;

    private StatePayload lastServerState;
    private StatePayload lastProcessedState;
    private ClientRpcParams aimedClient;
     
    private void Awake()
    {       
        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputBuffer = new InputPayload[BUFFER_SIZE];
        inputQueue = new Queue<InputPayload>();
        aimedClient = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { OwnerClientId } } };
    }

    private void Start()
    { 
        if(!IsOwner)
        {
            camera.SetActive(false);
        }
        normGravity = gravity.normalized;
    }

    void Update()
    {
        if (!IsOwner && !IsServer) return;

        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleClientTick();
            HandleServerTick();
            currentTick++;
        }
    }  

    private void HandleClientTick()
    {
        if (!IsClient || !IsOwner) return;

        if(!lastServerState.Equals(default(StatePayload)) && 
            lastProcessedState.Equals(default(StatePayload)) || 
            !lastServerState.Equals(lastProcessedState))
        {
            HandleServerReconciliation();
            Debug.Log("Should Reconcile");
        }
        else
        {
            Debug.Log("Not Reconcile");
        }

        int bufferIndex = currentTick % BUFFER_SIZE;

        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
            angle = Input.GetAxis("Horizontal")
        };

        inputBuffer[bufferIndex] = inputPayload;
        stateBuffer[bufferIndex] = ProcessMovement(inputPayload);

        SendToServerRpc(inputPayload);
    }

    private void HandleServerTick()
    {
        if(!IsServer) return;

        int bufferIndex = -1;
        while (inputQueue.Count > 0) { 
            InputPayload inputPayload = inputQueue.Dequeue();
            bufferIndex = inputPayload.tick % BUFFER_SIZE;
            StatePayload statePayload = ProcessMovement(inputPayload);
            stateBuffer[bufferIndex] = statePayload;
        }
        if (bufferIndex != -1)
        {
            SendToClientRpc(stateBuffer[bufferIndex], aimedClient);
        }
    }
    
    [ClientRpc]
    private void SendToClientRpc(StatePayload statePayload,ClientRpcParams clientRpcSendParams)
    {
        if (!IsOwner) return;
        lastServerState = statePayload;
    }

    [ServerRpc]
    private void SendToServerRpc(InputPayload input)
    {
        inputQueue.Enqueue(input);
    }

    private float test = -1;
    //ProcessMovement muss deterministisch sein
    private StatePayload ProcessMovement(InputPayload input)
    {

        float upwards = 0;
        Vector3 acceleration = Vector3.zero;
        Vector3 angularAcceleration = Vector3.zero;


        //Detect floor distance
        RaycastHit hit;
        float leftFrontHeight = flyingHeight * 2;
        float rightFrontHeight = flyingHeight * 2;
        float leftBackHeight = flyingHeight * 2;
        float rightBackHeight = flyingHeight * 2;
        if (Physics.Raycast(LeftFrontTurbineRaycastPoint.transform.position, -transform.up, out hit, flyingHeight * 2, groundLayer))
        {
            leftFrontHeight = hit.distance;
        }
        if (Physics.Raycast(RightFrontTurbineRaycastPoint.transform.position, -transform.up, out hit, flyingHeight * 2, groundLayer))
        {
            rightFrontHeight = hit.distance;
        }
        if (Physics.Raycast(LeftBackTurbineRaycastPoint.transform.position, -transform.up, out hit, flyingHeight * 2, groundLayer))
        {
            leftBackHeight = hit.distance;
        }
        if (Physics.Raycast(RightBackTurbineRaycastPoint.transform.position, -transform.up, out hit, flyingHeight * 2, groundLayer))
        {
            rightBackHeight = hit.distance;
        }
        float total = (leftFrontHeight + rightFrontHeight + leftBackHeight + rightBackHeight) / 4;

        if (total < flyingHeight)
        {
            upwards = (1 - total / flyingHeight);
            upwards *= upwards;
        }
        float turnRight;
        float turnForward;
        if (total < flyingHeight * 1.1)
        {
            if ((leftFrontHeight + leftBackHeight) / 2 > (rightBackHeight + rightFrontHeight) / 2)
            {
                turnRight = rotationAccelerationXZ;
            }
            else
            {
                turnRight = -rotationAccelerationXZ;
            }

            if ((leftFrontHeight + rightFrontHeight) / 2 > (leftBackHeight + rightBackHeight) / 2)
            {
                turnForward = rotationAccelerationXZ;
            }
            else
            {
                turnForward = -rotationAccelerationXZ;
            }
        }
        else
        {
            turnForward = -Vector3.Dot(normGravity, transform.forward) * rotationAccelerationXZ;
            turnRight = Vector3.Dot(normGravity, transform.right) * rotationAccelerationXZ;
        }

        angularAcceleration = new Vector3(turnForward, input.angle * rotationAccelerationY, turnRight);

        float forward = 1 - upwards;
        if (!carForewardIsActive)
            forward = 0;

        RotateTurbines(input.angle, forward);

        acceleration = upwards * turbineStrength * transform.up + forward * turbineStrength * transform.forward + gravity;

        //damping
        angularVelocity -= angularDamping * minTimeBetweenTicks * angularVelocity;
        velocity -= damping * minTimeBetweenTicks * velocity;

        //adding acceleration
        angularVelocity += angularAcceleration * minTimeBetweenTicks;
        velocity += acceleration * minTimeBetweenTicks;     

        //Transform
        characterController.Move(velocity * minTimeBetweenTicks);
        transform.Rotate(minTimeBetweenTicks * angularVelocity);
        
        return new StatePayload
        {
            tick = input.tick,
            angularVelocity = angularVelocity,
            position = transform.position ,
            rotation = transform.rotation,
            velocity = velocity
        };
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        velocity -= hit.normal * Vector3.Dot(velocity, hit.normal);
    }

    private void HandleServerReconciliation()
    {
        lastProcessedState = lastServerState;

        int serverStateBufferIndex = lastServerState.tick % BUFFER_SIZE;
        float positionError = Vector3.Distance(lastServerState.position, stateBuffer[serverStateBufferIndex].position);

        if (positionError > 0.001f)
        {
            Debug.Log("Reconcile");

            //Setze alle den Zustand beschreibenden Werte aud den letzten vom Server richtig berechneten Wert
            transform.position = lastProcessedState.position;
            transform.rotation = lastServerState.rotation;
            velocity = lastServerState.velocity;
            angularVelocity = lastServerState.angularVelocity;

            stateBuffer[serverStateBufferIndex] = lastServerState;

            int tickToProcess = lastServerState.tick + 1;

            //Alle weiteren Eingaben simulieren
            while (tickToProcess < currentTick)
            {
                int bufferIndex = tickToProcess % BUFFER_SIZE;

                StatePayload statePayload = ProcessMovement(inputBuffer[bufferIndex]);

                stateBuffer[bufferIndex] = statePayload;

                tickToProcess++;
            }
        }
    }

    private void RotateTurbines(float rotationStrength, float forwardStrength)
    {
        if (rotationStrength < 0)
        {
            LeftFrontTurbineY.transform.localEulerAngles = new Vector3(-90 + 20 * rotationStrength, 0, 0);
            LeftBackTurbineY.transform.localEulerAngles = new Vector3(90 + 40 * rotationStrength, 180, 0);
            RightBackTurbineY.transform.localEulerAngles = new Vector3(-90 + 20 * rotationStrength, 0, 0);
            RightFrontTurbineY.transform.localEulerAngles = new Vector3(-90 - 40 * rotationStrength, 0, 0);
        }
        else
        {
            LeftFrontTurbineY.transform.localEulerAngles = new Vector3(-90 + 40 * rotationStrength, 0, 0);
            LeftBackTurbineY.transform.localEulerAngles = new Vector3(90 + 20 * rotationStrength, 180, 0);
            RightBackTurbineY.transform.localEulerAngles = new Vector3(-90 + 40 * rotationStrength, 0, 0);
            RightFrontTurbineY.transform.localEulerAngles = new Vector3(-90 - 20 * rotationStrength, 0, 0);
        }

        LeftFrontTurbineX.transform.localEulerAngles = new Vector3(0,-forwardStrength * 90, 0);
        LeftBackTurbineX.transform.localEulerAngles = new Vector3(0, -forwardStrength * 90, 0);
        RightBackTurbineX.transform.localEulerAngles = new Vector3(0, forwardStrength * 90, 0);
        RightFrontTurbineX.transform.localEulerAngles = new Vector3(0, forwardStrength * 90, 0);
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            if (velocity.magnitude > 0.1f && !velocity.normalized.Equals(Vector3.up))
                cameraParent.rotation = Quaternion.LookRotation(velocity);
            else
                cameraParent.rotation = Quaternion.LookRotation(transform.forward);
        }
    }
}
