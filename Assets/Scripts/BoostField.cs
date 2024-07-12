using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostField : MonoBehaviour
{
    [SerializeField] private float boostValue = 1f;

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
        {
            rigidbody.AddRelativeTorque(new Vector3(0,Vector3.Dot(transform.up,rigidbody.transform.right) * 30, 0), ForceMode.Acceleration);
            rigidbody.AddForce(transform.up * boostValue, ForceMode.Acceleration);
        }
    }
}
