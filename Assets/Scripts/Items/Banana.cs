using UnityEngine;

public class Banana : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CarController carController))
        {
                // Apply the effect to the other car
                carController.spinCar();
                Destroy(gameObject);
        }
    }
}
