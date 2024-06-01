using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using System.Linq;
using UnityEditor.PackageManager;


public struct StatePayload : INetworkSerializable
{
    public int tick;
    public ulong networkObjectId;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref networkObjectId);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref angularVelocity);
    }

    public override String ToString()
    {
        return "tick: " + tick +
               "networkObjectId: " + networkObjectId +
               "position: " + position +
               "rotation: " + rotation +
               "velocity: " + velocity +
               "angularVelocity: " + angularVelocity;

    }
}

public struct InputPayload : INetworkSerializable
{
    public int tick;
    public DateTime timestamp;
    public ulong networkObjectId;
    public float angle;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref timestamp);
        serializer.SerializeValue(ref networkObjectId);
        serializer.SerializeValue(ref angle);
    }
    
    public override String ToString()
    {
        return "tick: " + tick +
               "networkObjectId: " + networkObjectId +
               "timestamp: " + timestamp +
               "angle: " + angle;
    }
}

public class CarController : NetworkBehaviour
{
    public static int LastCheckpointCollected = 0;
    public static int RoundsCompleted = 0;
    private Vector3 lastCheckpointPosition;
    private Vector3 lastCheckpointRotation;
    [SerializeField] bool debugMode = true;

    [Header("Speed")]
    [SerializeField] private float horizontalForwardAcceleration = 10;
    [SerializeField] private float horizontalDriftDamping = 10;
    [Tooltip("The accerleration")]
    [SerializeField] private float verticalRotationSpeed = 30;
    [SerializeField] private float rotationSpeedCarVelocityInfluence = 0.1f;
    [SerializeField] private float rotationSpeed = 15;

    [Header("Flying")]
    [SerializeField] private Vector3 gravityDirection = Vector3.down;
    [SerializeField] private float gravityStrength = 10;
    [SerializeField] private float upwardsAcceleration = 20;
    [SerializeField] private float flyingHeight = 2f;
    [SerializeField] private LayerMask groundMask;


    [Header("Needed Components")]
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private Transform cameraParent;
    [SerializeField] private GameObject camera;

    [SerializeField] private GameObject RightFrontTurbine;
    [SerializeField] private GameObject LeftFrontTurbine;
    [SerializeField] private GameObject RightBackTurbine;
    [SerializeField] private GameObject LeftBackTurbine;

    //Netcode
    NetworkTimer networkTimer;
    const float k_serverTickRate = 60f; // 60 FPS
    const int k_bufferSize = 1024;

    // Netcode client specific
    CircularBuffer<StatePayload> clientStateBuffer;
    CircularBuffer<InputPayload> clientInputBuffer;
    StatePayload lastServerState;
    StatePayload lastProcessedState;

    ClientNetworkTransform clientNetworkTransform;

    // Netcode server specific
    CircularBuffer<StatePayload> serverStateBuffer;
    Queue<InputPayload> serverInputQueue;
    CountdownTimer reconciliationTimer;
    CountdownTimer extrapolationTimer;
    StatePayload extrapolationState;
    [SerializeField] float reconciliationCooldownTime = 1f;
    [SerializeField] float reconciliationThreshold = 10f;
    [SerializeField] float extrapolationLimit = 0.5f;
    [SerializeField] float extrapolationMultiplier = 1.2f;

    private void Awake()
    {
        clientNetworkTransform = GetComponent<ClientNetworkTransform>();
        networkTimer = new NetworkTimer(k_serverTickRate);
        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverInputQueue = new Queue<InputPayload>();

        reconciliationTimer = new CountdownTimer(reconciliationCooldownTime);
        extrapolationTimer = new CountdownTimer(extrapolationLimit);

        reconciliationTimer.OnTimerStart += () => {
            extrapolationTimer.Stop();
        };

        extrapolationTimer.OnTimerStart += () => {
            reconciliationTimer.Stop();
            SwitchAuthorityMode(AuthorityMode.Server);
        };
        extrapolationTimer.OnTimerStop += () => {
            extrapolationState = default;
            SwitchAuthorityMode(AuthorityMode.Client);
        };
    }

