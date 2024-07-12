using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;

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
}

public class CarController : NetworkBehaviour
{

    public int carId;

    public Vector3 LastCheckpointPosition;
    public Vector3 LastCheckpointRotation;
    public Vector3 LastCheckpointGravity;

    [SerializeField] bool debugMode = false;

    [SerializeField] public CarSettings carSettings;
    [SerializeField] private float rotationSpeed = 2;
    [SerializeField] private AnimationCurve rotationStrength;
    [SerializeField] private bool isDriving = false;

    [SerializeField] private Vector3 gravity;
    [SerializeField] private float flyingHeight = 2f;
    [SerializeField] private float flyingBuffer = 1f;

    [SerializeField] private LayerMask groundMask;

    [Header("Needed Components")]
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private Renderer helmet;

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

    private PlayerIcons playerIcons;
    public bool hasPlayerIcon = false;
    public PlayerColors.PlayerColor playerColor;


    private void Start()
    {
        if (IsOwner)
        {
            CarCameraScript.instance.Setup(cameraTarget, cameraLookAt);

            HUD.instance.UpdateRounds(1);
        }
    }

    private void Update()
    {

        if (IsOwner)
        {
            if (debugMode)
            {
                rotationInput = Input.GetAxis("Horizontal");
                HUD.instance.RotateSteeringWheelIndicator(rotationInput);
            }
            else
            {
                rotationInput = Angle(AppInputController.Orientation);
                HUD.instance.RotateSteeringWheelIndicator(AppInputController.Orientation/180);
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
            Vector3 boostAccerleration = Vector3.zero;
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
                acceleration += Math.Max((1 - (total - flyingHeight) / flyingBuffer),0) * carSettings.upwardTurbineStrength * transform.up;
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
        float normalizedValue = orientation / 180;
        return Math.Sign(normalizedValue) * rotationStrength.Evaluate(Math.Abs(normalizedValue));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Bouncer>(out Bouncer collisionBouncer))
        {
            carRigidbody.AddForce(collision.impulse.normalized * 5, ForceMode.VelocityChange);
            carRigidbody.AddForce(collision.impulse * collisionBouncer.BounceRate(), ForceMode.Impulse);
        }

        if (collision.gameObject.CompareTag("DeathBarrier"))
        {
            ReturnToLastCheckpoint();
            HUD.instance.ToggleWrongDirectionText(false);
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
        LastCheckpointPosition = checkpointPosition + offset;
        LastCheckpointRotation = checkpointRotation;
        LastCheckpointGravity = gravity;
    }

    public void ReturnToLastCheckpoint()
    {
        gravity = LastCheckpointGravity;
        carRigidbody.velocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
        transform.position = LastCheckpointPosition;
        transform.eulerAngles = LastCheckpointRotation;
    }
    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        if (IsOwner)
        {
            CarCameraScript.instance.Setup(cameraTarget, cameraLookAt);
            
            PlayerColors.PlayerColor color = PlayerColors.instance.GetAllColors()[carId];
            HUD.instance.ChangeColors(color.color, color.gradientColors);
        }
    }

    [ClientRpc]
    public void StopDrivingClientRpc()
    {
        isDriving = false;
    }

    public void SetColor(PlayerColors.PlayerColor color)
    {
        playerColor = color;
    }

    public void Boost(float boostAmount)
    {
  
            carRigidbody.AddForce(transform.forward * boostAmount, ForceMode.VelocityChange);
        
    }

    [ClientRpc]
    public void FreezeClientRpc()
    {
        carRigidbody.velocity = Vector3.zero;
        carRigidbody.isKinematic = true;
    }

    public void Unfreeze()
    {
        carRigidbody.isKinematic = false;
    }

    [ClientRpc]
    public void setSpawnInformationClientRpc(int id)
    {
        this.carId = id;
        RaceController.instance.registerCar(id,this);

        helmet.SetMaterials(new List<Material>() {PlayerColors.instance.GetAllColors()[id].material});
        HUD.instance.miniMap.InstantiateMarker(gameObject,id);
    }

    public void Lightning() 
    {
        carRigidbody.velocity = Vector3.zero;
        carRigidbody.isKinematic = true;
    }

    public void switchPositionWihtOtherCar()
    {
        //switch position with other car
    }

}
