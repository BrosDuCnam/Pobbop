using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BallSpawner : NetworkBehaviour
{
    public GameObject ballPrefab;
    public bool spawnBall;


    private void Update()
    {
        if (spawnBall)
        {
            SpawnBall();
            spawnBall = false;
        }
    }

    [Command (requiresAuthority = false)]
    private void SpawnBall()
    {
        if (isServer)
        {
            GameObject testBall = Instantiate(ballPrefab, new Vector3(10, 3, 0), Quaternion.Euler(0, 0, 0));
            NetworkServer.Spawn(testBall);
        }
    }
}
