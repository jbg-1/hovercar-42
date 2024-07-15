using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpectatorCamera : BaseCamera
{
    public enum CameraFocus
    {
        Map, Car
    }

    [SerializeField] protected CameraFocus focus = CameraFocus.Map;
    public OnMapShown onMapShown;

    private void Awake()
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
