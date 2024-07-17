using PuzzleCubes.Communication;
using PuzzleCubes.Models;
using Unity.Netcode;
using System.Collections;
using UnityEngine;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class MqttCommunicationHoverCar : MonoBehaviour
{

    public static MqttCommunicationHoverCar instance;
    private MqttCommunication mqttCommunication;

    public LobbyManager lobbyManager;

    private bool isHost = false;
    private bool isClient = false;

    private void Awake()
    {
        instance = this;
    }
    public void Init()
    {

        mqttCommunication = GetComponent<MqttCommunication>();
        if (mqttCommunication == null)
        {
            Debug.LogError("MqttCommunication component not found!");
            return;
        }

        // Subscribe to relevant topics
        mqttCommunication.Subscribe("puzzleCubes/app/emergencyConnection", HandleSetHostIPForClientsMessage);

        mqttCommunication.Subscribe("puzzleCubes/app/designateHostCommand", HandleDesignateHostCommandMessage);
        mqttCommunication.Subscribe("puzzleCubes/app/setHostIPForClients", HandleSetHostIPForClientsMessage);
        mqttCommunication.Subscribe("puzzleCubes/app/sendLevel", HandleSetLevel);
        mqttCommunication.Subscribe("puzzleCubes/app/startGameEvent", HandleStartGame);
        mqttCommunication.Subscribe("puzzleCubes/app/restartGameEvent", HandleRestartGame);

        // Register client with the broker
        SendClientRegistration();
    }

     

    private void SendClientRegistration()
    {
        var payload = new { clientId = mqttCommunication.clientId };

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
        if (command.command == "designateHost" && command.clientId == mqttCommunication.clientId && isHost == false)
        {
            Debug.Log("Designating host...");
            StartHost();
        }
    }

    private void StartHost()
    {

        lobbyManager.StartAsHost();

        // Get the host IP
        var hostIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList
          .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();
        isHost = true;

        
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
            lobbyManager.LoadRace(); 
        }
    }

    private void HandleRestartGame(MqttApplicationMessage message, object context)
    {
        if (isHost)
        {
            Debug.Log("Going back to lobby.");
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);

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