using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SpawnSystem : NetworkBehaviour
{
    public static SpawnSystem instance;
    
    [SerializeField] private List<Transform> SpawnPointsList = new List<Transform>();

    public Transform playerPrefab;
    
    [SyncVar] private List<List<Transform>> teamTransformLists = new List<List<Transform>>();

    private static List<List<NetworkConnection>> teamLists;

    public static event Action<List<Transform>> onUpdateSpawnPoints;
    public static event Action<List<List<Transform>>> onUpdateTeamTransformList; 

    /// <summary>
    /// Ce callback est apellé quand le serveur est crée
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkManagerLobby.OnServerReadied += SetTeamList;
        //RealPlayer.OnEliminated += PlayerEliminated;

    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    
    public void AddSpawnPoint(Transform spawnPoint)
    {
        print(SpawnPointsList.Count);
        SpawnPointsList.Add(spawnPoint);
    }

    private void SetTeamList(List<List<Transform>> transformTeamList)
    {
        teamTransformLists = transformTeamList;
        onUpdateSpawnPoints?.Invoke(SpawnPointsList);
        onUpdateTeamTransformList?.Invoke(teamTransformLists);
        SetTeamNumber();
    }

    private void SetTeamNumber()
    {
        for (int i = 0; i < teamTransformLists.Count; i++)
        {
            foreach (Transform player in teamTransformLists[i])
            {
                player.GetComponent<PlayerSpawnSystem>().teamNumber = i + 1;
            }
        }

        MoveSpawn();
    }

    private void MoveSpawn()
    {
        for (int i = 0; i < teamTransformLists.Count; i++)
        {
            int spawnIndex = 0;
            List<Transform> startSpawnPoint = GetStartSpawnPoints(i + 1);
            foreach (Transform player in teamTransformLists[i])
            {
                if (spawnIndex >= startSpawnPoint.Count)
                {
                    spawnIndex = 0;
                }

                player.position = startSpawnPoint[spawnIndex].position;
                player.rotation = startSpawnPoint[spawnIndex].rotation;
            }
        }
    }

    private void SpawnPlayer()
    {
        for (int i = 0; i < teamLists.Count; i++)
        {
            int spawnIndex = 0;

            List<Transform> transformTeam = new List<Transform>();
            teamTransformLists.Add(transformTeam);

            List<Transform> startSpawnPoints = GetStartSpawnPoints(i + 1);

            foreach (NetworkConnection conn in teamLists[i])
            {
                Transform playerSpawned = Instantiate(playerPrefab, startSpawnPoints[spawnIndex].position,
                    startSpawnPoints[spawnIndex].rotation);
                NetworkServer.Spawn(playerSpawned.gameObject, conn);
                playerSpawned.gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
                teamTransformLists[i].Add(playerSpawned);
               // playerSpawned.GetComponent<BasePlayer>().teamNumber = i + 1;
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
                startSpawnPoints.Add(spawn);
            }
        }

        return startSpawnPoints;
    }
    
    public void PlayerAddTransform(Transform player, int teamNumber)
    {
        teamTransformLists[teamNumber - 1].Add(player);
        onUpdateTeamTransformList?.Invoke(teamTransformLists);
    }

    public void PlayerRemoveTransform(Transform player, int teamNumber)
    {
        teamTransformLists[teamNumber - 1].Remove(player);
        onUpdateTeamTransformList?.Invoke(teamTransformLists);
    }

}