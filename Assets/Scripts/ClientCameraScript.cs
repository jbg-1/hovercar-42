using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ClientCameraInformation{
    public GameObject cameraPositionAim;
    public GameObject lookAt;
}

public class ClientCameraScript : MonoBehaviour
{
    public float speed = 1;
    public static ClientCameraScript activeCamera;
    public ClientCameraInformation clientCameraInformation;

    private void Awake()
    {
        activeCamera = this;
    }

    private void LateUpdate()
    {
        if(clientCameraInformation.cameraPositionAim != null)
            transform.Translate(speed * Time.deltaTime * (clientCameraInformation.cameraPositionAim.transform.position - transform.position), Space.World);
        if (clientCameraInformation.lookAt != null)
            transform.LookAt(clientCameraInformation.lookAt.transform.position, clientCameraInformation.lookAt.transform.up);
    }

    public void SetClientCameraInformation(ClientCameraInformation clientCameraInformation)
    {
        this.clientCameraInformation = clientCameraInformation;
    }

}
