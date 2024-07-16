using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCarItem : Item
{
    public override void useItem(ItemController itemController)
    {
        Debug.Log("SwitchCarItem used");
        itemController.gameObject.GetComponent<CarController>().SwitchPositionWihtOtherCarClientRpc();
    }
}
