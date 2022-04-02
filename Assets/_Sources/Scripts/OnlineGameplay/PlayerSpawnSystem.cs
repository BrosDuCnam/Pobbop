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
        print("update spawn list");
        spawnPointsList = spawnList;
    }

    private void UpdateTransformTeam(List<List<Transform>> teamList)
    {
        print("transform lists");
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
        print("Picked spawn " + spawnPoint);
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
        Transform spawnPoint = null;
        float dist = 0f;
        float n = 0f;
        foreach (Transform spawn in spawnPointsList)
        {
            n = CalculateDist(spawn, allEnemies, allTm8);
            print("distance point " + n);
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
            print("enemie " + enemie);
            distFinal += Vector3.Distance(spawnPoint.position, enemie.position) * 2;
        }

        foreach (Transform tm8 in allTm8)
        {
            print("tm8 " + tm8);
            distFinal -= Vector3.Distance(spawnPoint.position, tm8.position);
        }

        return distFinal;
    }

    private List<Transform> GetAllEnemies()
    {
        print("Getting enemies");
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
        print("getting tm8s");
        return teamTransformLists[teamNumber - 1];
    }
}
