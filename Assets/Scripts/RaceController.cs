using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaceController : NetworkBehaviour
{
    public static RaceController instance;

    [Header("Prefabs of objects to spawn")]
    [SerializeField] GameObject carPrefab; //Car to spawn
    [SerializeField] GameObject carCameraPrefab;

    [Header("")]
    [SerializeField] GameObject[] spawnPoints;
    [SerializeField] RankingManager rankingManager;
    [SerializeField] CheckpointLogic checkpointLogic;

    [Header("Sound")]
    [SerializeField] AudioSource countdownSound;
    [SerializeField] AudioSource backgroundMusic;

    private void Awake()
    {
        instance = this;
    }

    public Dictionary<int, GameObject> carGameobjects = new Dictionary<int, GameObject>();
    public Dictionary<int, CarController> carController = new Dictionary<int, CarController>();

    private Dictionary<ulong, int> clientIDCarIdDicrionary = new Dictionary<ulong, int>();


    private List<int> finishedPlayers = new List<int>();

    [SerializeField] private CameraController cameraController;


    [SerializeField] private FinishTimer finishTimer;


    // Start is called before the first frame update
    void Start()
    {
        SpawnCars();
        cameraController.onMapShown = OnMapShown;
        cameraController.ShowMap();

        checkpointLogic.onFinish += CarFinishedServerRpc;

        // if (IsHost)
        // {
        //     backgroundMusic.Play();
        // }
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
        SetCountDownToClientRpc(0);
        yield return new WaitForSeconds(1f);
        SetCountDownToClientRpc(1);
        yield return new WaitForSeconds(1f);
        SetCountDownToClientRpc(2);
        yield return new WaitForSeconds(1f);
        SetCountDownToClientRpc(3);
        StartRace();
        yield return new WaitForSeconds(1f);
        SetCountDownToClientRpc(4);
    }

    [ClientRpc]
    public void SetCountDownToClientRpc(int value)
    {
        HUD.instance.toggleVisibility(true);
        if (value == 0)
            countdownSound.Play();
        HUD.instance.SetCountdownToValue(value);
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
        if(rank.Count >= 3)
            ResultSetClientRpc(rank[0], rank[1], rank[2]);
        else if (rank.Count >= 2)
            ResultSetClientRpc(rank[0], rank[1], -1);
        else if (rank.Count >= 1)
            ResultSetClientRpc(rank[0], -1, -1);
        NetworkManager.SceneManager.LoadScene("EndOfGame", LoadSceneMode.Single);
        NetworkManager.SceneManager.OnLoadComplete += DestroyCar;
    }

    [Rpc(SendTo.Everyone)]
    public void ResultSetClientRpc(int rank1, int rank2, int rank3)
    {
        ResultNet.instance.LastRaceResult = new List<int> { rank1, rank2, rank3};
    }

    public void DestroyCar(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        NetworkManager.SceneManager.OnLoadComplete -= DestroyCar;
        foreach (GameObject x in carGameobjects.Values)
        {
            Destroy(x);
        }
    }


    //only on server
    private void SpawnCars()
    {
        if (IsServer) {
            int i = 0;

            foreach (NetworkClient x in NetworkManager.Singleton.ConnectedClientsList)
            {
                AddClientIDToDictonaryClientRpc(x.ClientId, i);
                GameObject newCar = Instantiate(carPrefab, spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
                newCar.GetComponent<NetworkObject>().Spawn();
                newCar.GetComponent<CarController>().SetSpawnInformationClientRpc(i, x.ClientId);
                newCar.GetComponent<NetworkObject>().ChangeOwnership(x.ClientId);
                i++;
            }
        }
    }

    [ClientRpc]
    public void AddClientIDToDictonaryClientRpc(ulong clientID, int carID)
    {

        clientIDCarIdDicrionary[clientID] = carID;
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