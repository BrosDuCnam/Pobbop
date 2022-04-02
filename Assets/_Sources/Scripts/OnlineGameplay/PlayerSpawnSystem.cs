using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSpawnSystem : NetworkBehaviour
{
    public static event Action<Transform> PlayerSpawned;
    public static event Action GetSpawnPoints;
    public static event Action<Transform, int> OnAddPlayerTransform;
    public static event Action<Transform, int> OnRemovePlayerTransform;

    [SerializeField] [SyncVar] private List<Transform> spawnPointsList;
    [SyncVar] private List<List<Transform>> teamTransformLists;

    [SyncVar] public int teamNumber = 0;

    private void Awake()
    {
        SpawnSystem.OnUpdateSpawnPoints += UpdateSpawnPoints;
        SpawnSystem.OnUpdateTeamTransformList += UpdateTransformTeam;
        
        PlayerSpawned?.Invoke(transform);
    }

    private void Start()
    {
        GetSpawnPoints?.Invoke();
    }
    
    private void UpdateSpawnPoints(List<Transform> spawnList)
    {
        spawnPointsList = spawnList;
    }

    private void UpdateTransformTeam(List<List<Transform>> teamList)
    {
        teamTransformLists = teamList;
    }
    
    public void PlayerEliminated()
    {
        if (teamNumber == 0)
        {
           RandomRespawn();
        }
        else
        {
            OnRemovePlayerTransform?.Invoke(transform, teamNumber);
            Respawn();
        }
    }

    public override void OnStopAuthority()
    {
        print("stop " + transform);
        base.OnStopAuthority();
    }
    private void RandomRespawn()
    {
        System.Random random = new System.Random();
        Transform spawnPoint = spawnPointsList[random.Next(0, spawnPointsList.Count)];
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        CmdDebugSpawn(gameObject.name);
    }
    
    private void Respawn()
    {
        Transform spawnPoint = PickSpawnPoint();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        OnAddPlayerTransform?.Invoke(transform, teamNumber);
        CmdDebugSpawn(gameObject.name);
    }

    void CmdDebugSpawn(string name)
    {
        RpcDebugSpawn(name);
    }
    
    void RpcDebugSpawn(string name)
    {
        Debug.Log($"{name} spawned");
    }
    

    private Transform PickSpawnPoint()
    {
        List<Transform> allEnemies = GetAllEnemies();
        List<Transform> allTm8 = GetAllTm8();
        print("enemies" + allEnemies.Count);
        print("tm8" + allTm8.Count);
        Transform spawnPoint = null;
        float dist = 0f;
        float n = 0f;
        foreach (Transform spawn in spawnPointsList)
        {
            n = CalculateDist(spawn, allEnemies, allTm8);
            if (n >= dist)
            {
                dist = n;
                spawnPoint = spawn;
            }
        }
        return spawnPoint;
    }

    private float CalculateDist(Transform spawnPoint, List<Transform> allEnemies, List<Transform> allTm8)
    {
        float distFinal = 0f;
        foreach (Transform enemie in allEnemies)
        {
            distFinal += Vector3.Distance(spawnPoint.position, enemie.position) * 2;
        }

        foreach (Transform tm8 in allTm8)
        {
            distFinal -= Vector3.Distance(spawnPoint.position, tm8.position);
        }

        return distFinal;
    }

    private List<Transform> GetAllEnemies()
    {
        List<Transform> enemies = new List<Transform>();
        for (int i = 0; i < teamTransformLists.Count; i++)
        {
            if (i != teamNumber - 1)
            {
                foreach (Transform t in teamTransformLists[i])
                {
                    enemies.Add(t);
                }
            }
        }

        return enemies;
    }

    private List<Transform> GetAllTm8()
    {
        return teamTransformLists[teamNumber - 1];
    }
}
