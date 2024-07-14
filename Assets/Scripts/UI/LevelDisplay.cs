using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelDisplay : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private TMP_Text statusText;

    public void SetLevelInformation(LobbyManager.LevelInformation? levelInformation)
    {
        if (levelInformation == null)
        {
            image.enabled = false;
            displayName.enabled = false;
        }
        else
        {
            image.enabled = true;
            displayName.enabled = true;

            image.sprite = levelInformation.Value.displayImage;
            displayName.text = levelInformation.Value.displayName;
        }
    }

    public void SetStatus(string status)
    {
        statusText.text = status;
    }
}
