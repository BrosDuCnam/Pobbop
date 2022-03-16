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

    [SerializeField] [SyncVar] private List<Transform> spawnPointsList;
    [SyncVar] private List<List<Transform>> teamTransformLists;

    [SyncVar] [HideInInspector] public int teamNumber = 0;

    private void Awake()
    {
        SpawnSystem.onUpdateSpawnPoints += UpdateSpawnPoints;
        SpawnSystem.onUpdateTeamTransformList += UpdateTransformTeam;
        
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
           Respawn(transform);
        }
        else
        {
            //SpawnSystem.instance.PlayerRemoveTransform(transform, teamNumber); 
            Respawn(transform, teamNumber);
        }
    }
    
    private void Respawn(Transform player)
    {
        System.Random random = new System.Random();
        Transform spawnPoint = spawnPointsList[random.Next(0, spawnPointsList.Count)];
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }
    
    private void Respawn(Transform player, int teamNumber)
    {
        Transform spawnPoint = PickSpawnPoint(teamNumber);
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        //SpawnSystem.instance.PlayerAddTransform(player, teamNumber);
    }

    private Transform PickSpawnPoint(int teamNumber)
    {
        List<Transform> allEnemies = GetAllEnemies(teamNumber);
        List<Transform> allTm8 = GetAllTm8(teamNumber);
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

    private List<Transform> GetAllEnemies(int teamNumber)
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

    private List<Transform> GetAllTm8(int teamNumber)
    {
        return teamTransformLists[teamNumber - 1];
    }
}
