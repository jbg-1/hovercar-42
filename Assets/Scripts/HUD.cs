using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
  public TextMeshProUGUI timerText;
  public TextMeshProUGUI roundsText;
  public TextMeshProUGUI rankText;
  public Text wrongDirectionText;

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

  public void ToggleWrongDirectionText(bool show)
  {
    wrongDirectionText.gameObject.SetActive(show);
  }
}
