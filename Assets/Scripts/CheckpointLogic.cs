using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FinishTimer;

[System.Serializable]
public class CheckpointLogic : MonoBehaviour
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
    if (!carController.IsOwner)
    {
      return; 
    }

    collectedCheckpoints.Add(checkPointId);

    carController.hud.ToggleWrongDirectionText(IsDrivinWrongDirection(checkPointId, carController));

    if (checkPointId == 2)
    {
      carController.hud.ToggleItemDisplay(true, "banana");
    }
    else if (checkPointId == 4)
    {
      carController.hud.ToggleItemDisplay(true, "bomb");
    }
    else if (checkPointId == 6)
    {
      carController.hud.ToggleItemDisplay(true, "ice-cube");
    }
    else if (checkPointId == 7)
    {
      carController.hud.ToggleItemDisplay(true, "thunder");
    }
    else
    {
      carController.hud.ToggleItemDisplay(false);
    }

    if (collectedCheckpoints.Count == totalCheckpointAmount - 1)
    {
      collectedCheckpoints.Clear();
    }

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

      if (checkPointCount[carController.carId] == totalCheckpointAmount)
      {
        carController.hud.UpdateRounds(2);
      } else if (checkPointCount[carController.carId] == totalCheckpointAmount * 2)
      {
        carController.hud.UpdateRounds(3);
      } else if (checkPointCount[carController.carId] == totalCheckpointAmount * 3)
      {
        onFinish(carController.carId);
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
