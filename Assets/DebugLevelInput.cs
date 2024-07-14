using Palmmedia.ReportGenerator.Core.Common;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugLevelInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private LobbyManager lobbyManager;

    public void OnInput()
    {
        lobbyManager.SelectLevel(inputField.text.ParseLargeInteger());
    }
}
