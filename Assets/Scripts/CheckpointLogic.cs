using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static FinishTimer;

[System.Serializable]
public class CheckpointLogic : NetworkBehaviour
{
    [SerializeField] GameObject checkPointParent;

    private Checkpoint[] checkpoints;

    public Dictionary<int, int> checkPointCount = new Dictionary<int, int>();
    public delegate void OnFinished(int carId);
    public event OnFinished onFinish;
    private int totalCheckpointAmount;

    //only for wrong direction detection
    private List<int> collectedCheckpoints = new List<int>();

    private void Start()
    {
        HUD.instance.toggleVisibility(false);
        checkpoints = checkPointParent.GetComponentsInChildren<Checkpoint>();
        totalCheckpointAmount = checkpoints.Length;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].SetID(i);
            checkpoints[i].SetCheckpointLogic(this);
        }
    }

    public void NotifyTrigger(int checkPointId, CarController carController)
    {
        if (!checkPointCount.ContainsKey(carController.carId))
        {
            checkPointCount.Add(carController.carId, 0);
            carController.SetLastCheckpoint(checkpoints[checkPointId].transform.position, checkpoints[checkPointId].transform.rotation.eulerAngles);
        }
        else
        {
            if (checkPointId == (checkPointCount[carController.carId] + 1) % checkpoints.Length)
            {
                checkPointCount[carController.carId] = checkPointCount[carController.carId] + 1;

                carController.SetLastCheckpoint(checkpoints[checkPointId].transform.position + new Vector3(0, 5, 0), checkpoints[checkPointId].transform.rotation.eulerAngles);
            }

            if (!carController.IsOwner)
                return;

            if (checkPointCount[carController.carId] == totalCheckpointAmount)
            {
                HUD.instance.UpdateRounds(2);
            }
            else if (checkPointCount[carController.carId] == totalCheckpointAmount * 2)
            {
                HUD.instance.UpdateRounds(3);
            }
            else if (checkPointCount[carController.carId] == totalCheckpointAmount * 3)
            {
                onFinish(carController.carId);
                HUD.instance.toggleVisibility(false);
            }
        }
        NotifyTriggerClientRpc(checkPointId, carController.carId);
    }

    [ClientRpc]
    public void NotifyTriggerClientRpc(int checkPointId, int carId)
    {
        if (!checkPointCount.ContainsKey(carId))
        {
            checkPointCount.Add(carId, 0);
        }
        else
        {
            if (checkPointId == (checkPointCount[carId] + 1) % checkpoints.Length)
            {
                checkPointCount[carId] = checkPointCount[carId] + 1;
            }
        }
    }


    public GameObject getCheckpoint(int checkpointCount)
    {
        return checkpoints[checkpointCount % checkpoints.Length].gameObject;
    }

    private bool IsDrivinWrongDirection(int checkPointId, CarController carController)
    {
        if (checkPointCount.ContainsKey(carController.carId))
        {
            int checkPointCountInCollectedCheckpoints = collectedCheckpoints.Count(x => x == checkPointId);

            if (checkPointCountInCollectedCheckpoints % 2 == 0 && checkPointCountInCollectedCheckpoints != 0)
            {
                return true;
            }
        }
        return false;
    }
}
