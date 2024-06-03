using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class LobbyPlayerListItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void SetPlayerId(ulong id)
    {
        text.SetText("Player " + id);
    }
}
