using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class StartOfGameUI : MonoBehaviour
{
  [SerializeField] private GameObject menuCanvas;
  [SerializeField] private GameObject hudCanvas;
  [SerializeField] private NetworkManager networkManager;

  public void StartHost()
  {
    networkManager.StartHost();
    SwitchCanvas();
    Debug.Log("Host started");
  }

   public void StartClient(string ipAddress)
    {
        if (networkManager != null)
        {
            var transport = networkManager.GetComponent<UnityTransport>();

            if (transport != null)
            {
                transport.SetConnectionData(ipAddress, 7777);

                Debug.Log($"IP Address set to: {ipAddress} on port: 7777");
            }
            else
            {
                Debug.LogError("UnityTransport component not found on the NetworkManager.");
            }
        }
        else
        {
            Debug.LogError("NetworkManager component not found.");
        }

        // Start the client
        networkManager.StartClient();

        SwitchCanvas();
        Debug.Log("Client started with IP: " + ipAddress);
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