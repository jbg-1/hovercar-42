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

    private void Start()
    {
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

    private void OnClientConnectedCallback(ulong clientId)
    {
        serverUI.SetListItems(NetworkManager.Singleton.ConnectedClientsIds);

        SelectMapClientRpc(selectedMap);
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        serverUI.SetListItems(NetworkManager.Singleton.ConnectedClientsIds);
    }


    public void SelectMap(int mapID) {
        selectedMap = mapID;
        SelectMapClientRpc(mapID); 
    }

    public void LoadLevel()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(level[selectedMap].sceneName,LoadSceneMode.Single);
    }

    [ClientRpc]
    private void SelectMapClientRpc(int mapID)
    {
        clientUI.Setup(level[mapID]);
    }
}
