using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using Random = System.Random;
using System.Collections;

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
    public ulong drivingClientID;

    public int carId;

    private Vector3 LastCheckpointPosition;
    private Vector3 LastCheckpointRotation;
    private Vector3 LastCheckpointGravity;
    private bool LastCheckpointUseTubeGravity;

    [SerializeField] bool debugMode = false;

    [SerializeField] public CarSettings carSettings;
    [SerializeField] private float rotationSpeed = 2;
    [SerializeField] private AnimationCurve rotationStrength;
    [SerializeField] public bool isDriving { get; private set; } = false;

    [SerializeField] private Vector3 gravity;
    private Vector3 normGravity;
    [SerializeField] private bool useTubeGravity;

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

    [SerializeField] private CarAudioController carAudioController;

    [SerializeField] private GameObject ice;

    private float rotationInput;

    [ReadOnlyField]
    public Vector3 velocity;

    private void Start()
    {
        normGravity = gravity.normalized;
        if (IsOwner)
        {
            HUD.instance.UpdateRounds(1);
        }
    }

    private void Update()
    {

        if (IsOwner && OwnerClientId == drivingClientID)
        {
            if (debugMode)
            {
                rotationInput = Input.GetAxis("Horizontal");
                HUD.instance.RotateSteeringWheelIndicator(rotationInput);
            }
            else
            {
                rotationInput = Angle(AppInputController.Orientation);
                HUD.instance.RotateSteeringWheelIndicator(AppInputController.Orientation / 180);
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
            Vector3 raycastDirection = -transform.up;
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
                acceleration += Math.Max((1 - (total - flyingHeight) / flyingBuffer), 0) * carSettings.upwardTurbineStrength * transform.up;
            }
            if (useTubeGravity)
            {
                acceleration -= transform.up * gravity.magnitude;
            }
            else
            {
                acceleration += gravity;
            }

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
                if (useTubeGravity)
                {
                    turnForward = Vector3.Dot(normGravity, transform.up) * carSettings.airStabilisationRotationStrengthForeward;
                    turnRight = 0;
                }
                else
                {
                    turnForward = -Vector3.Dot(normGravity, transform.forward) * carSettings.airStabilisationRotationStrengthForeward;
                    turnRight = Vector3.Dot(normGravity, transform.right) * carSettings.airStabilisationRotationStrengthSide;
                }
            }

            if (isDriving)
            {
                carRigidbody.AddRelativeTorque(new Vector3(turnForward, rotationAcceleration, turnRight), ForceMode.Acceleration);
            }
            carRigidbody.AddForce(acceleration, ForceMode.Acceleration);

            velocity = carRigidbody.velocity;
        }
    }

    private float Angle(float orientation)
    {
        float normalizedValue = orientation / 180;
        return Math.Sign(normalizedValue) * rotationStrength.Evaluate(Math.Abs(normalizedValue));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsOwner)
        {
            if (collision.gameObject.TryGetComponent<Bouncer>(out Bouncer collisionBouncer))
            {
                carRigidbody.AddForce(collision.impulse.normalized * 5, ForceMode.VelocityChange);
                carRigidbody.AddForce(collision.impulse * collisionBouncer.BounceRate(), ForceMode.Impulse);
                carAudioController.BounceClientRpc(collision.impulse.magnitude / 25000);
            }

            if (collision.gameObject.CompareTag("DeathBarrier"))
            {
                ReturnToLastCheckpoint();
                HUD.instance.ToggleWrongDirectionText(false);
            }
        }
    }



    public void ChangeGravityDirectionTo(Vector3 newGravityDirection)
    {
        gravity = newGravityDirection;
        normGravity = gravity.normalized;
    }

    public void SetUseTubeGravity(bool value)
    {
        useTubeGravity = value;
    }


    // Checkpoint Logic
    public void SetLastCheckpoint(Vector3 checkpointPosition, Vector3 checkpointRotation)
    {
        Vector3 offset = new Vector3(0, 5, 0);
        LastCheckpointPosition = checkpointPosition + offset;
        LastCheckpointRotation = checkpointRotation;
        LastCheckpointGravity = gravity;
        LastCheckpointUseTubeGravity = useTubeGravity;
    }

    public void ReturnToLastCheckpoint()
    {
        gravity = LastCheckpointGravity;
        carRigidbody.velocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
        transform.position = LastCheckpointPosition;
        transform.eulerAngles = LastCheckpointRotation;
        useTubeGravity = LastCheckpointUseTubeGravity;

    }
    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        if (IsOwner)
        {
            if (OwnerClientId == drivingClientID)
            {
                PlayerColors.PlayerColor color = PlayerColors.instance.GetAllColors()[carId];
                HUD.instance.ChangeColors(color.color, color.gradientColors);
            }
            else { 
                isDriving = false;
            }
        }
       
    }

    [ClientRpc]
    public void StopDrivingClientRpc()
    {
        isDriving = false;
    }

    [ClientRpc]
    public void StartDrivingClientRpc()
    {
        isDriving = true;
    }

    public void Boost(float boostAmount)
    {

        carRigidbody.AddForce(transform.forward * boostAmount, ForceMode.VelocityChange);

    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void FreezeClientRpc()
    {
        ice.SetActive(true);
        carRigidbody.velocity = Vector3.zero;
        carRigidbody.isKinematic = true;
        StartCoroutine(delayFreeze());
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void UnfreezeClientRpc()
    {
        ice.SetActive(false);
        carRigidbody.isKinematic = false;
    }

    [ClientRpc]
    public void SetSpawnInformationClientRpc(int id, ulong clientID)
    {
        this.drivingClientID = clientID;
        this.carId = id;
        RaceController.instance.RegisterCar(id, this);

        helmet.SetMaterials(new List<Material>() { PlayerColors.instance.GetAllColors()[id].material });
        HUD.instance.miniMap.InstantiateMarker(gameObject, id);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void LightningClientRpc()
    {
        carRigidbody.velocity = Vector3.zero;
        carRigidbody.isKinematic = true;
    }


    public void SpinCar()
    {
        StartCoroutine(SpinOutCoroutine());
    }

    [ClientRpc]
    public void BombClientRpc(Vector3 bombImpulse)
    {
        if (IsOwner)
        {
            carRigidbody.AddForce(bombImpulse, ForceMode.VelocityChange);
        }
    }

    private IEnumerator SpinOutCoroutine()
    {
        // Example logic for spinning out
        float spinDuration = 2f;
        float spinSpeed = 360f;  // degrees per second

        float timer = 0f;
        while (timer < spinDuration)
        {
            carRigidbody.MoveRotation(carRigidbody.rotation * Quaternion.Euler(0, spinSpeed * Time.deltaTime, 0));
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator delayFreeze()
    {
        //wait for 2 seconds
        yield return new WaitForSeconds(2);
        UnfreezeClientRpc();
    }


    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void spinLightningClientRpc()
    {
        StartCoroutine(SpinOutCoroutine());
    }
}
