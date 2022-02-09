using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System;

public class SpawnMove : MonoBehaviour
{
    public static event Action<Transform> OnSpawnPlayer;

    public static event Action playerSpawned;
    // Start is called before the first frame update
    private void Awake()
    {
        NetworkManagerLobby.OnInvokeSpawnPlayer += InvokeSpawnPlayer;
    }

    private void Start()
    {
        playerSpawned?.Invoke();
    }

    private void InvokeSpawnPlayer()
    {
        OnSpawnPlayer?.Invoke(transform);
    }
}
