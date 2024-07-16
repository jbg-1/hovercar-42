using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostItem : Item
{
    public override void useItem(ItemController itemController)
    {
        itemController.gameObject.GetComponent<CarController>().Boost(50f);
    }
}
