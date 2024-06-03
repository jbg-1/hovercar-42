using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LevelSelectionItem : MonoBehaviour
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI levelName;
    [SerializeField] private Button button;
    private int id;
    private ServerUI serverUI;


    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        serverUI.OnSelectItem(id);
    }

    public void Setup(LevelDescription levelDescription, int id, ServerUI serverUI)
    {
        thumbnail.sprite = levelDescription.thumbnail;
        levelName.SetText(levelDescription.levelName);
        this.id = id;
        this.serverUI = serverUI;
    }
}
