using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Item item;
    private Coroutine autoUseItemCoroutine;

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
        Debug.Log("Item collected");
        CarController carController = GetComponent<CarController>();

        if (item == null)
        {
            int random = Random.Range(1, 5);
            switch (random)
            {
                case 1:
                    HUD.instance.ToggleItemDisplay(true, "banana");
                    item = new BoostItem();
                    break;
                case 2:
                    HUD.instance.ToggleItemDisplay(true, "ice-cube");
                    item = new FreezeItem();
                    break;
                case 3:
                    HUD.instance.ToggleItemDisplay(true, "thunder");
                    item = new LightningItem();
                    break;
                case 4:
                    HUD.instance.ToggleItemDisplay(true, "bomb");
                    item = new SwitchCarItem();
                    break;
            }

            // Start the coroutine to use the item after 3 seconds
            if (autoUseItemCoroutine != null)
            {
                Debug.Log("Item Stop auto use item coroutine");
                StopCoroutine(autoUseItemCoroutine);
            }
            Debug.Log("Item Start auto use item coroutine");
            autoUseItemCoroutine = StartCoroutine(AutoUseItemAfterDelay(3f));
        }
    }

    private IEnumerator AutoUseItemAfterDelay(float delay)
    {
        Debug.Log("Item Auto use item after delay");
        yield return new WaitForSeconds(delay);
        UseItem();
    }
}

