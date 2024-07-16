using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float radius = 5f;


    private void OnCollisionEnter(Collision collision)
    {
         Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

         foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                rigidbody.AddForce((rigidbody.transform.position - transform.position).normalized * explosionForce, ForceMode.VelocityChange);
            }
        }
         //TODO EFFECT

         Destroy(gameObject);
    }
}
