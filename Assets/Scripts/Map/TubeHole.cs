using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeHole : MonoBehaviour
{
    [SerializeField] float strength;

    private void OnTriggerStay(Collider other)
    {
        if(other.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
        {
            Vector3 diff = (transform.position - other.transform.position);
            float value = strength / diff.magnitude;
            rigidbody.AddForce((diff.normalized - transform.up) * value, ForceMode.Acceleration);
        }
    }
}
