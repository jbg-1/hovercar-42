using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text checkpointText;
    public Text roundsText;

    private void Update()
    {
        //checkpointText.text = "Last Checkpoint: " + CarController.LastCheckpointCollected;
        //roundsText.text = "Round " + (CarController.RoundsCompleted + 1) + "/3";
    }
}