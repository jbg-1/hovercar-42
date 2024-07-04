using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EOGUI : NetworkBehaviour
{
  public Text rankingText;

  [ClientRpc]
  public void ShowAndSetRankingsClientRpc(string rankings)
  {
    rankingText.text = rankings;
  }
}