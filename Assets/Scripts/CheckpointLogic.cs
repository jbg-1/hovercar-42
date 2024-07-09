using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FinishTimer;

[System.Serializable]
public class CheckpointLogic : MonoBehaviour
{
  [SerializeField] GameObject checkPointParent;

  public Checkpoint[] checkpoints;

  public Dictionary<int, int> checkPointCount = new Dictionary<int, int>();
  public delegate void OnFinished(int carId);
  public event OnFinished onFinish;
  public HUD hud;
  private int totalCheckpointAmount;

  //only for wrong direction detection
  private List<int> collectedCheckpoints = new List<int>();

  private void Start()
  {
    checkpoints = checkPointParent.GetComponentsInChildren<Checkpoint>();
    totalCheckpointAmount = checkpoints.Length;

    hud.UpdateRounds(1);

    for (int i = 0; i < checkpoints.Length; i++)
    {
      checkpoints[i].SetID(i);
      checkpoints[i].SetCheckpointLogic(this);
    }
  }

  public void NotifyTrigger(int checkPointId, CarController carController)
  {
    collectedCheckpoints.Add(checkPointId);

    hud.ToggleWrongDirectionText(IsDrivinWrongDirection(checkPointId, carController));

    if (checkPointId == 2)
    {
      hud.ToggleItemDisplay(true, "banana");
    }
    else if (checkPointId == 4)
    {
      hud.ToggleItemDisplay(true, "bomb");
    }
    else if (checkPointId == 6)
    {
      hud.ToggleItemDisplay(true, "ice-cube");
    }
    else if (checkPointId == 7)
    {
      hud.ToggleItemDisplay(true, "thunder");
    }
    else
    {
      hud.ToggleItemDisplay(false);
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
        Debug.Log("Checkpoint " + checkPointId + " collected; count " + checkPointCount[carController.carId]);
      }

      if (checkPointCount[carController.carId] == totalCheckpointAmount)
      {
        hud.UpdateRounds(2);
      }
      else if (checkPointCount[carController.carId] == totalCheckpointAmount * 2)
      {
        hud.UpdateRounds(3);
      }
      else if (checkPointCount[carController.carId] == totalCheckpointAmount * 3)
      {
        onFinish(carController.carId);
      }
    }
  }

  public GameObject getCheckpoint(int checkpointCount)
  {
    return checkpoints[checkpointCount % checkpoints.Length].gameObject;
  }
  int count = 0;
  private bool IsDrivinWrongDirection(int checkPointId, CarController carController)
  {
    count++;
    if (checkPointCount.ContainsKey(carController.carId))
    {
      int checkPointCountInCollectedCheckpoints = collectedCheckpoints.Count(x => x == checkPointId);

      if (checkPointCountInCollectedCheckpoints % 2 == 0 && checkPointCountInCollectedCheckpoints != 0)
      {
        Debug.Log("Wrong Direction, count: " + count);
        return true;
      }
    }
    Debug.Log("Correct Direction, count: " + count);
    return false;
  }
}
