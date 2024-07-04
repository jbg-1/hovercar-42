using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RaceController : NetworkBehaviour
{
    [SerializeField] GameObject carPrefab;
    [SerializeField] GameObject[] spawnPoints;

    [SerializeField] RankingManager rankingManager;
    [SerializeField] CheckpointLogic checkpointLogic;


    public struct PlayerInformation
    {
        public CarController carController;
        public GameObject carGameObject;
        public int id;
    }

    public PlayerInformation[] playerInformation;
    private List<int> finishedPlayers = new List<int>();

    [SerializeField] private Button spawn;

    [Header("UI")]
    [SerializeField] private EOGUI eogui;
    [SerializeField] private HUD hud;

    [SerializeField] private FinishTimer finishTimer;




    // Start is called before the first frame update
    void Start()
    {
        eogui.gameObject.SetActive(false);
        hud.gameObject.SetActive(false);

        spawn.onClick.AddListener(() =>
        {
            SpawnCars();
        });

        checkpointLogic.onFinish += CarFinishedServerRpc;
    }

    private void Update()
    {
        List<int> rank = rankingManager.CalculateRankingsServerRpc();
        int ranking = 1;
        foreach (int x in rank) {
            if (playerInformation[x].carController.IsOwner)
            {
                hud.UpdateRank(ranking);
                break;
            }
            ranking++;
        }
    }



    public void StartRace()
    {
        hud.gameObject.SetActive(true);
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
            playerInformation[id].carController.StopDrivingClientRpc();
        }


        if (finishedPlayers.Count == playerInformation.Length)
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
        eogui.gameObject.SetActive(true);
        hud.gameObject.SetActive(false);

        List<int> rank = rankingManager.CalculateRankingsServerRpc();

        foreach (int x in rank)
        {
            if (!finishedPlayers.Contains(x))
            {
                finishedPlayers.Add(x);
                playerInformation[x].carController.StopDrivingClientRpc();
            }
        }

        string finalRankings = string.Join("\n", finishedPlayers.Select((player, index) => $"Player {player} finished in position {index + 1}"));
        ShowFinalRankClientRpc(finalRankings);
    }

    [ClientRpc]
    private void ShowFinalRankClientRpc(string result) {
        eogui.gameObject.SetActive(true);
        eogui.ShowAndSetRankingsClientRpc(result);
    }

    
    private void SpawnCars()
    {
        int i = 0;
        playerInformation = new PlayerInformation[NetworkManager.Singleton.ConnectedClientsList.Count];
        foreach (NetworkClient x in NetworkManager.Singleton.ConnectedClientsList) { 
            GameObject newCar = Instantiate(carPrefab, spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
            PlayerInformation newPlayerInformation = new PlayerInformation()
            {
                carGameObject = newCar,
                carController = newCar.GetComponent<CarController>(),
                id = i
            };
            newPlayerInformation.carController.carId = i;
            playerInformation[i] = newPlayerInformation;
            newCar.GetComponent<NetworkObject>().Spawn();
            newCar.GetComponent<NetworkObject>().ChangeOwnership(x.ClientId);
            i++;
        }

        rankingManager.playerInformation = playerInformation;

    }

    [ClientRpc]
    private void StartTimerClientRpc()
    {
        finishTimer.StartTimer();
        if (IsServer) {
            finishTimer.onFinish += RaceFinishedServerRpc;
        }
    }
}
