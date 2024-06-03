using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RaceController : NetworkBehaviour
{
        void Start()
        {
            NetworkManager.ConnectionApprovalCallback = ConnectionApprovalCallback;
        }

        void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            /* you can use this method in your project to customize one of more aspects of the player
             * (I.E: its start position, its character) and to perform additional validation checks. */
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = GetPlayerSpawnPosition();
        }

        Vector3 GetPlayerSpawnPosition()
        {
            /*
             * this is just an example, and you change this implementation to make players spawn on specific spawn points
             * depending on other factors (I.E: player's team)
             */
            return new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));
        }
    }
