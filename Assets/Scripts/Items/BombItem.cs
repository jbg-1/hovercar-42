using Unity.Netcode;
using UnityEngine;

public class BombItem : Item
{
    public override void useItem(ItemController itemController)
    {
        Debug.Log("BombItem used");

       // Get the position above the kart
        Vector3 spawnPosition = itemController.transform.position + itemController.transform.up * 4;
        Quaternion spawnRotation = Quaternion.identity;

        // Spawn the bomb
        GameObject bomb = GameObject.Instantiate(itemController.bombPrefab, spawnPosition, spawnRotation);
        Rigidbody bombRigidbody = bomb.GetComponent<Rigidbody>();

        // Apply forward and upward force to throw the bomb in an arc
        

        // Ensure the bomb is networked (if applicable)
        bomb.GetComponent<NetworkObject>().Spawn();

        Vector3 throwForce = itemController.GetComponent<Rigidbody>().velocity + itemController.transform.forward * 10f + itemController.transform.up * 5f;
        bombRigidbody.AddForce(throwForce, ForceMode.VelocityChange);

    }
}
