using Unity.Netcode;
using UnityEngine;

public class BombItem : Item
{
    public override void useItem(ItemController itemController)
    {
       // Get the position above the kart
        Vector3 spawnPosition = itemController.transform.position + itemController.transform.up * 4;
        Quaternion spawnRotation = Quaternion.identity;
        Vector3 throwForce = itemController.GetComponent<Rigidbody>().velocity + itemController.transform.forward * 10f + itemController.transform.up * 5f;

        itemController.SpawnBombServerRpc(spawnPosition,spawnRotation,throwForce);
    }
}
