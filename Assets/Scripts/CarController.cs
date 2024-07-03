using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using Unity.Netcode;

[Serializable]
public struct CarSettings
{
    public float forewardTurbineStrength;
    public float upwardTurbineStrength;
    public float maxSpeed; // The maximum Speed the foreward acceleration is working
    public float rotationSpeedCarVelocityInfluence;
    public float groundStabilisationRotationStrengthForeward;
    public float groundStabilisationRotationStrengthSide;
    public float airStabilisationRotationStrengthForeward;
    public float airStabilisationRotationStrengthSide;
    public string playerColor;
}

public class CarController : NetworkBehaviour
{
    public NetworkVariable<int> LastCheckpointCollected = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> RoundsCompleted = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> LastCheckpointPosition = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> LastCheckpointRotation = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> Rank = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<Vector3> LastCheckpointGravity = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private static List<ulong> finishedPlayers = new List<ulong>();

    private HUD hud;
    private EOGUI eogui;
    public String TimerString;
    private bool isTimerRunning;
    private float finishTimerDuration = 30f;
    [SerializeField] bool debugMode = false;

    [SerializeField] public CarSettings carSettings;
    [SerializeField] private float rotationSpeed = 2;
    [SerializeField] private bool isDriving = false;

    [SerializeField] private Vector3 gravity;
    [SerializeField] private float flyingHeight = 2f;
    [SerializeField] private float flyingBuffer = 1f;

    [SerializeField] private LayerMask groundMask;

    [Header("Needed Components")]
    [SerializeField] private Rigidbody carRigidbody;

    [SerializeField] private GameObject RightFrontTurbine;
    [SerializeField] private GameObject LeftFrontTurbine;
    [SerializeField] private GameObject RightBackTurbine;
    [SerializeField] private GameObject LeftBackTurbine;

    [Header("Camera")]
    [SerializeField] private Transform cameraLookAt;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform cameraParent;

    private float rotationInput;

    [ReadOnlyField]
    public Vector3 velocity;

    private void Start()
    {
        if (IsOwner)
        {
            CarCameraScript.instance.Setup(cameraTarget, cameraLookAt);
            hud = FindObjectOfType<HUD>();
            if (hud != null)
            {
                hud.SetCarController(this);
            }

            eogui = FindObjectOfType<EOGUI>();
            eogui.gameObject.SetActive(false);

            carSettings.playerColor = "blue";
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (debugMode)
            {
                rotationInput = Input.GetAxis("Horizontal");
            }
            else
            {
                rotationInput = Angle(AppInputController.Orientation);
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Vector3 parallelVector = (Vector3.Dot(transform.forward, carRigidbody.velocity)) * transform.forward;
            Vector3 sideDriftVector = (Vector3.Dot(transform.right, carRigidbody.velocity)) * transform.right;

            float forewardvelocity = parallelVector.magnitude;

            Vector3 acceleration = -sideDriftVector;

            if (isDriving && forewardvelocity < carSettings.maxSpeed)
            {
                acceleration += carSettings.forewardTurbineStrength * transform.forward;
            }

            // Steering
            float rotationAcceleration = rotationInput * rotationSpeed;

            float leftFrontHeight = flyingHeight * 2;
            float rightFrontHeight = flyingHeight * 2;
            float leftBackHeight = flyingHeight * 2;
            float rightBackHeight = flyingHeight * 2;
            Vector3 raycastDirection = gravity;
            if (Physics.Raycast(LeftFrontTurbine.transform.position, raycastDirection, out RaycastHit hitLF, flyingHeight * 2, groundMask))
            {
                leftFrontHeight = hitLF.distance;
            }
            if (Physics.Raycast(RightFrontTurbine.transform.position, raycastDirection, out RaycastHit hitRF, flyingHeight * 2, groundMask))
            {
                rightFrontHeight = hitRF.distance;
            }
            if (Physics.Raycast(LeftBackTurbine.transform.position, raycastDirection, out RaycastHit hitLB, flyingHeight * 2, groundMask))
            {
                leftBackHeight = hitLB.distance;
            }
            if (Physics.Raycast(RightBackTurbine.transform.position, raycastDirection, out RaycastHit hitRB, flyingHeight * 2, groundMask))
            {
                rightBackHeight = hitRB.distance;
            }
            float total = (leftFrontHeight + rightFrontHeight + leftBackHeight + rightBackHeight) / 4;

            if (total < flyingHeight)
            {
                acceleration += carSettings.upwardTurbineStrength * transform.up;
            }
            else
            {
                acceleration += (1 - (total - flyingHeight) / flyingBuffer) * carSettings.upwardTurbineStrength * transform.up;
            }
            acceleration += gravity;

            float turnRight;
            float turnForward;
            if (total < flyingHeight * 2)
            {
                if ((leftFrontHeight + leftBackHeight) - (rightBackHeight + rightFrontHeight) > 0)
                {
                    turnRight = carSettings.groundStabilisationRotationStrengthSide;
                }
                else
                {
                    turnRight = -carSettings.groundStabilisationRotationStrengthSide;
                }
                if ((leftFrontHeight + rightFrontHeight) - (leftBackHeight + rightBackHeight) > 0)
                {
                    turnForward = carSettings.groundStabilisationRotationStrengthForeward;
                }
                else
                {
                    turnForward = -carSettings.groundStabilisationRotationStrengthForeward;
                }
            }
            else
            {
                turnForward = -Vector3.Dot(gravity, transform.forward) * carSettings.airStabilisationRotationStrengthForeward;
                turnRight = Vector3.Dot(gravity, transform.right) * carSettings.airStabilisationRotationStrengthSide;
            }

            if (isDriving)
            {
                carRigidbody.AddRelativeTorque(new Vector3(turnForward, rotationAcceleration, turnRight), ForceMode.Acceleration);
            }
            carRigidbody.AddForce(acceleration, ForceMode.Acceleration);

            velocity = carRigidbody.velocity;
        }
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            if (!isDriving)
                cameraParent.rotation = Quaternion.LookRotation(transform.forward);
            else
                cameraParent.rotation = Quaternion.LookRotation(carRigidbody.velocity);
        }
    }