    void SwitchAuthorityMode(AuthorityMode mode)
    {
        clientNetworkTransform.authorityMode = mode;
        bool shouldSync = mode == AuthorityMode.Client;
        clientNetworkTransform.SyncPositionX = shouldSync;
        clientNetworkTransform.SyncPositionY = shouldSync;
        clientNetworkTransform.SyncPositionZ = shouldSync;
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
        networkTimer.Update(Time.deltaTime);
        reconciliationTimer.Tick(Time.deltaTime);
        extrapolationTimer.Tick(Time.deltaTime);
        Extraplolate();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.position += transform.forward * 20f;
        }
    }

    void FixedUpdate()
    {
        while (networkTimer.ShouldTick())
        {
            HandleClientTick();
            HandleServerTick();
        }

        Extraplolate();
    }

    //The Server looks into the 
    void HandleServerTick()
    {
        if (!IsServer) return;

        var bufferIndex = -1;
        InputPayload inputPayload = default;
        while (serverInputQueue.Count > 0)
        {
            inputPayload = serverInputQueue.Dequeue();
            Debug.Log("Server inputPayload " + inputPayload.ToString());

            bufferIndex = inputPayload.tick % k_bufferSize;

            StatePayload statePayload = ProcessMovement(inputPayload);
            Debug.Log("Server statePayload " + statePayload.ToString());

            serverStateBuffer.Add(statePayload, bufferIndex);
        }

        if (bufferIndex == -1) return;
       
        SendToClientRpc(serverStateBuffer.Get(bufferIndex));
        HandleExtrapolation(serverStateBuffer.Get(bufferIndex), CalculateLatencyInMillis(inputPayload));
    }

    static float CalculateLatencyInMillis(InputPayload inputPayload) => (DateTime.Now - inputPayload.timestamp).Milliseconds / 1000f;

    void Extraplolate()
    {
        if (IsServer && extrapolationTimer.IsRunning)
        {
            transform.position += extrapolationState.position;
        }
    }

    //Wenn die Latenz zu Groß wird übernimmt der Server die Kontrolle und gleicht den Rigedbody ab
    void HandleExtrapolation(StatePayload latest, float latency)
    {
        if (ShouldExtrapolate(latency))
        {
            // Calculate the arc the object would traverse in degrees
            float axisLength = latency * latest.angularVelocity.magnitude * Mathf.Rad2Deg;
            Quaternion angularRotation = Quaternion.AngleAxis(axisLength, latest.angularVelocity);

            if (extrapolationState.position != default)
            {
                latest = extrapolationState;
            }

            // Update position and rotation based on extrapolation
            var posAdjustment = latest.velocity * (1 + latency * extrapolationMultiplier);
            extrapolationState.position = posAdjustment;
            extrapolationState.rotation = angularRotation * transform.rotation;
            extrapolationState.velocity = latest.velocity;
            extrapolationState.angularVelocity = latest.angularVelocity;
            extrapolationTimer.Start();
        }
        else
        {
            extrapolationTimer.Stop();
        }
    }

    bool ShouldExtrapolate(float latency) => latency < extrapolationLimit && latency > Time.fixedDeltaTime;

    [ClientRpc]
    void SendToClientRpc(StatePayload statePayload)
    {
        if (!IsOwner) return;
        lastServerState = statePayload;
    }

    void HandleClientTick()
    {
        if (!IsClient || !IsOwner) return;

        var currentTick = networkTimer.CurrentTick;
        var bufferIndex = currentTick % k_bufferSize;

        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
            timestamp = DateTime.Now,
            networkObjectId = NetworkObjectId,
            angle = Input.GetAxis("Horizontal"),
        };

        Debug.Log("Client inputPayload " + inputPayload.ToString());

        clientInputBuffer.Add(inputPayload, bufferIndex);
        SendToServerRpc(inputPayload);

        StatePayload statePayload = ProcessMovement(inputPayload);
        Debug.Log("Client statePayload " + statePayload.ToString());

        clientStateBuffer.Add(statePayload, bufferIndex);

        HandleServerReconciliation();
    }

    bool ShouldReconcile()
    {
        bool isNewServerState = !lastServerState.Equals(default);
        bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default)
                                               || !lastProcessedState.Equals(lastServerState);

        return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationTimer.IsRunning && !extrapolationTimer.IsRunning;
    }

    void HandleServerReconciliation()
    {
        if (!ShouldReconcile()) return;

        float positionError;
        int bufferIndex;

        bufferIndex = lastServerState.tick % k_bufferSize;
        if (bufferIndex - 1 < 0) return; // Not enough information to reconcile

        StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState; // Host RPCs execute immediately, so we can use the last server state
        StatePayload clientState = IsHost ? clientStateBuffer.Get(bufferIndex - 1) : clientStateBuffer.Get(bufferIndex);
        positionError = Vector3.Distance(rewindState.position, clientState.position);

        if (positionError > reconciliationThreshold)
        {
            ReconcileState(rewindState);
            reconciliationTimer.Start();
        }

        lastProcessedState = rewindState;
    }

    void ReconcileState(StatePayload rewindState)
    {
        transform.position = rewindState.position;
        transform.rotation = rewindState.rotation;
        carRigidbody.velocity = rewindState.velocity;
        carRigidbody.angularVelocity = rewindState.angularVelocity;

        if (!rewindState.Equals(lastServerState)) return;

        clientStateBuffer.Add(rewindState, rewindState.tick % k_bufferSize);

        // Replay all inputs from the rewind state to the current state
        int tickToReplay = lastServerState.tick;

        while (tickToReplay < networkTimer.CurrentTick)
        {
            int bufferIndex = tickToReplay % k_bufferSize;
            StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
            clientStateBuffer.Add(statePayload, bufferIndex);
            tickToReplay++;
        }
    }

    //Dem Server die informationen der eingabe senden
    [ServerRpc]
    void SendToServerRpc(InputPayload input)
    {
        serverInputQueue.Enqueue(input);
    }

    //Das Ausführen der bewegung mit Rückmeldung der rigdbody Information inform einer StatePayload
    StatePayload ProcessMovement(InputPayload input)
    {
        Move(input.angle);

        return new StatePayload()
        {
            tick = input.tick,
            networkObjectId = NetworkObjectId,
            position = transform.position,
            rotation = transform.rotation,
            velocity = carRigidbody.velocity,
            angularVelocity = carRigidbody.angularVelocity
        };
    }

    void Move(float angle)
    {
        Vector3 parallelVector = (Vector3.Dot(transform.forward, carRigidbody.velocity)) * transform.forward;
        Vector3 orthogonalVector = carRigidbody.velocity - parallelVector;
        Vector3 accelerationMovement = (-orthogonalVector * horizontalDriftDamping + transform.forward * horizontalForwardAcceleration) * networkTimer.MinTimeBetweenTicks;
     

        RaycastHit hit;
        float leftFrontHeight = flyingHeight * 2;
        float rightFrontHeight = flyingHeight * 2;
        float leftBackHeight = flyingHeight * 2;
        float rightBackHeight = flyingHeight * 2;
        if (Physics.Raycast(LeftFrontTurbine.transform.position, gravityDirection, out hit, flyingHeight * 2, groundMask))
        {
            leftFrontHeight = hit.distance;
        }
        if (Physics.Raycast(RightFrontTurbine.transform.position, gravityDirection, out hit, flyingHeight * 2, groundMask))
        {
            rightFrontHeight = hit.distance;
        }
        if (Physics.Raycast(LeftBackTurbine.transform.position, gravityDirection, out hit, flyingHeight * 2, groundMask))
        {
            leftBackHeight = hit.distance;
        }
        if (Physics.Raycast(RightBackTurbine.transform.position, gravityDirection, out hit, flyingHeight * 2, groundMask))
        {
            rightBackHeight = hit.distance;
        }
        float total = (leftFrontHeight + rightFrontHeight + leftBackHeight + rightBackHeight) / 4;
        Vector3 gravityToAdd;
        if (total < flyingHeight)
        {
            gravityToAdd = -upwardsAcceleration * (1 - total / flyingHeight) * gravityDirection;
        }
        else
        {
            gravityToAdd = gravityDirection * gravityStrength;
        }
        float turnRight;
        float turnForward;
        if (total < flyingHeight * 1.1)
        {
            if ((leftFrontHeight + leftBackHeight) / 2 > (rightBackHeight + rightFrontHeight) / 2)
            {
                turnRight = networkTimer.MinTimeBetweenTicks * rotationSpeed;
            }
            else
            {
                turnRight = -networkTimer.MinTimeBetweenTicks * rotationSpeed;
            }

            if ((leftFrontHeight + rightFrontHeight) / 2 > (leftBackHeight + rightBackHeight) / 2)
            {
                turnForward = networkTimer.MinTimeBetweenTicks * rotationSpeed;
            }
            else
            {
                turnForward = -networkTimer.MinTimeBetweenTicks * rotationSpeed;
            }
        }
        else
        {
            turnForward = -Vector3.Dot(gravityDirection, transform.forward) * networkTimer.MinTimeBetweenTicks * rotationSpeed;
            turnRight = Vector3.Dot(gravityDirection, transform.right) * networkTimer.MinTimeBetweenTicks * rotationSpeed;
        }

        carRigidbody.velocity += (accelerationMovement + gravityToAdd * networkTimer.MinTimeBetweenTicks) *networkTimer.MinTimeBetweenTicks;
        carRigidbody.AddRelativeTorque(new Vector3(turnForward, angle, turnRight), ForceMode.Acceleration);
        //carRigidbody.AddForce(accelerationMovement + gravityToAdd * networkTimer.MinTimeBetweenTicks, ForceMode.Acceleration);
        //carRigidbody.AddRelativeTorque(new Vector3(0, angle, 0), ForceMode.Acceleration);
        //carRigidbody.AddForce(accelerationMovement * networkTimer.MinTimeBetweenTicks, ForceMode.Acceleration);
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            if(carRigidbody.velocity.magnitude > 0.1f)
                cameraParent.rotation = Quaternion.LookRotation(carRigidbody.velocity);
            else
                cameraParent.rotation = Quaternion.LookRotation(transform.forward);
        }
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        Bouncer collisionBouncer;
        if(collision.gameObject.TryGetComponent<Bouncer>(out collisionBouncer))
        {

            carRigidbody.AddForce(collision.impulse * collisionBouncer.BounceRate(), ForceMode.Impulse);
        }

        if (collision.gameObject.CompareTag("DeathBarrier"))
        {
            //ReturnToLastCheckpoint();
        }
    }
    /*
    public void ChangeGravityDirectionTo(Vector3 newGravityDirection)
    {
        gravityDirection = newGravityDirection.normalized;
    }

    /*
    // Checkpoint Logic
    public void SetLastCheckpoint(Vector3 checkpointPosition, Vector3 checkpointRotation)
    {
        Vector3 offset = new Vector3(0, 5, 0);
        lastCheckpointPosition = checkpointPosition;
        lastCheckpointRotation = checkpointRotation; 
    }

    public void ReturnToLastCheckpoint()
    {
        carRigidbody.velocity = Vector3.zero; 
        carRigidbody.angularVelocity = Vector3.zero; 
        transform.position = lastCheckpointPosition;
        transform.eulerAngles = lastCheckpointRotation; 
    }
    */
}
