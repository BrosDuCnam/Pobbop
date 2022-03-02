using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkManagerLobby : NetworkManager
{
    private System.Random random = new System.Random();
   
    public static event Action<List<List<Transform>>> OnServerReadied;
    public static event Action OnNetworkManagerSpawn;
    private static event Action OnAllPlayersSpawned;

    private List<NetworkConnection> playerList = new List<NetworkConnection>(); 
    private List<Transform> playerTransformList = new List<Transform>();
    private List<List<Transform>> teamLists = new List<List<Transform>>();
    private static List<int> teamScores = new List<int>();

    [SerializeField] private int nbPlayers;
    [SerializeField] private int nbTeams;
    public override void OnStartServer()
    {
        base.OnStartServer();

        OnAllPlayersSpawned += AllPlayerSpawned;
        PlayerSpawnMove.PlayerSpawned += AddPlayerTransform;
        
        OnNetworkManagerSpawn?.Invoke();
        GenerateTeamAmount();
    }


    /// <summary>
    /// Ce callback est appellé à chaque fois qu'un joueur ce connecte au serveur
    /// </summary>
    /// <param name="conn"></param>
    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        playerList.Add(conn);
    }

    /// <summary>
    /// Cette fonction ajoute tous les joueurs du lobby dans des équipes aléatoirement
    /// </summary>
    private void GenerateTeams()
    {
        int teamNumber = 0;
        int n = nbPlayers / nbTeams;
        List<Transform> removeList = new List<Transform>(playerTransformList);
        Transform player;
        
        for (int i = 0; i < playerList.Count; i++)
        {
            if (i % n == 0 && i != 0)
            {
                teamNumber++;
            }
            player = removeList[random.Next(0, removeList.Count - 1)];
            removeList.Remove(player);
            teamLists[teamNumber].Add(player);
        }
    }

    /// <summary>
    /// Cette fonction ajoute des listes vides à la liste d'équipe
    /// </summary>
    private void GenerateTeamAmount()
    {
        List<Transform> newList;
        int newScore;
        for (int i = 0; i < nbTeams; i++)
        {
            newList = new List<Transform>();
            newScore = 0;
            teamLists.Add(newList);
            teamScores.Add(newScore);
        }
    }

    /// <summary>
    /// Cette fonction permet d'ajouter un point à une équipe
    /// </summary>
    /// <param name="teamNumber"></param>
    public static void AddPoint(int teamNumber)
    {
        teamScores[teamNumber - 1]++;
    }

    private void AddPlayerTransform(Transform player)
    {
        playerTransformList.Add(player);
        if (nbPlayers == playerTransformList.Count)
        {
            OnAllPlayersSpawned?.Invoke();
        }
    }

    private void AllPlayerSpawned()
    {
        GenerateTeams();
        OnServerReadied?.Invoke(teamLists);
    }
}
