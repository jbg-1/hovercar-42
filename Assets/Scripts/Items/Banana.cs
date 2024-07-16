using UnityEngine;

public class Banana : MonoBehaviour
{
    [SerializeField] private GameObject toDestroy;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CarController carController))
        {
                // Apply the effect to the other car
                carController.SpinCar();
                Destroy(toDestroy);
        }
    }
}
