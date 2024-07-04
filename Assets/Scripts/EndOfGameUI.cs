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

    [ClientRpc]
    public void ActivateEndOfGameUIClientRpc()
    {
        hudCanvas.SetActive(false);
        eogCanvas.SetActive(true);
    }

    [ClientRpc]
    public void ShowAndSetRankingsClientRpc(string rankings)
    {
        rankingText.text = rankings;
    }

    [ClientRpc]
    public void RestartGameClientRpc()
    {
        // Implementation to restart the game
    }
}