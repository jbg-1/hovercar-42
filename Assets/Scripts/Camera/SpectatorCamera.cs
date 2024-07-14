using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpectatorCamera : MonoBehaviour
{
    public enum CameraFocus
    {
        Map, Car
    }

    [SerializeField] protected Camera camera;
    [SerializeField] protected AudioListener audioListener;
    [SerializeField] protected CameraFocus focus = CameraFocus.Map;
    public OnMapShown onMapShown;

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

    public virtual void SetFocus(CameraFocus focus)
    {
        this.focus = focus;
    }

    public delegate void OnMapShown();

}
