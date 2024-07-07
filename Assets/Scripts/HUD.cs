using UnityEngine;
using TMPro;
using System.Collections;

public class HUD : MonoBehaviour
{
  public TextMeshProUGUI timerText;
  public TextMeshProUGUI roundsText;
  public TextMeshProUGUI rankText;
  public TextMeshProUGUI wrongDirectionText;
  private Coroutine currentFade;
  public float fadeSpeed = 2.0f;
  
  void Start()
    {
        SetTextAlpha(0f);
    }

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
    roundsText.text = "Runde " + round + "/3";
  }

  public void ToggleWrongDirectionText(bool show)
  {
    if (show)
    {
      StartFading();
    }
    else
    {
      StopFading();
    }
  }

    private void StartFading()
    {
        if (currentFade != null)
            StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeInOut());
    }

    private void StopFading()
    {
        SetTextAlpha(0f); 
        if (currentFade != null)
            StopCoroutine(currentFade);
    }

    private IEnumerator FadeInOut()
    {
        while (true)
        {
            yield return StartCoroutine(FadeTextIn());
            yield return StartCoroutine(FadeTextOut());
        }
    }

    private IEnumerator FadeTextIn()
    {
        Color color = wrongDirectionText.color;
        while (color.a < 1f)
        {
            color.a += Time.deltaTime * fadeSpeed;
            SetTextAlpha(color.a);
            yield return null;
        }
        color.a = 1f;
        SetTextAlpha(color.a);
    }

    private IEnumerator FadeTextOut()
    {
        Color color = wrongDirectionText.color;
        while (color.a > 0f)
        {
            color.a -= Time.deltaTime * fadeSpeed;
            SetTextAlpha(color.a);
            yield return null;
        }
        color.a = 0f;
        SetTextAlpha(color.a);
    }

    private void SetTextAlpha(float alpha)
    {
        Color color = wrongDirectionText.color;
        color.a = alpha;
        wrongDirectionText.color = color;
    }
}
