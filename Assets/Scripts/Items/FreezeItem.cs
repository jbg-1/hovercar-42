using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeItem : Item
{
    public override void useItem(ItemController itemController)
    {
        
        Debug.Log("FreezeItem used");
        itemController.gameObject.GetComponent<CarController>().Freeze();
        itemController.StartCoroutine(BoostAfterFreeze(itemController, 10f, 2f));

    }

    private IEnumerator BoostAfterFreeze(ItemController itemController, float boostAmount, float freezeDuration)
    {
        yield return new WaitForSeconds(freezeDuration);
        itemController.gameObject.GetComponent<CarController>().Unfreeze();
        itemController.gameObject.GetComponent<CarController>().Boost(boostAmount);
    }


}
