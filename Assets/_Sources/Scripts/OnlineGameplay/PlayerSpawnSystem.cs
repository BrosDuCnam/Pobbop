using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
{
    public static List<Transform> SpawnPointsList = new List<Transform>();
    
    [HideInInspector] public Transform playerPrefab;

    private List<List<NetworkConnection>> teamLists;

    private static List<List<Transform>> teamTransformLists = new List<List<Transform>>();

    int spawnIndex = 0;

    /// <summary>
    /// Ce callback est apellé quand le serveur est crée
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        //NetworkManagerLobby.OnServerReadied += SpawnPlayer;
    }

    public static void AddSpawnPoint(Transform spawnPoint)
    {
        SpawnPointsList.Add(spawnPoint);
    }
    
    private void SpawnPlayer(List<List<NetworkConnection>> teamLists)
    {
        int spawnIndex;
        List<Transform> startSpawnPoints;
        for(int i=0; i < teamLists.Count; i++)
        {
            spawnIndex = 0;

            List<Transform> transformTeam = new List<Transform>();
            teamTransformLists.Add(transformTeam);

            startSpawnPoints = GetStartSpawnPoints(i + 1);
            
            foreach (NetworkConnection conn in teamLists[i])
            {
                Transform playerSpawned = Instantiate(playerPrefab, startSpawnPoints[spawnIndex].position, startSpawnPoints[spawnIndex].rotation);
                NetworkServer.Spawn(playerSpawned.gameObject, conn);
                playerSpawned.gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
                teamTransformLists[i].Add(playerSpawned);
                playerSpawned.GetComponent<BasePlayer>().teamNumber = i + 1;
                spawnIndex++;
            }
        }
    }
    
    private List<Transform> GetStartSpawnPoints(int teamNumber)
    {
        List<Transform> startSpawnPoints = new List<Transform>();
        string teamSpawnTag = "StartTagTeam" + teamNumber.ToString();
        foreach (Transform spawn in SpawnPointsList)
        {
            if (spawn.CompareTag(teamSpawnTag))
            {
                startSpawnPoints.Add(spawn); ;
            }
        }

        return startSpawnPoints;
    }
    
    public static void Respawn(Transform player, int teamNumber)
    {
        //Transform spawnPoint = PickSpawnPoint(teamNumber);
        System.Random random = new System.Random();
        Debug.Log("Respawn");
        Transform spawnPoint = SpawnPointsList[random.Next(0, SpawnPointsList.Count)];
        player.position = spawnPoint.position;
        player.rotation = spawnPoint.rotation;
        //PlayerAddTransform(player, teamNumber);
    }
    
    private static Transform PickSpawnPoint(int teamNumber)
    {
        List<Transform> allEnemies = GetAllEnemies(teamNumber);
        List<Transform> allTm8 = GetAllTm8(teamNumber);
        Transform spawnPoint = null;
        float dist = 0f;
        float n = 0f;
        foreach (Transform spawn in SpawnPointsList)
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
    
    private static float CalculateDist(Transform spawnPoint, List<Transform> allEnemies, List<Transform> allTm8)
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
    
    private static List<Transform> GetAllEnemies(int teamNumber)
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
    
    private static List<Transform> GetAllTm8(int teamNumber)
    {
 
        return teamTransformLists[teamNumber - 1];
    }
    
    private static void PlayerAddTransform(Transform player, int teamNumber)
    {
        teamTransformLists[teamNumber - 1].Add(player);
    }
    
    public static void PlayerRemoveTransform(Transform player, int teamNumber)
    {
        teamTransformLists[teamNumber - 1].Remove(player);
    }
}
