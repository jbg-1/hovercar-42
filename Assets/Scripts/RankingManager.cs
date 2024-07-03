using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RankingManager : NetworkBehaviour
{
    private CarController[] cars;
    private Checkpoint[] checkpoints;

    private void Start()
    {
        cars = FindObjectsOfType<CarController>();
        checkpoints = FindObjectsOfType<Checkpoint>();
    }

    private void Update()
    {
        if (IsServer)
        {
            CalculateRankingsServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CalculateRankingsServerRpc()
    {

        cars = FindObjectsOfType<CarController>(); // Ensure we always have the latest list of cars
        var rankedCars = cars.OrderByDescending(car => car.RoundsCompleted.Value)
                            .ThenByDescending(car => car.LastCheckpointCollected.Value)
                            .ThenBy(car => Vector3.Distance(car.transform.position, GetNextCheckpointPosition(car.LastCheckpointCollected.Value + 1)))
                            .ToList();

        for (int i = 0; i < rankedCars.Count; i++)
        {
            rankedCars[i].SetRankClientRpc(i + 1);
        }
    }

    private Vector3 GetNextCheckpointPosition(int nextCheckpointOrder)
    {
        if (nextCheckpointOrder > checkpoints.Length)
        {
            nextCheckpointOrder = 1;
        }

        foreach (var checkpoint in checkpoints)
        {
            if (checkpoint.CheckpointOrder == nextCheckpointOrder)
            {
                return checkpoint.transform.position;
            }
        }

        return Vector3.zero;
    }
}
