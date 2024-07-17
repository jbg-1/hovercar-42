using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningItem : Item
{
    public override void useItem(ItemController itemController)
    {
        CarController usingCar = itemController.gameObject.GetComponent<CarController>();

        
        foreach (var car in RaceController.instance.carController.Values)
        {
            Debug.Log("Freeze Item on all cars");
            if (car != usingCar)
            {
                car.spinLightningCLientRPC();
            }
        }

    }
}

