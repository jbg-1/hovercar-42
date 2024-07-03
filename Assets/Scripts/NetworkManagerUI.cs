using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
  [SerializeField] private Button hostButton;
  [SerializeField] private Button clientButton;
  [SerializeField] private Button serverButton;
  [SerializeField] private GameObject menuCanvas;
  [SerializeField] private GameObject hudCanvas;
  [SerializeField] private PlayerIcons playerIcons;

  private void Awake()
  {
    hudCanvas.SetActive(false);

    hostButton.onClick.AddListener(() =>
    {
      NetworkManager.Singleton.StartHost();
      SwitchCanvas();
    });
    serverButton.onClick.AddListener(() =>
    {
      NetworkManager.Singleton.StartServer();
      SwitchCanvas();
    });

    clientButton.onClick.AddListener(() =>
    {
      NetworkManager.Singleton.StartClient();
      SwitchCanvas();
    });
  }

  private void Start()
  {
    //NetworkManager.Singleton.StartClient();
    //SwitchCanvas();
  }

  private void SwitchCanvas()
  {
    menuCanvas.SetActive(false);
    hudCanvas.SetActive(true);
  }
}