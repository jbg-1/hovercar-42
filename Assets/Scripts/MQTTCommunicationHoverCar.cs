using PuzzleCubes.Communication;
using PuzzleCubes.Models;
using System.Collections;
using UnityEngine;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using Unity.Netcode;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class MqttCommunicationHoverCar : MonoBehaviour
{
  private MqttCommunication mqttCommunication;
  private string clientId;

  [SerializeField] private LobbyManager lobbyManager;

  private NetworkManager networkManager;

  private bool isHost = false;
  private bool isClient = false;

  void Start()
  {
    mqttCommunication = GetComponent<MqttCommunication>();
    networkManager = GetComponent<NetworkManager>();

    if (mqttCommunication == null)
    {
      Debug.LogError("MqttCommunication component not found!");
      return;
    }

    // Generate a unique client ID
    clientId = Guid.NewGuid().ToString();

    // Initialize MqttCommunication with a unique client ID
    mqttCommunication.Initialize(clientId);

    // Subscribe to relevant topics
    mqttCommunication.Subscribe("puzzleCubes/app/emergencyConnection", HandleSetHostIPForClientsMessage);

    mqttCommunication.Subscribe("puzzleCubes/app/designateHostCommand", HandleDesignateHostCommandMessage);
    mqttCommunication.Subscribe("puzzleCubes/app/setHostIPForClients", HandleSetHostIPForClientsMessage);
    mqttCommunication.Subscribe("puzzleCubes/app/sendLevel", HandleSetLevel);
    mqttCommunication.Subscribe("puzzleCubes/app/startGameEvent", HandleStartGame);

    // Register client with the broker
    SendClientRegistration();
  }

  private void SendClientRegistration()
  {
    var payload = new { clientId = clientId };

    // Create a JsonDatagram with payload
    var datagram = new JsonDatagram();
    datagram.TokenData.Add("payload", JToken.FromObject(payload));

    // Send the JsonDatagram
    mqttCommunication.Send("puzzleCubes/app/registerClient", datagram);
  }


  private void HandleDesignateHostCommandMessage(MqttApplicationMessage message, object context)
  {
    string payload = Encoding.UTF8.GetString(message.Payload);
    var command = JsonUtility.FromJson<HostCommand>(payload);
    if (command.command == "designateHost" && command.clientId == clientId && isHost == false)
    {
      Debug.Log("Designating host...");
      StartHost();
    }
  }

  private void StartHost()
  {
    lobbyManager.StartAsHost(); 
    isHost = true;

    // Get the host IP
    var hostIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList
      .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();

    if (hostIP != null)
    {
      // Prepare the message
      var message = new HostIPMessage
      {
        command = "setHostIP",
        hostIP = hostIP,
        timestamp = DateTime.Now.ToString("o")
      };

      // Serialize the message to JSON
      string jsonMessage = JsonUtility.ToJson(message);

      // Create a JsonDatagram with payload
      var datagram = new JsonDatagram();
      datagram.TokenData.Add("setHostIP", JToken.FromObject(message));

      // Send the JsonDatagram over MQTT
      mqttCommunication.Send("puzzleCubes/app/setHostIP", datagram);
    }
    else
    {
      Debug.LogError("Failed to retrieve host IP.");
    }
  }

  private void HandleSetHostIPForClientsMessage(MqttApplicationMessage message, object context)
  {
    if (isHost || isClient)
    {
      Debug.Log("Ignoring host IP: Already connected");
      return;
    }

    string payload = Encoding.UTF8.GetString(message.Payload);
    var hostIPMessage = JsonUtility.FromJson<HostIPMessage>(payload);
    if (hostIPMessage.command == "setHostIPForClients")
    {
      Debug.Log("Setting host IP: " + hostIPMessage.hostIP);
      // Set the host IP in the network manager and start the client
      lobbyManager.StartAsClient(hostIPMessage.hostIP); 
      isClient = true;
    }
  }

  private void HandleSetLevel(MqttApplicationMessage message, object context)
  {
    Debug.Log("isHost: " + isHost);

    if (isHost)
    {
      string payload = Encoding.UTF8.GetString(message.Payload);
      var levelCommand = JsonUtility.FromJson<LevelCommand>(payload);

      if (levelCommand.command == "setLevel")
      {
        // Print levelId to the debug log
        lobbyManager.SelectLevel(levelCommand.levelId);
      }
    }
  }

  private void HandleStartGame(MqttApplicationMessage message, object context)
  {
    if (isHost)
    {
      Debug.Log("All clients connected. Starting game...");
      lobbyManager.LoadRace(); //TODO
    }
  }
}

[System.Serializable]
public class HostCommand
{
  public string command;
  public string clientId;
  public string timestamp;
}

[System.Serializable]
public class HostIPMessage
{
  public string command;
  public string hostIP;
  public string timestamp;
}

[System.Serializable]
public class LevelCommand
{
  public string command;
  public int levelId;
  public string timestamp;
}