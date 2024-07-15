using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartCountdownUI : MonoBehaviour
{
    [System.Serializable]
    public struct CountdownState{
        public string text;
        public VertexGradient colors;

    }

    [SerializeField] private CountdownState[] countdownStates;

    [SerializeField] TMP_Text text;

    public void SetCountdownToValue(int value)
    {
        Debug.Log(value);
        if (value < countdownStates.Length)
        {
            text.enabled = true;
            text.SetText(countdownStates[value].text);
            text.colorGradient = countdownStates[value].colors;
        }
        else { 
            text.enabled = false;
        }
    }
}
