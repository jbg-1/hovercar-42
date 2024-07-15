using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCamera : BaseCamera
{

    [System.Serializable]
    public struct CameraPoints
    {
        public Transform cameraTartgetPosition;
        public Transform lookAtPosition;
        public Transform upVector;
    }
    public CameraPoints? cameraPoints;

    private Vector3 cuurentLookAtPosiotion = Vector3.zero;
    private Vector3 currentupVector = Vector3.up;

    private Vector3 velocityLooAt;
    private Vector3 velocityUpVector;
    private Vector3 velocity;


    private void LateUpdate()
    {
        if(cameraPoints != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, cameraPoints.Value.cameraTartgetPosition.position, ref velocity, 0.3f);
            cuurentLookAtPosiotion = Vector3.SmoothDamp(cuurentLookAtPosiotion, cameraPoints.Value.lookAtPosition.position,ref velocityLooAt, 0.3f);
            currentupVector = Vector3.SmoothDamp(currentupVector, cameraPoints.Value.upVector.up, ref velocityUpVector, 0.1f);

            transform.LookAt(cuurentLookAtPosiotion, currentupVector);
        }
    }

    public void Setup(CameraPoints? cameraPoints)
    {
        this.cameraPoints = cameraPoints;
    }

}
