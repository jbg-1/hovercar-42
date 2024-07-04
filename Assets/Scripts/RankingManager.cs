using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static RaceController;

public class RankingManager : MonoBehaviour
{
    public RaceController.PlayerInformation[] playerInformation;

    [SerializeField] private CheckpointLogic checkpointLogic;

    public List<int> CalculateRankingsServerRpc()
    {
        List<KeyValuePair<int,int>> ranking = checkpointLogic.checkPointCount.ToList()
            .OrderBy(x => x.Value)
            .ThenBy(x => Vector3.Distance(playerInformation[x.Key].carGameObject.transform.position, checkpointLogic.getCheckpoint(x.Value+1).transform.position))
            .ToList();

        List<int> rank = new List<int>();

        foreach(KeyValuePair<int, int> x in ranking)
        {
            rank.Add(x.Key);
        }
        return rank;
    }
}
