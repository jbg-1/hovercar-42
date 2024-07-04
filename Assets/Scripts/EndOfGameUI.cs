using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EOGUI : NetworkBehaviour
{
    public Text rankingText;

    public void ShowAndSetRankingsClientRpc(string rankings)
    {
        rankingText.text = rankings;
    }

}