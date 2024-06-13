using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCameraScript : MonoBehaviour
{
    public static CarCameraScript instance;

    [SerializeField] private Transform cameraTartgetPosition;
    [SerializeField] private Transform lookAtPosition;
    private Vector3 cuurentLookAtPosiotion = Vector3.zero;
    private Vector3 velocity;
    private Vector3 velocityLooAt;


    private void Awake()
    {
        instance = this;
    }

    private void LateUpdate()
    {
        if(cameraTartgetPosition != null && lookAtPosition != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, cameraTartgetPosition.position, ref velocity, 0.3f);
            cuurentLookAtPosiotion = Vector3.SmoothDamp(cuurentLookAtPosiotion, lookAtPosition.position,ref velocityLooAt, 0.3f);
            transform.LookAt(cuurentLookAtPosiotion, Vector3.up);
        }
    }

    public void Setup(Transform cameraTartgetPosition, Transform lookAtPosition)
    {
        this.cameraTartgetPosition = cameraTartgetPosition;
        this.lookAtPosition = lookAtPosition;
    }
}
