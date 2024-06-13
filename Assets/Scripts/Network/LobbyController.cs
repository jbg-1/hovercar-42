using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


enum ClientServer
{
    Client,
    Server
}

public class LobbyController : NetworkBehaviour
{
    [SerializeField] private ClientServer clientOrServer = ClientServer.Client;

    [SerializeField] private LevelDescription[] level;

    [SerializeField] ServerUI serverUI;
    [SerializeField] ClientUI clientUI;

    private int selectedMap = 0;

    private SortedSet<ulong> clientIDList = new SortedSet<ulong>();

    private void Start()
    {
        NetworkObject.DestroyWithScene = true;
        serverUI.gameObject.SetActive(clientOrServer == ClientServer.Server);
        clientUI.gameObject.SetActive(clientOrServer == ClientServer.Client);
        if (clientOrServer == ClientServer.Server)
        {
            NetworkManager.Singleton.StartServer();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            serverUI.SetLevelItems(level);
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    private void Update()
    {
        Cursor.visible = clientOrServer == ClientServer.Server;
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        clientIDList.Add(clientId);
        serverUI.SetListItems(clientIDList);

        SelectMapClientRpc(selectedMap);
        serverUI.SetRaceStartable(clientIDList.Count > 0);
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        clientIDList.Remove(clientId);
        serverUI.SetListItems(clientIDList);
        serverUI.SetRaceStartable(clientIDList.Count > 0);
    }


    public void SelectMap(int mapID) {
        selectedMap = mapID;
        SelectMapClientRpc(mapID); 
    }

    public void LoadLevel()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        NetworkManager.Singleton.SceneManager.LoadScene(level[selectedMap].sceneName,LoadSceneMode.Single);
    }

    [ClientRpc]
    private void SelectMapClientRpc(int mapID)
    {
        clientUI.Setup(level[mapID]);
    }
}
