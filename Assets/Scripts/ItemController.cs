using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Item item;

    // Method to use the item
    public void UseItem()
    {
        if (item != null)
        {
            Debug.Log("hasCarItem is true");
            item.useItem(this);
            item = null;
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

    public void collectItem() {
        CarController carController = GetComponent<CarController>();
    
        if(item == null){
            //generate a random number between 1 and 4
            int random = Random.Range(1, 5);
            //switch case to determine which item to generate
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
        }

    }
}
