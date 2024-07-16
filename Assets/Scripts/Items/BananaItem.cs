using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BananaItem : Item
{
    public override void useItem(ItemController itemController)
    {
        Debug.Log("BananaItem used");

        // Get the position behind the kart
        Vector3 spawnPosition = itemController.transform.position - itemController.transform.forward * 2;
        Quaternion spawnRotation = Quaternion.identity;

        // Spawn the banana
        GameObject banana = GameObject.Instantiate(itemController.bananaPrefab, spawnPosition, spawnRotation);
        banana.GetComponent<NetworkObject>().Spawn();

    }
}
