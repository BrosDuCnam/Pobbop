using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerSpawnMove : MonoBehaviour
{
    public static event Action<Transform> PlayerSpawned;

    private void Awake()
    {
        PlayerSpawned?.Invoke(transform);
    }
}