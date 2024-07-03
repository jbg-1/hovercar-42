using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private int checkpointOrder = 0;

    public int CheckpointOrder => checkpointOrder;

    public static int TotalCheckpointsInScene;

    void Start()
    {
        TotalCheckpointsInScene = FindObjectsOfType<Checkpoint>().Length;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called");

        CarController car = other.gameObject.GetComponent<CarController>();
        if (car != null && car.IsOwner)
        {
            Debug.Log("Collided with local player");

            // Check if this is the next checkpoint in the order
            if (checkpointOrder == car.LastCheckpointCollected.Value + 1)
            {
                car.LastCheckpointCollected.Value = checkpointOrder;

                Vector3 checkpointPosition = GetComponent<Collider>().bounds.center;
                Vector3 checkpointRotation = transform.eulerAngles;
                car.SetLastCheckpoint(checkpointPosition, checkpointRotation);

                Debug.Log("Checkpoint set to: " + checkpointPosition);
            }

            if (car.LastCheckpointCollected.Value == 4)
            {
                car.NotifyFinish();
            }

            // Check if this is the last checkpoint in the round
            if (checkpointOrder == 1 && car.LastCheckpointCollected.Value == TotalCheckpointsInScene)
            {
                car.RoundsCompleted.Value++;
                car.LastCheckpointCollected.Value = checkpointOrder;

                Vector3 checkpointPosition = GetComponent<Collider>().bounds.center;
                Vector3 checkpointRotation = transform.eulerAngles;
                car.SetLastCheckpoint(checkpointPosition, checkpointRotation);

                // Check if the player has completed 3 rounds
                if (car.RoundsCompleted.Value == 3)
                {
                    Debug.Log("Finished");
                    car.NotifyFinish();
                }
            }
        }
    }
}
