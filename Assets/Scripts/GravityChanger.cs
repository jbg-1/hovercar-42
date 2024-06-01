using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityChanger : MonoBehaviour { 

    [SerializeField] bool useGravityTransformDown = true;
    [SerializeField] Vector3 gravityDirection = -Vector3.up;

    private void Start()
    {
        if (useGravityTransformDown)
        {
            gravityDirection = -transform.up;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController carControllerVar;
        if(other.TryGetComponent<CarController>(out carControllerVar))
        {
            //carControllerVar.ChangeGravityDirectionTo(gravityDirection);
        }
    }
}
