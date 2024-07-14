using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public static CarCamera instance;

    public enum CameraStatus
    {
        Spectator, CarFollow
    }

    private CameraStatus cameraStatus = CameraStatus.Spectator;


    



    [SerializeField] private Transform cameraTartgetPosition;
    [SerializeField] private Transform lookAtPosition;
    private Vector3 cuurentLookAtPosiotion = Vector3.zero;
    private Vector3 velocity;
    private Vector3 velocityLooAt;
    private Vector3 velocityUpVector;

    [SerializeField] private Transform upVector;
    private Vector3 currentupVector;


    private void Awake()
    {
        instance = this;
    }

    private void LateUpdate()
    {
        if(cameraTartgetPosition != null && lookAtPosition != null && upVector != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, cameraTartgetPosition.position, ref velocity, 0.3f);
            cuurentLookAtPosiotion = Vector3.SmoothDamp(cuurentLookAtPosiotion, lookAtPosition.position,ref velocityLooAt, 0.3f);
            currentupVector = Vector3.SmoothDamp(currentupVector, upVector.up, ref velocityUpVector, 0.1f);

            transform.LookAt(cuurentLookAtPosiotion, currentupVector);
        }
    }

    public void Setup(Transform cameraTartgetPosition, Transform lookAtPosition, Transform upVector)
    {
        this.upVector = upVector;
        this.cameraTartgetPosition = cameraTartgetPosition;
        this.lookAtPosition = lookAtPosition;
    }

}
