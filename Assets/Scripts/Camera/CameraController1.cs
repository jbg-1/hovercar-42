using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public enum CameraStatus
    {
        Spectator, CarFollow
    }

    private CameraStatus cameraStatus = CameraStatus.Spectator;

    public SpectatorCamera[] spectatorCameras;

    private int currentMapCam;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ShowMap();
    }

    public void ShowMap()
    {
        currentMapCam = 0;
        spectatorCameras[0].onMapShown += MapShown;
        spectatorCameras[0].SetFocus(SpectatorCamera.CameraFocus.Map);
        spectatorCameras[0].EnableCamera();
    }

    private void MapShown()
    {
        spectatorCameras[currentMapCam].onMapShown -= MapShown;
        spectatorCameras[currentMapCam].DisableCamera();

        currentMapCam++;
        if (currentMapCam < spectatorCameras.Length)
        {
            spectatorCameras[currentMapCam].onMapShown += MapShown;
            spectatorCameras[currentMapCam].SetFocus(SpectatorCamera.CameraFocus.Map);
            spectatorCameras[currentMapCam].EnableCamera();
        }
    }
}
