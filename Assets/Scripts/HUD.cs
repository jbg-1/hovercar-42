using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text timerText;
    public Text roundsText;
    public Text rankText;

    private CarController carController;

    public void SetCarController(CarController controller)
    {
        carController = controller;
    }

    private void Update()
    {
        if (carController != null)
        {
            roundsText.text = "Round " + (carController.RoundsCompleted.Value + 1) + "/3";
            rankText.text = "" + carController.Rank.Value;
        }
    }

    public void UpdateRank(int rank)
    {
        rankText.text = "" + rank;
    }

    public void UpdateTimer(string time)
    {
        timerText.text = time;
    }
}
