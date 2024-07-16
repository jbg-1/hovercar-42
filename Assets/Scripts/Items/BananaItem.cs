using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BananaItem : Item
{
    public override void useItem(ItemController itemController)
    {
        Vector3 raycastPosition = itemController.transform.position - itemController.transform.forward * 2;

        itemController.SpawnBananaServerRpc(raycastPosition);   
    }
}
