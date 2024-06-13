using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : Collide
{
    public override void action(CarController car)
    {
        Debug.Log("Death");

        car.RespawnplayerAtLastCheckpoint();
    }
}