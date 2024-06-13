using UnityEngine;
using Unity.Netcode;

public class Checkpoint : Collide
{
    [SerializeField]
    private int checkpointOrder = 0;

    public override void action(CarController car)
    {
        // Check if this is the next checkpoint in the order
        if (checkpointOrder == CarController.LastCheckpointCollected + 1)
        {
            CarController.LastCheckpointCollected = checkpointOrder;
            car.SetLastCheckpoint(transform.position, transform.rotation);
        }

        // Check if this is the last checkpoint in the round
        if (checkpointOrder == 1 && CarController.LastCheckpointCollected == 9)
        {
            CarController.RoundsCompleted++;
            CarController.LastCheckpointCollected = checkpointOrder;

            // Check if the player has completed 3 rounds
            if (CarController.RoundsCompleted == 3)
            {
            }
        } 
    }
}
