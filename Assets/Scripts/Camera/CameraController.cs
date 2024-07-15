using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpectatorCamera;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    public OnMapShown onMapShown;
    public enum CameraStatus
    {
        Spectator, CarFollow
    }

    private CameraStatus cameraStatus = CameraStatus.Spectator;

    [SerializeField] private SpectatorCamera[] spectatorCameras;
    [SerializeField] private Dictionary<int, CarCamera> carCameras = new Dictionary<int, CarCamera>();

    [SerializeField] BaseCamera currentActiveCamera;

    private int currentMapCam;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
    }

    public void ShowMap()
    {
        currentMapCam = 0;
        spectatorCameras[0].onMapShown += MapShown;
        SwitchToSpectatorCam(0, CameraFocus.Map);
    }

    private void MapShown()
    {

        spectatorCameras[currentMapCam].onMapShown -= MapShown;
        currentActiveCamera.DisableCamera();

        currentMapCam++;
        if (currentMapCam < spectatorCameras.Length)
        {
            spectatorCameras[currentMapCam].onMapShown += MapShown;
            SwitchToSpectatorCam(currentMapCam,CameraFocus.Map);
        }
        else {
            onMapShown();
        }
    }

    public delegate void OnMapShown();

    public void AddCarCamera(int carId, CarCamera carCamera) {
        carCameras[carId] = carCamera;
    }

    public void SwitchToCarCamera(int carId)
    {
        if(currentActiveCamera != null)
            currentActiveCamera.DisableCamera();
        currentActiveCamera = carCameras[carId];
        currentActiveCamera.EnableCamera();
    }

    public void SwitchToSpectatorCam(int cameraId, SpectatorCamera.CameraFocus focus)
    {
        if (currentActiveCamera != null)
            currentActiveCamera.DisableCamera();
        spectatorCameras[cameraId].SetFocus(focus);
        currentActiveCamera = spectatorCameras[cameraId];
        currentActiveCamera.EnableCamera();
    }
}
