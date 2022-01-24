using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private List<Transform> spawnPointsList;
    [SerializeField] private Transform playerPrefab;
    
    private int spawnIndex;

    public override void OnStartServer()
    {
        base.OnStartServer();

        spawnIndex = 0;

        NetworkManagerLobby.OnServerReadied += SpawnPlayer;
    }

    private void SpawnPlayer(List<NetworkConnection> playerList)
    {
        Transform spawnPoint = spawnPointsList[spawnIndex];
        foreach (NetworkConnection conn in playerList)
        {
            Transform playerSpawned = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(playerSpawned.gameObject, conn);
            spawnIndex++;
        }
    }
}
