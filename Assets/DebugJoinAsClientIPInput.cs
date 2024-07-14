using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugJoinAsClientIPInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private LobbyManager lobbyManager;


    public void OnInput()
    {
        lobbyManager.StartAsClient(inputField.text);
    }
}
