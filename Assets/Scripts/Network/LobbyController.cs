using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyController : NetworkBehaviour
{
    [SerializeField] private string[] scenes;
    [SerializeField] private int selectedMapID;
    private void Start()
    {
        if (!IsServer) return;
        
    }


    public void SelectMap(int mapID) {
        if (!IsServer) return;

        selectedMapID = mapID;
    }

    public void StartRace()
    {
        if (!IsServer) return;

        NetworkManager.SceneManager.LoadScene(scenes[selectedMapID], LoadSceneMode.Additive);
    }
}
