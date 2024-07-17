using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;

public class HUD : MonoBehaviour
{
    public static HUD instance;
    [SerializeField] public MiniMap miniMap;
    [SerializeField] private StartCountdownUI startCountdownUI;


    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundsText;
    public TextMeshProUGUI rankText;
    public GameObject steeringWheelIndicator;
    public TextMeshProUGUI wrongDirectionText;
    public GameObject itemDisplay;
    private Coroutine currentFade;
    public float fadeSpeed = 2.0f;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        wrongDirectionText.gameObject.SetActive(false);
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

    public void ChangeColors(Color vertexColor, Color[] gradientColors)
    {
        rankText.color = vertexColor;
        wrongDirectionText.color = vertexColor;

        VertexGradient gradient = new VertexGradient(gradientColors[0], gradientColors[1], gradientColors[2], gradientColors[3]);
        rankText.colorGradient = gradient;
        wrongDirectionText.colorGradient = gradient;
    }

    public void RotateSteeringWheelIndicator(float rotation)
    {
        steeringWheelIndicator.transform.rotation = Quaternion.Euler(0, 0, -rotation * 180f);
    }

    public void ToggleItemDisplay(bool show, string itemName = "", string itemFileEnding = "png")
    {
        if (!show && itemName == "")
        {
            itemDisplay.SetActive(false);
            return;
        }

        string pathToItem = $"Assets/Images/Items/{itemName}.{itemFileEnding}";
        Texture2D texture = LoadTexture(pathToItem);

        if (texture != null)
        {
            RawImage rawImage = itemDisplay.GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = texture;
                itemDisplay.SetActive(true);
            }
        }
    }

    Texture2D LoadTexture(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            if (!tex.LoadImage(fileData))
            {
                tex = null;
            }
        }
        return tex;
    }

    public void ToggleWrongDirectionText(bool show)
    {
        if (show)
        {
            wrongDirectionText.gameObject.SetActive(true);
            StartFading();
        }
        else
        {
            wrongDirectionText.gameObject.SetActive(false);
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

    public void SetCountdownToValue(int value)
    {
        startCountdownUI.SetCountdownToValue(value);
    }

    public void toggleVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
