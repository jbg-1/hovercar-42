using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ServerUI : MonoBehaviour
{
    [SerializeField] LobbyController lobbyController;

    [Header("LevelSelection")]
    [SerializeField] private GameObject levelSelectionRoot;
    [SerializeField] private GameObject levelUIItem;
    private Button[] buttons;
    private int lastSelecte;
    [SerializeField] private Button startButton;


    [Header("PlayerList")]
    [SerializeField] private GameObject playerListRoot;
    [SerializeField] private GameObject lobbyPlayerListItem;
    private readonly List<GameObject> listOfItems = new ();


    public void SetListItems(IEnumerable<ulong> ids)
    {
        foreach(GameObject x in listOfItems)
        {
            Destroy(x);
        }
        int counter = 0;
        foreach(ulong x in ids)
        {
            GameObject g = Instantiate(lobbyPlayerListItem, playerListRoot.transform);
            g.GetComponent<LobbyPlayerListItem>().SetPlayerId(x);
            g.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,-100 - 75 * counter,0);
            listOfItems.Add(g);
            counter++;
        }
    }

    public void SetLevelItems(LevelDescription[] levelDescriptions)
    {
        buttons = new Button[levelDescriptions.Length];
        lastSelecte = 0;
        for (int i = 0; i < levelDescriptions.Length; i++)
        {
            GameObject g = Instantiate(levelUIItem, levelSelectionRoot.transform);

            RectTransform rT = g.GetComponent<RectTransform>();

            rT.anchoredPosition = new Vector3(0,-200 -400 * (i/2), 0);
            rT.anchorMin = new Vector2(0.5f * (i%2),1);
            rT.anchorMax = new Vector2(0.5f + 0.5f * (i % 2), 1);

            g.GetComponent<LevelSelectionItem>().Setup(levelDescriptions[i],i,this);

            buttons[i] = g.GetComponent<Button>();
        }
        buttons[0].interactable = false;
    }

    public void OnSelectItem(int id)
    {
        buttons[lastSelecte].interactable = true;
        buttons[id].interactable = false;
        lastSelecte = id;
        lobbyController.SelectMap(id);
    }

    public void SetRaceStartable(bool value)
    {
        startButton.interactable = value;
    }
}
