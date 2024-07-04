using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text timerText;
    public Text roundsText;
    public Text rankText;

    public void UpdateRank(int rank)
    {
        rankText.text = "" + rank;
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
