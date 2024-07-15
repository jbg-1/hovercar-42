using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class CarCameraTarget : MonoBehaviour
{
    [SerializeField] private CarController carController;
    [SerializeField] private CarCamera.CameraPoints carCameraPoints;
    [SerializeField] private Transform cameraParent;
    [SerializeField] private Rigidbody carRigdbody;

    private void LateUpdate()
    {
        if (carController.isDriving)
        {
            cameraParent.rotation = Quaternion.LookRotation(carRigdbody.velocity);
        }
        else
        {
            cameraParent.rotation = Quaternion.LookRotation(transform.forward);
        }
    }

    public CarCamera.CameraPoints GetCarCameraPoints()
    {
        return carCameraPoints;
    }
}
