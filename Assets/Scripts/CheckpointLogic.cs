using UnityEngine;
using Unity.Netcode;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private int checkpointOrder = 0;

   private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called");

        CarController car = other.gameObject.GetComponent<CarController>();
        if (car != null && car.IsLocalPlayer)
        {
            Debug.Log("Collided with local player");

            // Check if this is the next checkpoint in the order
            if (checkpointOrder == CarController.LastCheckpointCollected + 1)
            {
                CarController.LastCheckpointCollected = checkpointOrder;

                Vector3 checkpointPosition = GetComponent<Collider>().bounds.center;
                Vector3 checkpointRotation = transform.eulerAngles;
                car.SetLastCheckpoint(checkpointPosition, checkpointRotation);

                Debug.Log("Checkpoint set to: " + checkpointPosition);
            }

            // Check if this is the last checkpoint in the round
            if (checkpointOrder == 1 && CarController.LastCheckpointCollected == 6)
            {
                CarController.RoundsCompleted++;
                CarController.LastCheckpointCollected = checkpointOrder;

                // Check if the player has completed 3 rounds
                if (CarController.RoundsCompleted == 3)
                {
                    Debug.Log("Finished");
                }
            }
        }
    }
}
