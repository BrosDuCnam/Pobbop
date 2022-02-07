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
    
    /// <summary>
    /// Ce callback est appellé à chaque fois qu'un joueur ce connecte au serveur
    /// </summary>
    /// <param name="conn"></param>
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

    /// <summary>
    /// Cette fonction ajoute tous les joueurs du lobby dans des équipes aléatoirement
    /// </summary>
    private void GenerateTeams()
    {
        int teamNumber = 0;
        int n = nbPlayers / nbTeams;
        List<NetworkConnection> removeList = new List<NetworkConnection>(playerList);
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

    /// <summary>
    /// Cette fonction ajoute des listes vides à la liste d'équipe
    /// </summary>
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

    /// <summary>
    /// Cette fonction permet d'ajouter un point à une équipe
    /// </summary>
    /// <param name="teamNumber"></param>
    public static void AddPoint(int teamNumber)
    {
        teamScores[teamNumber - 1]++;
    }
}