    private float Angle(float orientation)
    {
        return Mathf.Sign(orientation) / (Mathf.Abs(orientation) / 50 + 1) - Mathf.Sign(orientation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Bouncer>(out Bouncer collisionBouncer))
        {
            carRigidbody.AddForce(collision.impulse * collisionBouncer.BounceRate(), ForceMode.Impulse);
        }

        if (collision.gameObject.CompareTag("DeathBarrier"))
        {
            ReturnToLastCheckpoint();
        }
    }

    public void ChangeGravityDirectionTo(Vector3 newGravityDirection)
    {
        gravity = newGravityDirection;
    }

    // Checkpoint Logic
    public void SetLastCheckpoint(Vector3 checkpointPosition, Vector3 checkpointRotation)
    {
        Vector3 offset = new Vector3(0, 5, 0);
        LastCheckpointPosition.Value = checkpointPosition + offset;
        LastCheckpointRotation.Value = checkpointRotation;
        LastCheckpointGravity.Value = gravity;
    }

    public void ReturnToLastCheckpoint()
    {
        gravity = LastCheckpointGravity.Value;
        carRigidbody.velocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
        transform.position = LastCheckpointPosition.Value;
        transform.eulerAngles = LastCheckpointRotation.Value;
    }

    [ClientRpc]
    public void SetRankClientRpc(int rank)
    {
        // Client requests the server to set the rank
        RequestSetRankServerRpc(rank);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSetRankServerRpc(int rank, ServerRpcParams rpcParams = default)
    {
        Rank.Value = rank;
        if (IsOwner && hud != null)
        {
            hud.UpdateRank(rank);
        }
    }

    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        if (IsOwner)
        {
            CarCameraScript.instance.Setup(cameraTarget, cameraLookAt);
        }
    }

    public void removeObject()
    {
        gameObject.SetActive(false);
    }

    public void stopDriving()
    {
        isDriving = false;
    }

    public void NotifyFinish()
    {
        if (IsOwner)
        {
            NotifyFinishServerRpc(OwnerClientId);
            stopDriving();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyFinishServerRpc(ulong clientId)
    {
        if (finishedPlayers == null)
        {
            Debug.LogError("finishedPlayers list is null.");
            return;
        }

        if (finishedPlayers.Count == 0)
        {
            StartTimerClientRpc();
        }

        if (!finishedPlayers.Contains(clientId))
        {
            finishedPlayers.Add(clientId);
        }

        if (finishedPlayers.Count == NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            StopTimerClientRpc();
            EndGameClientRpc();
        }
    }

    [ClientRpc]
    private void StartTimerClientRpc()
    {
        Debug.Log("Timer started");
        //StartCoroutine(EndGameTimer(30));
        isTimerRunning = true;
        StartCoroutine(UpdateFinishTimer());
    }

    [ClientRpc]
    private void StopTimerClientRpc()
    {
        Debug.Log("Timer should stop running since all players finished.");
        StopCoroutine(UpdateFinishTimer());
        TimerString = ""; 
        isTimerRunning = false;
        EndGameClientRpc();
    }

    private IEnumerator UpdateFinishTimer()
    {
        float timer = finishTimerDuration;

        while (timer > 0 && isTimerRunning)
        {
            TimerString = FormatTimer(timer); // Update TimerString
            
            if (hud == null) {
                hud = FindObjectOfType<HUD>();
            }
          
            hud.UpdateTimer(TimerString); // Update HUD

            timer -= Time.deltaTime;
            yield return null;
        }

        TimerString = ""; 
        isTimerRunning = false;
        EndGameClientRpc();
    }

    private string FormatTimer(float timer)
    {
        int seconds = Mathf.FloorToInt(timer % 60);
        return string.Format("{0:00}", seconds);
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        if (IsServer)
        {
            Debug.Log("Ending the game...");

            CarController[] cars = FindObjectsOfType<CarController>();

            CarController[] sortedCars = cars.OrderBy(car => car.Rank.Value).ToArray();

            foreach (var car in sortedCars)
            {
                if (!finishedPlayers.Contains(car.OwnerClientId))
                {
                    finishedPlayers.Add(car.OwnerClientId);
                    Debug.Log("Player " + car.OwnerClientId + " did not finish in time and has been added to the finished list.");
                    car.stopDriving();
                }
            }
        }

        Debug.Log("All players have finished");
        string finalRankings = string.Join("\n", finishedPlayers.Select((player, index) => $"Player {player} finished in position {index + 1}"));
        eogui.gameObject.SetActive(true);
        eogui.ShowEndOfGameUIAndSetRankingsServerRpc(finalRankings);
    }
}
