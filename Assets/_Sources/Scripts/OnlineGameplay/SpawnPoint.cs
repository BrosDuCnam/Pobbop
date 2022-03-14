using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SpawnPoint : NetworkBehaviour
{
    public static event Action<Transform> OnAddSpawnPoint; 
    private void Awake()
    {
        NetworkManagerLobby.OnNetworkManagerSpawn += AddSpawn;
    }
    
    
    private void AddSpawn()
    {
        SpawnSystem.instance.AddSpawnPoint(transform);
        //OnAddSpawnPoint?.Invoke(transform);
    }
}
