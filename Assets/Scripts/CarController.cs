using Unity.Netcode;
using UnityEngine;

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


    private Vector3 accelerationMovement = Vector3.zero;
    private float accelerationRotation = 0;

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
                accelerationRotation += -Input.GetAxis("Horizontal") * verticalRotationSpeed * (1 + carRigidbody.velocity.magnitude * rotationSpeedCarVelocityInfluence) * Time.deltaTime;
            }
            else
            {
                accelerationRotation += Angle(AppInputController.Orientation) * verticalRotationSpeed * (1 + carRigidbody.velocity.magnitude * rotationSpeedCarVelocityInfluence) * Time.deltaTime;
            }
            Vector3 parallelVector = (Vector3.Dot(transform.forward, carRigidbody.velocity)) * transform.forward;
            Vector3 orthogonalVector = carRigidbody.velocity - parallelVector;
            accelerationMovement += (-orthogonalVector * horizontalDriftDamping + transform.forward * horizontalForwardAcceleration) * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
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
                    turnRight = Time.fixedDeltaTime * rotationSpeed;
                }
                else
                {
                    turnRight = -Time.fixedDeltaTime * rotationSpeed;
                }

                if ((leftFrontHeight + rightFrontHeight) / 2 > (leftBackHeight + rightBackHeight) / 2)
                {
                    turnForward = Time.fixedDeltaTime * rotationSpeed;
                }
                else
                {
                    turnForward = -Time.fixedDeltaTime * rotationSpeed;
                }
            }
            else
            {
                turnForward = -Vector3.Dot(gravityDirection, transform.forward) * Time.fixedDeltaTime * rotationSpeed;
                turnRight = Vector3.Dot(gravityDirection, transform.right) * Time.fixedDeltaTime * rotationSpeed;
            }
            carRigidbody.AddRelativeTorque(new Vector3(turnForward, accelerationRotation, turnRight), ForceMode.Acceleration);
            accelerationRotation = 0;
            carRigidbody.AddForce(accelerationMovement + gravityToAdd * Time.fixedDeltaTime, ForceMode.Acceleration);
            accelerationMovement = Vector3.zero;
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
        gravityDirection = newGravityDirection.normalized;
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
