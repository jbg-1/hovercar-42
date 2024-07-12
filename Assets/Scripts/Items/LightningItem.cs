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
            if (car != usingCar)
            {
                car.LightningClientRpc();
            }
        }
        Debug.Log("LightningItem used");
        itemController.gameObject.GetComponent<CarController>().LightningClientRpc();
        itemController.StartCoroutine(BoostAfterLightning(itemController, 10f, 2f));
    }

    private IEnumerator BoostAfterLightning(ItemController itemController, float boostAmount, float freezeDuration)
    {
        yield return new WaitForSeconds(freezeDuration);
        itemController.gameObject.GetComponent<CarController>().UnfreezeClientRpc();
        itemController.gameObject.GetComponent<CarController>().Boost(boostAmount);
    }
}

