using System.Collections.Generic;
using UnityEngine;
using static FinishTimer;

[System.Serializable]
public class CheckpointLogic : MonoBehaviour
{
  [SerializeField] GameObject checkPointParent;

  public Checkpoint[] checkpoints;

  public Dictionary<int, int> checkPointCount = new Dictionary<int, int>();
  public HUD hud;
  public delegate void OnFinished(int carId);
  public event OnFinished onFinish;


  private void Start()
  {
    checkpoints = checkPointParent.GetComponentsInChildren<Checkpoint>();

    for (int i = 0; i < checkpoints.Length; i++)
    {
      checkpoints[i].SetID(i);
      checkpoints[i].SetCheckpointLogic(this);
    }
  }

  public void NotifyTrigger(int checkPointId, CarController carController)
  {
    if (IsDrivinWrongDirection(checkPointId, carController))
    {
      hud.ToggleWrongDirectionText(true);
    }
    else
    {
      hud.ToggleWrongDirectionText(false);
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

      if (checkPointCount[carController.carId] / checkpoints.Length == 1)
      {
        onFinish(carController.carId);
      }
    }
  }

  public GameObject getCheckpoint(int checkpointCount)
  {
    return checkpoints[checkpointCount % checkpoints.Length].gameObject;
  }


  // TODO: funktioniert einwandfrei, einziges Problem ist, dass es dadurch dass es nur bei trigger eines checkpoints aufgrufen wird, verzögert ist
  // TODO: möglicher anderer Ansatz: object/camera welches IMMER vor dem auto fliegt und dieses faced, wenn kart dann nicht mehr faced -> falsche Richtung
  private bool IsDrivinWrongDirection(int checkPointId, CarController carController)
  {
    Vector3 checkPointForward = checkpoints[checkPointId].transform.forward;
    Vector3 carForward = carController.transform.forward;

    float angle = Vector3.Angle(checkPointForward, carForward);

    return angle > 90;
  }
}
