using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EOGUI : NetworkBehaviour
{
    public Text rankingText;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject eogCanvas; 
    [SerializeField] private GameObject hudCanvas; 


    private void Awake()
    {
        restartButton.onClick.AddListener(() =>
        {
            RestartGameClientRpc();
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShowEndOfGameUIAndSetRankingsServerRpc(string rankings)
    {
        ShowEndOfGameUIAndSetRankingsClientRpc(rankings);
    }

    [ClientRpc]
    private void ShowEndOfGameUIAndSetRankingsClientRpc(string rankings)
    {
        hudCanvas.SetActive(false);
        eogCanvas.SetActive(true);
        rankingText.text = rankings;
    }

    [ClientRpc]
    public void RestartGameClientRpc()
    {
        // Implementation to restart the game
    }
}