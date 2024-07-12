using System.Collections;
using UnityEngine;

public class FreezeItem : Item
{
    public override void useItem(ItemController itemController)
    {
        Debug.Log("FreezeItem used");
        CarController usingCar = itemController.gameObject.GetComponent<CarController>();

        // Freeze all other cars
        foreach (var car in RaceController.instance.carController.Values)
        {
            Debug.Log("Freeze Item on all cars");
            if (car != usingCar)
            {
                car.FreezeClientRpc();
            }
        }

        // Start coroutine to unfreeze all other cars after duration
        itemController.StartCoroutine(UnfreezeCarsAfterDelay(usingCar, 2f, 10f));
    }

    private IEnumerator UnfreezeCarsAfterDelay(CarController usingCar, float delay, float boostAmount)
    {
        yield return new WaitForSeconds(delay);

        foreach (var car in RaceController.instance.carController.Values)
        {
            if (car != usingCar)
            {
                car.UnfreezeClientRpc();
            }
        }

        // Boost the car that used the item after unfreezing others
        usingCar.Boost(boostAmount);
    }
}

