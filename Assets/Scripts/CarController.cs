using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct CarSettings
{
    public float forewardTurbineStrength;
    public float upwardTurbineStrength;
    public float maxSpeed; //The maximum Speed the foreward acceleration is working
    public float rotationSpeedCarVelocityInfluence;
    public float groundStabilisationRotationStrengthForeward;
    public float groundStabilisationRotationStrengthSide;
    public float airStabilisationRotationStrengthForeward;
    public float airStabilisationRotationStrengthSide;

}


public class CarController : NetworkBehaviour
{

    public NetworkVariable<int> LastCheckpointCollected = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> RoundsCompleted = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> LastCheckpointPosition = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> LastCheckpointRotation = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> Rank = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<Vector3> LastCheckpointGravity = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    private HUD hud;  
    [SerializeField] bool debugMode = false;

    [SerializeField] private CarSettings carSettings;
    [SerializeField] private float rotationSpeed = 2;


    [SerializeField] private Vector3 gravity;
    [SerializeField] private float flyingHeight = 2f;
    [SerializeField] private float flyingBuffer = 1f;

    [SerializeField] private LayerMask groundMask;


    [Header("Needed Components")]
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private Transform cameraParent;
    [SerializeField] private GameObject camera;

    [SerializeField] private GameObject RightFrontTurbine;
    [SerializeField] private GameObject LeftFrontTurbine;
    [SerializeField] private GameObject RightBackTurbine;
    [SerializeField] private GameObject LeftBackTurbine;


    private float rotationInput;

    [ReadOnlyField]
   public Vector3 velocity;

    private void Start()
    { 
        if(!IsOwner)
        {
            camera.SetActive(false);
        }
        else 
        {
            hud = FindObjectOfType<HUD>(); 
            if (hud != null)
            {
                hud.SetCarController(this);
            }
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

            if(forewardvelocity < carSettings.maxSpeed)
            {
                acceleration += carSettings.forewardTurbineStrength * transform.forward;
            }

            //Steereing
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

            if(total < flyingHeight)
            {
                acceleration += carSettings.upwardTurbineStrength * transform.up;
            }
            else
            {
                acceleration += (1 - (total-flyingHeight) / flyingBuffer) * carSettings.upwardTurbineStrength * transform.up;
            }
            acceleration += gravity;


            float turnRight;
            float turnForward;
            if (total < flyingHeight * 2)
            {
                if((leftFrontHeight + leftBackHeight) - (rightBackHeight + rightFrontHeight) > 0)
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
                turnRight = Vector3.Dot(gravity, transform.right)  * carSettings.airStabilisationRotationStrengthSide;
            }
      
            carRigidbody.AddRelativeTorque(new Vector3(turnForward, rotationAcceleration, turnRight), ForceMode.Acceleration);
            carRigidbody.AddForce(acceleration, ForceMode.Acceleration);

            velocity = carRigidbody.velocity;
        }    
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            cameraParent.rotation = Quaternion.LookRotation(carRigidbody.velocity);
        }
    }

    private float Angle(float orientation)
    {
        return -Mathf.Sign(orientation) / (Mathf.Abs(orientation) / 20 + 1) + Mathf.Sign(orientation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent<Bouncer>(out Bouncer collisionBouncer))
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
        if(IsOwner)
        {
            Rank.Value = rank;
            //Debug.Log("Rank updated to " + rank);
            if (hud != null)
            {
                hud.UpdateRank(rank);
            }
        }
        
    }
}
