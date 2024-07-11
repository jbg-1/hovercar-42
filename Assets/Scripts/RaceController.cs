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

    [SerializeField] GameObject carPrefab; //Car to spawn
    [SerializeField] GameObject[] spawnPoints;
    [SerializeField] RankingManager rankingManager;
    [SerializeField] CheckpointLogic checkpointLogic;

    private void Awake()
    {
        instance = this;
    }

    public GameObject[] carGameobjects { get; private set; }
    public CarController[] carController { get; private set; }

    private List<int> finishedPlayers = new List<int>();

    [SerializeField] private Button spawn;

    [Header("UI")]
    [SerializeField] private EOGUI eogui;

    [SerializeField] private FinishTimer finishTimer;

    [SerializeField] private PlayerColors playerColors;

    private PlayerColors.PlayerColor[] colors;


    // Start is called before the first frame update
    void Start()
    {
        eogui.gameObject.SetActive(false);
        HUD.instance.gameObject.SetActive(false);

        spawn.onClick.AddListener(() =>
        {
            SpawnCars();
        });

        checkpointLogic.onFinish += CarFinishedServerRpc;
        colors = playerColors.GetAllColors().ToArray();
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



    public void StartRace()
    {
        HUD.instance.gameObject.SetActive(true);
    }

    [ServerRpc]
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


        if (finishedPlayers.Count == carController.Length)
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

    [ServerRpc]
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

        string finalRankings = string.Join("\n", finishedPlayers.Select((playerIndex, index) => $"{index + 1}. {carController[playerIndex].playerColor.name}"));
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
            carGameobjects = new GameObject[NetworkManager.Singleton.ConnectedClientsList.Count];
            carController = new CarController[NetworkManager.Singleton.ConnectedClientsList.Count];


            foreach (NetworkClient x in NetworkManager.Singleton.ConnectedClientsList)
            {
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

    public void registerCar(int id, CarController carController)
    {
        this.carController[id] = carController;
        this.carGameobjects[id] = carController.gameObject;
    }
}