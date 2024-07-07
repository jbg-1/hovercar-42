using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundsText;
    public TextMeshProUGUI rankText;

    public void UpdateRank(int rank)
    {
        rankText.text = rank + ".";
    }

    public void UpdateTimer(string time)
    {
        timerText.text = time;
    }

    public void UpdateRounds(int round)
    {
        roundsText.text = round + "/3";
    }
}
