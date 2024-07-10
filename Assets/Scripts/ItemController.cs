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

    // Call useItem method when the space key is pressed
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key was pressed.");
            UseItem();
        }
    }

    public void collectItem() {
    
        if(item == null){
            //generate a random number between 1 and 4
            int random = Random.Range(1, 5);
            //switch case to determine which item to generate
            switch (random)
            {
                case 1:
                    item = new BoostItem();
                    break;
                case 2:
                    item = new FreezeItem();
                    break;
                case 3:
                    item = new FreezeItem();
                    break;
                case 4:
                    item = new FreezeItem();
                    break;
            }
        }

    }
}
