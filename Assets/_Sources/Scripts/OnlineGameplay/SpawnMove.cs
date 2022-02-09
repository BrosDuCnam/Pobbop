using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System;

public class SpawnMove : MonoBehaviour
{
    public static event Action<Transform> OnSpawnPlayer;
    // Start is called before the first frame update
    void Start()
    {
        PlayerSpawnSystem.OnInvokeSpawnPlayer += InvokeSpawnPlayer;
    }

    private void InvokeSpawnPlayer()
    {
        OnSpawnPlayer.Invoke(transform);
    }
}
