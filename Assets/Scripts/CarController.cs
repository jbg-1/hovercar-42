using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct CarSettings
{
    public float forewardTurbineStrength;
    public float upwardTurbineStrength;
    public float maxSpeed; //The maximum Speed the foreward acceleration is working
    public float rotationSpeedCarVelocityInfluence;
    public float airStabilisationRotationStrength;
    public float groundStabilisationRotationStrength;
}


public class CarController : NetworkBehaviour
{
    public static int LastCheckpointCollected = 0;
    public static int RoundsCompleted = 0;
    private Vector3 lastCheckpointPosition;
    private Vector3 lastCheckpointRotation;
    [SerializeField] bool debugMode = true;

    [SerializeField] private CarSettings carSettings;

    [SerializeField] private Vector3 gravity;
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


    private float rotationInput;

    private void Start()
    { 
        if(!IsOwner)
        {
            camera.SetActive(false);
            SetLastCheckpoint(transform.position, transform.eulerAngles);
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
            float rotationAcceleration = rotationInput * 1;


            float leftFrontHeight = flyingHeight * 4;
            float rightFrontHeight = flyingHeight * 4;
            float leftBackHeight = flyingHeight * 4;
            float rightBackHeight = flyingHeight * 4;
            if (Physics.Raycast(LeftFrontTurbine.transform.position, -transform.up, out RaycastHit hitLF, flyingHeight * 2, groundMask))
            {
                leftFrontHeight = hitLF.distance;
            }
            if (Physics.Raycast(RightFrontTurbine.transform.position, -transform.up, out RaycastHit hitRF, flyingHeight * 2, groundMask))
            {
                rightFrontHeight = hitRF.distance;
            } 
            if (Physics.Raycast(LeftBackTurbine.transform.position, -transform.up, out RaycastHit hitLB, flyingHeight * 2, groundMask))
            {
                leftBackHeight = hitLB.distance;
            }
            if (Physics.Raycast(RightBackTurbine.transform.position, -transform.up, out RaycastHit hitRB, flyingHeight * 2, groundMask))
            {
                rightBackHeight = hitRB.distance;
            }     
            float total = (leftFrontHeight + rightFrontHeight + leftBackHeight + rightBackHeight) / 4;

            if(total < flyingHeight)
            {
                acceleration += (1 - total / flyingHeight) * carSettings.upwardTurbineStrength * transform.up;
            }
            acceleration += gravity;


            float turnRight;
            float turnForward;
            if (total < flyingHeight * 1.5)
            {
                turnRight = carSettings.groundStabilisationRotationStrength * ((leftFrontHeight + leftBackHeight) - (rightBackHeight + rightFrontHeight));
                turnForward = carSettings.groundStabilisationRotationStrength * ((leftFrontHeight + rightFrontHeight) - (leftBackHeight + rightBackHeight));
            }
            else
            {
                turnForward = -Vector3.Dot(gravity, transform.forward) * carSettings.airStabilisationRotationStrength;
                turnRight = Vector3.Dot(gravity, transform.right)  * carSettings.airStabilisationRotationStrength;
            }
      
            carRigidbody.AddRelativeTorque(new Vector3(turnForward, rotationAcceleration, turnRight), ForceMode.Acceleration);
            carRigidbody.AddForce(acceleration, ForceMode.Acceleration);
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
        Bouncer collisionBouncer;
        if(collision.gameObject.TryGetComponent<Bouncer>(out collisionBouncer))
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
}
