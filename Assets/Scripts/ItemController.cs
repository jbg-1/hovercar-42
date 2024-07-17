using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ItemController : NetworkBehaviour
{
    [SerializeField] private LayerMask ground;

    public GameObject bananaPrefab;
    public GameObject bombPrefab;

    public Item item;
    private Coroutine autoUseItemCoroutine;
    private Coroutine displayRandomItemsCoroutine;

    // Method to use the item
    public void UseItem()
    {
        if (item != null)
        {
            item.useItem(this);
            item = null;
            HUD.instance.ToggleItemDisplay(false);
        }
    }

    private void Start()
    {
        ItemCountdown.onUseItem += UseItem;
       
    }

    public void CollectItem()
    {
        if (IsOwner)
        {
            if (item == null)
            {
                int random = Random.Range(1, 6);
                switch (random)
                {
                    case 1:
                        item = new BoostItem();
                        break;
                    case 2:
                        item = new FreezeItem();
                        break;
                    case 3:
                        item = new LightningItem();
                        break;
                    case 4:
                        item = new BananaItem();
                        break;
                    case 5:
                        item = new BombItem();
                        break;
                }

                // Start the coroutine to display random items for 2 seconds
                if (displayRandomItemsCoroutine != null)
                {
                    StopCoroutine(displayRandomItemsCoroutine);
                }
                displayRandomItemsCoroutine = StartCoroutine(DisplayRandomItemsForDuration(2f));
            }
        }
    }

    private IEnumerator DisplayRandomItemsForDuration(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += 0.2f; // Change item every 0.2 seconds
            int random = Random.Range(0, 5);
            HUD.instance.ToggleItemDisplay(true, random);
            yield return new WaitForSeconds(0.2f);
        }

        // Display the actual item
        if (item != null)
        {
            ItemCountdown.instance.StartCountdown(3f);
            if (item is BoostItem)
                HUD.instance.ToggleItemDisplay(true, 0);
            else if (item is FreezeItem)
                HUD.instance.ToggleItemDisplay(true, 1);
            else if (item is LightningItem)
                HUD.instance.ToggleItemDisplay(true, 2);
            else if (item is BananaItem)
                HUD.instance.ToggleItemDisplay(true, 3);
            else if (item is BombItem)
                HUD.instance.ToggleItemDisplay(true, 4);

            // Start the coroutine to use the item after 3 seconds
            if (autoUseItemCoroutine != null)
            {
                StopCoroutine(autoUseItemCoroutine);
            }
            autoUseItemCoroutine = StartCoroutine(AutoUseItemAfterDelay(3f));
        }
    }

    private IEnumerator AutoUseItemAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UseItem();
    }

    [ServerRpc]
    public void SpawnBombServerRpc(Vector3 spawnPosition, Quaternion spawnRotation, Vector3 throwForce)
    {
        GameObject bomb = GameObject.Instantiate(this.bombPrefab, spawnPosition, spawnRotation);
        bomb.GetComponent<NetworkObject>().Spawn();

        Rigidbody bombRigidbody = bomb.GetComponent<Rigidbody>();
        bombRigidbody.AddForce(throwForce, ForceMode.VelocityChange);
    }

    [ServerRpc]
    public void SpawnBananaServerRpc(Vector3 raycastPosition)
    {
        if(Physics.Raycast(raycastPosition, -Vector3.up, out RaycastHit hit, 5f, ground))
        {
            GameObject banana = GameObject.Instantiate(bananaPrefab, hit.point, Quaternion.LookRotation(hit.normal,transform.forward));
            banana.GetComponent<NetworkObject>().Spawn();
        }
    }
}