using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningItem : Item
{
    public override void useItem(ItemController itemController)
    {
        Debug.Log("LightningItem used");
        itemController.gameObject.GetComponent<CarController>().Lightning();
        itemController.StartCoroutine(BoostAfterLightning(itemController, 10f, 2f));
    }

    private IEnumerator BoostAfterLightning(ItemController itemController, float boostAmount, float freezeDuration)
    {
        yield return new WaitForSeconds(freezeDuration);
        itemController.gameObject.GetComponent<CarController>().Unfreeze();
        itemController.gameObject.GetComponent<CarController>().Boost(boostAmount);
    }
}

