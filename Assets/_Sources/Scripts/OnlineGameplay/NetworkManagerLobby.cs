using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkManagerLobby : NetworkManager
{
    public static event Action<List<List<NetworkConnection>>> OnServerReadied;

    private List<NetworkConnection> playerList = new List<NetworkConnection>();
    private List<List<NetworkConnection>> teamLists = new List<List<NetworkConnection>>();

    private static List<int> teamScores = new List<int>();

    private System.Random random = new System.Random();

    [SerializeField] private int nbPlayers;
    [SerializeField] private int nbTeams;

    private void Start()
    {
        GenerateTeamAmount();
    }
    
    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        playerList.Add(conn);

        if (playerList.Count == nbPlayers)
        {
            GenerateTeams();
            OnServerReadied?.Invoke(teamLists);
        }
    }

    private void GenerateTeams()
    {
        int teamNumber = 0;
        int n = nbPlayers / nbTeams;
        List<NetworkConnection> removeList = playerList;
        NetworkConnection player;

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

    private void GenerateTeamAmount()
    {
        List<NetworkConnection> newList;
        int newScore;
        for (int i = 0; i < nbTeams; i++)
        {
            newList = new List<NetworkConnection>();
            newScore = 0;
            teamLists.Add(newList);
            teamScores.Add(newScore);
        }
    }

    public static void AddPoint(int teamNumber)
    {
        teamScores[teamNumber - 1]++;
    }
}
