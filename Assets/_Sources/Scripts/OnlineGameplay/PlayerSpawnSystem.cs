using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private List<Transform> spawnPointsList;
    [SerializeField] private Transform playerPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkManagerLobby.OnServerReadied += SpawnPlayer;
    }

    private void SpawnPlayer(List<List<NetworkConnection>> teamLists)
    {
        string teamSpawnTag;
        int teamNumber;
        int spawnIndex;
        List<Transform> startSpawnPoints = new List<Transform>();
        for(int i=0; i < teamLists.Count; i++)
        {
            spawnIndex = 0;
            teamNumber = i + 1;
            teamSpawnTag = "StartTagTeam" + teamNumber.ToString();
            
            foreach (Transform spawn in spawnPointsList)
            {
                if (spawn.CompareTag(teamSpawnTag))
                {
                    startSpawnPoints.Add(spawn);
                }
            }
            
            foreach (NetworkConnection conn in teamLists[i])
            {
                Transform playerSpawned = Instantiate(playerPrefab, startSpawnPoints[spawnIndex].position, startSpawnPoints[spawnIndex].rotation);
                NetworkServer.Spawn(playerSpawned.gameObject, conn);
                spawnIndex++;
            }
        }
    }
}
