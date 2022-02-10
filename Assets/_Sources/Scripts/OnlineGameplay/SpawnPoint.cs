using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private void Awake()
    {
        NetworkManagerLobby.OnNetworkManagerSpawn += AddSpawn;
    }
    
    
    private void AddSpawn()
    {
        PlayerSpawnSystem.AddSpawnPoint(transform);
    }
}
