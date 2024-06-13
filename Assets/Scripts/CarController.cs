using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct StatePayload : INetworkSerializable
{
    public int tick;
    public Vector3 gravity;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public bool useItem;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref gravity);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref angularVelocity);
        serializer.SerializeValue(ref useItem);
    }

    public override String ToString()
    {
        return "StatePayload(" +
               "tick: " + tick +
               "gravity: " + gravity +
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
    public static int LastCheckpointCollected = 0;
    public static int RoundsCompleted = 0;
    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;




    //CartStateValues
    //Movement
    private Vector3 velocity;
    private Vector3 angularVelocity;

    [SerializeField] bool debugMode = true;

    [Header("CarSettings")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private float flyingHeight = 2;
    [SerializeField] private float turbineStrength = 10;
    [SerializeField] private float maxSpeed = 10;
    [SerializeField] private float driftDamping;
    [SerializeField] private float angularDamping;
  

    [SerializeField] private Vector3 gravity = Vector3.down;
    private Vector3 normGravity = Vector3.down;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float rotationAccelerationY;

    [SerializeField] private float rotationAccelerationX;
    [SerializeField] private float rotationAccelerationZ;


    [Header("Needed Components")]
    private CharacterController characterController;

    [Header("Camera")]
    [SerializeField] private Transform cameraLookAt;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform cameraParent;


    [Header("RaycastPoints")]
    [SerializeField] private GameObject RightFrontTurbineRaycastPoint;
    [SerializeField] private GameObject LeftFrontTurbineRaycastPoint;
    [SerializeField] private GameObject RightBackTurbineRaycastPoint;
    [SerializeField] private GameObject LeftBackTurbineRaycastPoint;

    //ClientPrediction
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 50; //50 FPS
    private const int BUFFER_SIZE = 1024;

    private StatePayload[] stateBuffer;

    //Client specific
    private InputPayload[] inputBuffer;
    //Server specific
    private Queue<InputPayload> inputQueue;

    private StatePayload lastServerState;
    private StatePayload lastProcessedState;
    private ClientRpcParams aimedClient;

    [Header("INFO")]
    [SerializeField] private float magnitude;
     
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
        normGravity = gravity.normalized;
        characterController = GetComponent<CharacterController>();
    }

    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        if (IsOwner)
        {
            CarCameraScript.instance.Setup(cameraTarget, cameraLookAt);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner && !IsServer) return;

        timer += Time.fixedDeltaTime;

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
        if (!IsClient || !IsOwner) {
            cameraParent.gameObject.SetActive(false);
            return;
        }

        if (!lastServerState.Equals(default(StatePayload)) && 
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
            angle = Input.GetAxis("Horizontal") * 180
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

    //ProcessMovement muss deterministisch sein
    private StatePayload ProcessMovement(InputPayload input)
    {

        float upwards = 0;

        Vector3 parallelVector = (Vector3.Dot(transform.forward, velocity)) * transform.forward;
        Vector3 orthogonalVector = velocity - parallelVector;
        magnitude = parallelVector.magnitude;


        Vector3 acceleration = Vector3.zero;


        //Detect floor distance
        float leftFrontHeight = flyingHeight * 2;
        float rightFrontHeight = flyingHeight * 2;
        float leftBackHeight = flyingHeight * 2;
        float rightBackHeight = flyingHeight * 2;
        if (Physics.Raycast(LeftFrontTurbineRaycastPoint.transform.position, -transform.up, out RaycastHit hit1, flyingHeight * 2, groundLayer))
        {
            leftFrontHeight = hit1.distance - 0.7f;
        }
        if (Physics.Raycast(RightFrontTurbineRaycastPoint.transform.position, -transform.up, out RaycastHit hit2, flyingHeight * 2, groundLayer))
        {
            rightFrontHeight = hit2.distance - 0.7f;
        }
        if (Physics.Raycast(LeftBackTurbineRaycastPoint.transform.position, -transform.up, out RaycastHit hit3, flyingHeight * 2, groundLayer))
        {
            leftBackHeight = hit3.distance - 0.7f;
        }
        if (Physics.Raycast(RightBackTurbineRaycastPoint.transform.position, -transform.up, out RaycastHit hit4, flyingHeight * 2, groundLayer))
        {
            rightBackHeight = hit4.distance - 0.7f;
        }
        float total = (leftFrontHeight + rightFrontHeight + leftBackHeight + rightBackHeight) / 4;

        if (total < flyingHeight)
        {
            upwards = (1 - total / flyingHeight);
        }
        float turnRight;
        float turnForward;
        if (total < flyingHeight * 1.1)
        {
            if ((leftFrontHeight + leftBackHeight) / 2 > (rightBackHeight + rightFrontHeight) / 2)
            {
                turnRight = rotationAccelerationZ;
            }
            else
            {
                turnRight = -rotationAccelerationZ;
            }

            if ((leftFrontHeight + rightFrontHeight) / 2 > (leftBackHeight + rightBackHeight) / 2)
            {
                turnForward = rotationAccelerationX;
            }
            else
            {
                turnForward = -rotationAccelerationX;
            }
        }
        else
        {
            turnForward = -Vector3.Dot(normGravity, transform.forward) * rotationAccelerationX;
            turnRight = Vector3.Dot(normGravity, transform.right) * rotationAccelerationZ;
        }

        Vector3 angularAcceleration = new Vector3(turnForward, isActive ? Angle(input.angle) * rotationAccelerationY * (magnitude / maxSpeed) : 0, turnRight);

        float forward = 1 - upwards;

        if (isActive)
        {
            if (magnitude < maxSpeed || Math.Sign(velocity.x) != Math.Sign(transform.forward.x))
            {
                acceleration = forward * turbineStrength * transform.forward;
            }
        }
        acceleration += (upwards * turbineStrength * transform.up) + gravity;
        Vector3 orthogonalVectorNotGravity = orthogonalVector-(Vector3.Dot(normGravity, orthogonalVector)) * normGravity;
        acceleration -= orthogonalVectorNotGravity * driftDamping;

        //damping
        angularVelocity -= angularDamping * minTimeBetweenTicks * angularVelocity;

        //adding acceleration
        angularVelocity += angularAcceleration * minTimeBetweenTicks;
        velocity += acceleration * minTimeBetweenTicks;


        //Transform

        characterController.Move(velocity * minTimeBetweenTicks);

        transform.Rotate(minTimeBetweenTicks * angularVelocity);

        return new StatePayload
        {
            tick = input.tick,
            gravity = gravity,
            angularVelocity = angularVelocity,
            position = transform.position ,
            rotation = transform.rotation,
            velocity = velocity
        };
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.controller == characterController)
        {  
            velocity -= hit.normal * Vector3.Dot(velocity, hit.normal);
        }
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
            gravity = lastServerState.gravity;

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
    
    private void LateUpdate()
    {
        if (IsOwner)
        {
            Vector3 upVector = (Vector3.Dot(transform.up, velocity)) * transform.up;

            if (velocity.magnitude > 0.1f && isActive)
                cameraParent.rotation = Quaternion.LookRotation(velocity-upVector, transform.up);
            else
                cameraParent.rotation = Quaternion.LookRotation(transform.forward, transform.up);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ActivateDrivingRpc()
    {
        isActive = true;
    }

    public void ChangeGravityDirectionTo(Vector3 gravity)
    {
        this.gravity = gravity;
        this.normGravity = gravity.normalized;
    }

    private float Angle(float orientation)
    {
        return -Mathf.Sign(orientation) / (Mathf.Abs(orientation) / 20 + 1) + Mathf.Sign(orientation);
    }

    public void RespawnplayerAtLastCheckpoint()
    {
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;
        transform.position = lastCheckpointPosition;
        transform.rotation = lastCheckpointRotation;
    }

    public void SetLastCheckpoint(Vector3 checkpointPosition, Quaternion checkpointRotation)
    {
        lastCheckpointPosition = checkpointPosition;
        lastCheckpointRotation = checkpointRotation;
    }
}
