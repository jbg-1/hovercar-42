using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCamera : MonoBehaviour
{

    [SerializeField] protected Camera camera;
    [SerializeField] protected AudioListener audioListener;

    private void Awake()
    {
        camera.enabled = false;
        audioListener.enabled = false;
    }

    public virtual void EnableCamera()
    {
        camera.enabled = true;
        audioListener.enabled = true;
    }

    public virtual void DisableCamera()
    {
        camera.enabled = false;
        audioListener.enabled = false;
    }
}
