using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour
{
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
        AppInputController.onUseItem += UseItem;
    }

    public void collectItem()
    {
        if (item == null)
        {
            int random = Random.Range(1, 5);
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
                    item = new SwitchCarItem();
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
            }
            yield return new WaitForSeconds(0.2f);
        }

        // Display the actual item
        if (item != null)
        {
            if (item is BoostItem)
                HUD.instance.ToggleItemDisplay(true, "banana");
            else if (item is FreezeItem)
                HUD.instance.ToggleItemDisplay(true, "ice-cube");
            else if (item is LightningItem)
                HUD.instance.ToggleItemDisplay(true, "thunder");
            else if (item is SwitchCarItem)
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
