using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private int id;
    private CheckpointLogic checkpointLogic;

    public void SetID(int id)
    {
        this.id = id;
    }

    public void SetCheckpointLogic(CheckpointLogic checkpointLogic)
    {
        this.checkpointLogic = checkpointLogic;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<CarController>(out CarController car))
            checkpointLogic.NotifyTrigger(id,car);
    }
}
