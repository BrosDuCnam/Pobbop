using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SpawnPoint : NetworkBehaviour
{
    public static event Action<Transform> OnAddSpawnPoint;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        OnAddSpawnPoint?.Invoke(transform);
    }
    
    private void AddSpawn()
    {
        //SpawnSystem.instance.AddSpawnPoint(transform);
        OnAddSpawnPoint?.Invoke(transform);
    }
}
