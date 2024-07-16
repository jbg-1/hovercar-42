using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ItemController : NetworkBehaviour
{
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
            Debug.Log("hasCarItem is true");
            item.useItem(this);
            item = null;
            HUD.instance.ToggleItemDisplay(false);
        }
        else
        {
            Debug.Log("No item to use");
        }
    }

    private void Start()
    {
        ItemCountdown.onUseItem += UseItem;
       
    }

    public void collectItem()
    {
        if (IsOwner)
        {
            if (item == null)
            {
                //int random = Random.Range(1, 5);
                int random = 5;
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
                        BananaItem bananaItem = new BananaItem();
                        item = bananaItem;
                        break;
                    case 5:
                        BombItem bombItem = new BombItem();
                        item = bombItem;
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
            int random = Random.Range(1, 5);
            switch (random)
            {
                case 1:
                    HUD.instance.ToggleItemDisplay(true, "banana");
                    break;
                case 2:
                    HUD.instance.ToggleItemDisplay(true, "ice-cube");
                    break;
                case 3:
                    HUD.instance.ToggleItemDisplay(true, "thunder");
                    break;
                case 4:
                    HUD.instance.ToggleItemDisplay(true, "bomb");
                    break;
                case 5:
                    HUD.instance.ToggleItemDisplay(true, "banana");
                    break;
                case 6:
                    HUD.instance.ToggleItemDisplay(true, "bomb");
                    break;
            }
            yield return new WaitForSeconds(0.2f);
        }

        // Display the actual item
        if (item != null)
        {
            ItemCountdown.instance.StartCountdown(2f);
            if (item is BoostItem)
                HUD.instance.ToggleItemDisplay(true, "banana");
            else if (item is FreezeItem)
                HUD.instance.ToggleItemDisplay(true, "ice-cube");
            else if (item is LightningItem)
                HUD.instance.ToggleItemDisplay(true, "thunder");
            else if (item is BananaItem)
                HUD.instance.ToggleItemDisplay(true, "banana");
            else if (item is BombItem)
                HUD.instance.ToggleItemDisplay(true, "bomb");

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
}
