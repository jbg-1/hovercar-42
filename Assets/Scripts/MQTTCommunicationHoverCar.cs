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

    [SerializeField]
    private StartOfGameUI startOfGame;

    private NetworkManager networkManager;

    private bool isHost = false;

    [SerializeField] private RaceController raceController;

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
        mqttCommunication.Subscribe("puzzleCubes/app/designateHostCommand", HandleDesignateHostCommandMessage);
        mqttCommunication.Subscribe("puzzleCubes/app/setHostIPForClients", HandleSetHostIPForClientsMessage);
        mqttCommunication.Subscribe("puzzleCubes/app/allConnectedEvent", HandleAllConnected);

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

        Debug.Log("Sent client registration message: " + JsonConvert.SerializeObject(datagram));
    }


    private void HandleDesignateHostCommandMessage(MqttApplicationMessage message, object context)
    {
        Debug.Log("Received host command message: " + Encoding.UTF8.GetString(message.Payload));
        string payload = Encoding.UTF8.GetString(message.Payload);
        var command = JsonUtility.FromJson<HostCommand>(payload);
        Debug.Log("Command: " + command.command + ", Client ID: " + command.clientId + "My client Id is " + clientId);
        if (command.command == "designateHost" && command.clientId == clientId)
        {
            Debug.Log("Designating host...");
            StartHost();
            NotifyConnection(clientId);
        }
    }

    private void HandleSetHostIPForClientsMessage(MqttApplicationMessage message, object context)
    {
        if (isHost)
        {
            Debug.Log("Ignoring host IP message because I am the host.");
            return;
        }
        
        Debug.Log("Received host IP message: " + Encoding.UTF8.GetString(message.Payload));
        string payload = Encoding.UTF8.GetString(message.Payload);
        var hostIP = JsonUtility.FromJson<HostIPMessage>(payload);
        if (hostIP.command == "setHostIPForClients")
        {
            Debug.Log("Setting host IP: " + hostIP.hostIP);
            // Set the host IP in the network manager and start the client
            startOfGame.StartClient(hostIP.hostIP);
            NotifyConnection(clientId);
        }
    }

    private void NotifyConnection (String clientId) {
            
            // Prepare the message
            var message = new HostCommand
            {
                command = "notifyConnection",
                clientId = clientId,
                timestamp = DateTime.Now.ToString("o")
            };

            // Serialize the message to JSON
            string jsonMessage = JsonUtility.ToJson(message);

            // Create a JsonDatagram with payload
            var datagram = new JsonDatagram();
            datagram.TokenData.Add("notifyConnection", JToken.FromObject(message));

            // Send the JsonDatagram over MQTT
            mqttCommunication.Send("puzzleCubes/app/notifyConnection", datagram);
    }

    private void StartHost()
    {
        startOfGame.StartHost();
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

            Debug.Log("Sent command: " + JsonConvert.SerializeObject(datagram));
        }
        else
        {
            Debug.LogError("Failed to retrieve host IP.");
        }
    }

    private void HandleAllConnected(MqttApplicationMessage message, object context)
    {
        if (isHost)
        {
            Debug.Log("All clients connected. Starting game...");
            raceController.SpawnCars();
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
