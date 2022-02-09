using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private List<Transform> spawnPointsList;
    public static List<Transform> staticSpawnPointsList;
    
    [SerializeField] private Transform playerPrefab;

    private List<List<NetworkConnection>> teamLists;

    private static List<List<Transform>> teamTransformLists = new List<List<Transform>>();
    
    public static event Action OnInvokeSpawnPlayer ;
    
    int spawnIndex = 0;

    /// <summary>
    /// Ce callback est apellé quand le serveur est crée
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        //NetworkManagerLobby.OnServerReadied += SpawnPlayer;
        
        NetworkManagerLobby.OnServerReadied += GetTeamLists;
        SpawnMove.OnSpawnPlayer += OnPlayerSpawned;
        
        staticSpawnPointsList = spawnPointsList;
    }

    private void OnPlayerSpawned(Transform player)
    {
        AssignTeamNumber(player);
        teamTransformLists[player.GetComponent<BasePlayer>().teamNumber - 1].Add(player);
        MoveSpawnPlayer(player);
    }
    private void AssignTeamNumber(Transform player)
    {
        NetworkIdentity playerConn = player.GetComponent<NetworkIdentity>();
        for (int i = 0; i < teamLists.Count; i++)
        {
            foreach (NetworkConnection conn in teamLists[i])
            {
                if (conn.identity == playerConn)
                {
                    player.GetComponent<BasePlayer>().teamNumber = i + 1;
                }
            }
        }
    }

    private void GetTeamLists(List<List<NetworkConnection>> teamLists)
    {
        this.teamLists = teamLists;
        List<Transform> newTeam = new List<Transform>();
        for (int i = 0; i < teamLists.Count; i++)
        {
            teamTransformLists.Add(newTeam);
        }
        
        OnInvokeSpawnPlayer.Invoke();
    }

    private void MoveSpawnPlayer(Transform player)
    {
        List<Transform> startSpawnPoints = new List<Transform>();
        int teamNumber = player.GetComponent<BasePlayer>().teamNumber;
        startSpawnPoints = GetStartSpawnPoints(teamNumber);
        if (spawnIndex >= teamTransformLists[teamNumber - 1].Count)
        {
                spawnIndex = 0;
        } 
        player.position = startSpawnPoints[spawnIndex].position;
        player.rotation = startSpawnPoints[spawnIndex].rotation;
        spawnIndex++;
    }

    private List<Transform> GetStartSpawnPoints(int teamNumber)
    {
        List<Transform> startSpawnPoints = new List<Transform>();
        string teamSpawnTag = "StartTagTeam" + teamNumber.ToString();
        foreach (Transform spawn in spawnPointsList)
        {
            if (spawn.CompareTag(teamSpawnTag))
            {
                startSpawnPoints.Add(spawn);
            }
        }

        return startSpawnPoints;
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
    
    public static void Respawn(Transform player, int teamNumber)
    {
        Transform spawnPoint = PickSpawnPoint(teamNumber);
        player.position = spawnPoint.position;
        player.rotation = spawnPoint.rotation;
        PlayerAddTransform(player, teamNumber);
    }
    
    private static Transform PickSpawnPoint(int teamNumber)
    {
        List<Transform> allEnemies = GetAllEnemies(teamNumber);
        List<Transform> allTm8 = GetAllTm8(teamNumber);
        Transform spawnPoint = null;
        float dist = 0f;
        float n = 0f;
        foreach (Transform spawn in staticSpawnPointsList)
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
