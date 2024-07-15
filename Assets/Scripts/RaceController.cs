using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RaceController : NetworkBehaviour
{
    public static RaceController instance;

    [Header("Prefabs of objects to spawn")]
    [SerializeField] GameObject carPrefab; //Car to spawn
    [SerializeField] GameObject carCameraPrefab;

    [SerializeField] GameObject[] spawnPoints;
    [SerializeField] RankingManager rankingManager;
    [SerializeField] CheckpointLogic checkpointLogic;

    private void Awake()
    {
        instance = this;
    }

    public Dictionary<int, GameObject> carGameobjects = new Dictionary<int, GameObject>();
    public Dictionary<int, CarController> carController = new Dictionary<int, CarController>();

    private Dictionary<ulong, int> clientIDCarIdDicrionary = new Dictionary<ulong, int>();


    private List<int> finishedPlayers = new List<int>();

    [SerializeField] private Button spawn;
    [SerializeField] private CameraController cameraController;


    [Header("UI")]
    [SerializeField] private EOGUI eogui;

    [SerializeField] private FinishTimer finishTimer;

    [SerializeField] private PlayerColors playerColors;



    // Start is called before the first frame update
    void Start()
    {
        SpawnCars();
        cameraController.onMapShown = OnMapShown;
        cameraController.ShowMap();

        eogui.gameObject.SetActive(false);
        HUD.instance.gameObject.SetActive(false);


        checkpointLogic.onFinish += CarFinishedServerRpc;
    }

    private void Update()
{
        List<int> rank = rankingManager.CalculateRankings();
        int ranking = 1;
        foreach (int x in rank)
        {
            if (carController[x].IsOwner)
            {
                HUD.instance.UpdateRank(ranking);
                break;
            }
            ranking++;
        }
    }

    public void OnMapShown()
    {
        cameraController.SwitchToCarCamera(clientIDCarIdDicrionary[NetworkManager.LocalClientId]);
        if (IsServer)
        {
            StartCoroutine(CountDown());
        }
    }

    IEnumerator CountDown()
    {
        SetCountDownToClientRpc(3);
        yield return new WaitForSeconds(1f);
        SetCountDownToClientRpc(2);
        yield return new WaitForSeconds(1f);
        SetCountDownToClientRpc(1);
        yield return new WaitForSeconds(1f);
        SetCountDownToClientRpc(0);
        StartRace();
    }

    [ClientRpc]
    public void SetCountDownToClientRpc(int value)
    {
        
    }




    public void StartRace()
    {
        for (int i = 0; i < carController.Count; i++)
        {
            carController[i].StartDrivingClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CarFinishedServerRpc(int id)
    {
        if (finishedPlayers.Count == 0)
        {
            StartTimerClientRpc();
        }


        if (!finishedPlayers.Contains(id))
        {
            finishedPlayers.Add(id);
            carController[id].StopDrivingClientRpc();
        }


        if (finishedPlayers.Count == carController.Count)
        {
            RaceFinishedServerRpc();
            StopEarlyClientRpc();
        }
    }

    [ClientRpc]
    public void StopEarlyClientRpc()
    {
        finishTimer.StopTimer();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RaceFinishedServerRpc()
    {
        List<int> rank = rankingManager.CalculateRankings();

        foreach (int x in rank)
        {
            if (!finishedPlayers.Contains(x))
            {
                finishedPlayers.Add(x);
                carController[x].StopDrivingClientRpc();
            }
        }

        string finalRankings = string.Join("\n", finishedPlayers.Select((playerIndex, index) => $"{index + 1}. {PlayerColors.instance.getColor(playerIndex).name}"));
        ShowFinalRankClientRpc(finalRankings);
    }



    [ClientRpc]
    private void ShowFinalRankClientRpc(string result)
    {
        eogui.gameObject.SetActive(true);
        HUD.instance.gameObject.SetActive(false);
        eogui.ShowAndSetRankings(result);
    }


    //only on server
    private void SpawnCars()
    {
        if (IsServer) {
            int i = 0;

            foreach (NetworkClient x in NetworkManager.Singleton.ConnectedClientsList)
            {
                clientIDCarIdDicrionary[x.ClientId] = i;
                GameObject newCar = Instantiate(carPrefab, spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
                newCar.GetComponent<NetworkObject>().Spawn();
                newCar.GetComponent<CarController>().setSpawnInformationClientRpc(i);
                newCar.GetComponent<NetworkObject>().ChangeOwnership(x.ClientId);
                i++;
            }
        }
    }


    [ClientRpc]
    private void StartTimerClientRpc()
    {
        finishTimer.StartTimer();
        if (IsServer)
        {
            finishTimer.onFinish += RaceFinishedServerRpc;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        foreach(GameObject x in spawnPoints)
        {
            Gizmos.DrawCube(x.transform.position,Vector3.one);
        }
    }

    public void RegisterCar(int carId, CarController carController)
    {
        this.carController[carId] = carController;
        this.carGameobjects[carId] = carController.gameObject;

        GameObject carCameraGameObject = Instantiate(carCameraPrefab, carController.transform.position, carController.transform.rotation);
        CarCamera carCamera = carCameraGameObject.GetComponent<CarCamera>();
        carCamera.Setup(carController.GetComponent<CarCameraTarget>().GetCarCameraPoints());
        cameraController.AddCarCamera(carId, carCamera);

    }
}