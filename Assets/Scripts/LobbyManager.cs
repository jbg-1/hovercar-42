using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Linq;



public class LobbyManager : NetworkBehaviour
{
    [System.Serializable]
    public struct LevelInformation
    {
        public string displayName;
        public string sceneName;
        public Sprite displayImage;
    }

    [SerializeField] private LevelDisplay levelDisplay;


    [SerializeField] private LevelInformation[] availableLevel;

    private int currentlySelectedLevel = -1;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        MqttCommunicationHoverCar.instance.lobbyManager = this;
    }

    public void SelectLevel(int id)
    {
        currentlySelectedLevel = id;
        SelectLevelClientRpc(id);
    }

    [ClientRpc]
    public void SelectLevelClientRpc(int id)
    {
        if (id < 0)
        {
            levelDisplay.SetLevelInformation(null);
        }
        else {
            levelDisplay.SetLevelInformation(availableLevel[id]);
        }
    }

    //TODO Start
    public void LoadRace() {
        if (NetworkManager.IsServer)
        {
            if(currentlySelectedLevel >= 0)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                levelDisplay.SetStatus("Scene wird geladen.");
                NetworkManager.SceneManager.LoadScene(availableLevel[currentlySelectedLevel].sceneName, LoadSceneMode.Single);
            }
            else
            {
                levelDisplay.SetStatus("Es wurde noch keine Strecke ausgew�hlt");
            }
        }
        else {
            levelDisplay.SetStatus("Nur der Server/Host kann das Race starten.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer) {
            Debug.Log(clientId + "joined");
            SelectLevelClientRpc(currentlySelectedLevel);
        }
        if(NetworkManager.LocalClientId == clientId)
        {
            levelDisplay.SetStatus("Verbindung zum Server wurde hergestellt.");
        }
    }

    //Start
    public void StartAsClient(string ip)
    {
        if (NetworkManager.Singleton != null)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            if (transport != null)
            {
                transport.SetConnectionData(ip, 7777);

                Debug.Log($"IP Address set to: {ip} on port: 7777");
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
        NetworkManager.StartClient();

        Debug.Log("Client started with IP: " + ip);
    }

    public void StartAsHost()
    {
        // Get the host IP
        string hostIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList
          .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(hostIP, 7777,"0.0.0.0");
        NetworkManager.StartHost();
    }

}
