using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

struct CarInformation
{
    public GameObject gameObject;
    public CarController carController;
    public ClientController clientControllerDtrivingThisCar;
}

public class RaceController : NetworkBehaviour
{
    private int amountOfPlayer;
    [SerializeField] private GameObject[] spawnPoints;

    [SerializeField] private GameObject carPrefab;
    private CarInformation[] cars;

    private ClientController[] clientController;

#if UNITY_EDITOR
    [Header("Editor Gizmos")]
    [SerializeField] private bool drawAlways = true;
    [SerializeField] private Mesh carMesh;
    [SerializeField] private float spawnPointFrontLineGizmoDistance = 1;
    [SerializeField] private float spawnPointFrontLineGizmoSize = 1;
#endif

    private void Start()
    {
        if (!IsServer)
            return;
        amountOfPlayer = NetworkManager.Singleton.ConnectedClientsList.Count;
        clientController = new ClientController[amountOfPlayer];
        cars = new CarInformation[amountOfPlayer];

        for (int i = 0; i < amountOfPlayer; i++)
        {
            clientController[i] = NetworkManager.Singleton.ConnectedClientsList[i].PlayerObject.GetComponent<ClientController>();
        }

        SpawnCars();
    }

    public float timeRemaining = 10;
    public bool started = false;

    void Update()
    {
        if (!IsServer)
            return;
        
        if (timeRemaining < 0 && !started)
        {
            StartRace();
            started = true;
        }

        timeRemaining -= Time.deltaTime;
    }

    private void StartRace()
    {
        foreach(CarInformation x in cars)
        {
            x.carController.ActivateDrivingRpc();
        }
    }

    private void SpawnCars()
    {
        for(int i = 0; i < amountOfPlayer; i++)
        {
            cars[i].gameObject = Instantiate(carPrefab, spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
            cars[i].gameObject.GetComponent<NetworkObject>().Spawn();
            cars[i].gameObject.GetComponent<NetworkObject>().ChangeOwnership(clientController[i].OwnerClientId);
            cars[i].carController = cars[i].gameObject.GetComponent<CarController>();
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(drawAlways)
            DrawSpawnPoints();
    }

    private void OnDrawGizmosSelected()
    {
        DrawSpawnPoints();
    }

    private void DrawSpawnPoints()
        {
            foreach (GameObject x in spawnPoints)
            {
                if (x != null)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireMesh(carMesh, 0, x.transform.position + x.transform.up, Quaternion.LookRotation(x.transform.up, x.transform.forward));
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(x.transform.position + x.transform.right * spawnPointFrontLineGizmoSize + x.transform.forward * spawnPointFrontLineGizmoDistance, x.transform.position - x.transform.right * spawnPointFrontLineGizmoSize + x.transform.forward * spawnPointFrontLineGizmoDistance);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(x.transform.position + x.transform.right * spawnPointFrontLineGizmoSize - x.transform.forward * spawnPointFrontLineGizmoDistance, x.transform.position - x.transform.right * spawnPointFrontLineGizmoSize - x.transform.forward * spawnPointFrontLineGizmoDistance);

                }
            }
        }
#endif
}
