using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net;
using Unity.Netcode.Transports.UTP;
using System;



public class LobbyManager : NetworkBehaviour
{
    [System.Serializable]
    public struct LevelInformation
    {
        public string displayName;
        public string sceneName;
        public Sprite displayImage;
    }

    [SerializeField] private LevelDisplay levelDisplay;


    [SerializeField] private LevelInformation[] availableLevel;

    private int currentlySelectedLevel = -1;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public void SelectLevel(int id)
    {
        currentlySelectedLevel = id;
        SelectLevelClientRpc(id);
    }

    [ClientRpc]
    public void SelectLevelClientRpc(int id)
    {
        if (id < 0)
        {
            levelDisplay.SetLevelInformation(null);
        }
        else {
            levelDisplay.SetLevelInformation(availableLevel[id]);
        }
    }

    public void LoadRace() {
        if (NetworkManager.IsServer)
        {
            if(currentlySelectedLevel >= 0)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                levelDisplay.SetStatus("Scene wird geladen.");
                NetworkManager.SceneManager.LoadScene(availableLevel[currentlySelectedLevel].sceneName, LoadSceneMode.Single);
            }
            else
            {
                levelDisplay.SetStatus("Es wurde noch keine Strecke ausgewählt");
            }
        }
        else {
            levelDisplay.SetStatus("Nur der Server/Host kann das Race starten.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer) {
            SelectLevelClientRpc(currentlySelectedLevel);
        }
        if(NetworkManager.LocalClientId == clientId)
        {
            levelDisplay.SetStatus("Verbindung zum Server wurde hergestellt.");
        }
    }

    //Start
    public void StartAsClient(string ip)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, 7777);
        NetworkManager.StartClient();
    }

    public void StartAsHost()
    {
        NetworkManager.StartHost();
    }

}
