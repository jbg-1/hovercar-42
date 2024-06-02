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

    private Vector3 velocity;
    private Vector3 angularVelocity;

    [SerializeField] bool debugMode = true;

    [Header("Needed Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraParent;
    [SerializeField] private GameObject camera;

    [SerializeField] private GameObject RightFrontTurbine;
    [SerializeField] private GameObject LeftFrontTurbine;
    [SerializeField] private GameObject RightBackTurbine;
    [SerializeField] private GameObject LeftBackTurbine;

    //ClientPrediction
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    const float SERVER_TICK_RATE = 30f; // 60 FPS
    const int BUFFER_SIZE = 1024;

    private StatePayload[] stateBuffer;
    private InputPayload[] inputBuffer;
    private Queue<InputPayload> inputQueue;

    StatePayload lastServerState;
    StatePayload lastProcessedState;
    ClientRpcParams aimedClient;

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
    }

    void Update()
    {
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

        //SendInputToServer
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
        characterController.Move(transform.forward * 10 * minTimeBetweenTicks);
        transform.Rotate(transform.up, input.angle);

        return new StatePayload
        {
            tick = input.tick,
            angularVelocity = angularVelocity,
            position = transform.position ,
            rotation = transform.rotation,
            velocity = velocity
        };
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
}
