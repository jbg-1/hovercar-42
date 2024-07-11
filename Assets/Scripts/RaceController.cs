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

    [SerializeField] MiniMap miniMap;

    public struct PlayerInformation
    {
        public CarController carController;
        public GameObject carGameObject;
        public int id;
        //public PlayerColors.PlayerColor playerColor;
    }

    public PlayerInformation[] playerInformation;
    private List<int> finishedPlayers = new List<int>();

    [SerializeField] private Button spawn;

    [Header("UI")]
    [SerializeField] private EOGUI eogui;
    [SerializeField] private HUD hud;

    [SerializeField] private FinishTimer finishTimer;

    [SerializeField] private PlayerColors playerColors;

    private PlayerColors.PlayerColor[] colors;


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
        colors = playerColors.GetAllColors().ToArray();
    }

    private void Update()
    {
        if (IsHost) { //Since PlayerInformation is not accessible from the clients
            List<int> rank = rankingManager.CalculateRankings();
            int ranking = 1;
            foreach (int x in rank)
            {
                if (playerInformation[x].carController.IsOwner)
                {
                    hud.UpdateRank(ranking);
                    break;
                }
                ranking++;
            }
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
        List<int> rank = rankingManager.CalculateRankings();

        foreach (int x in rank)
        {
            if (!finishedPlayers.Contains(x))
            {
                finishedPlayers.Add(x);
                playerInformation[x].carController.StopDrivingClientRpc();
            }
        }

        string finalRankings = string.Join("\n", finishedPlayers.Select((playerIndex, index) => $"{index + 1}. {playerInformation[playerIndex].carController.playerColor.name}"));
        ShowFinalRankClientRpc(finalRankings);
    }



    [ClientRpc]
    private void ShowFinalRankClientRpc(string result)
    {
        eogui.gameObject.SetActive(true);
        hud.gameObject.SetActive(false);
        eogui.ShowAndSetRankings(result);
    }


    private void SpawnCars()
    {
        int i = 0;
        playerInformation = new PlayerInformation[NetworkManager.Singleton.ConnectedClientsList.Count];

        foreach (NetworkClient x in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject newCar = Instantiate(carPrefab, spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
            PlayerColors.PlayerColor color = colors[i];

            PlayerInformation newPlayerInformation = new PlayerInformation()
            {
                carGameObject = newCar,
                carController = newCar.GetComponent<CarController>(),
                id = i
            };

            newPlayerInformation.carController.carId = i;
            //newPlayerInformation.playerColor = color;
            newPlayerInformation.carController.SetColor(color);
            playerInformation[i] = newPlayerInformation;

            Transform kartTransform = newCar.transform.Find("Kart");
            if (kartTransform != null)
            {
                Transform helmTransform = kartTransform.Find("Helm");
                if (helmTransform != null)
                {
                    Material helmMaterial = color.material;
                    Renderer helmRenderer = helmTransform.GetComponent<Renderer>();
                    if (helmRenderer != null)
                    {
                        helmRenderer.material = helmMaterial;
                    }
                }
            }

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
}
