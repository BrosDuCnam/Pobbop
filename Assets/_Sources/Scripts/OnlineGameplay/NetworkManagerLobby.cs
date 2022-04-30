using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManagerLobby : NetworkManager
{
    private System.Random random = new System.Random();

    public string playerName;
    
    public static event Action<List<List<Transform>>> OnServerReadied;
    private static event Action OnAllPlayersSpawned;
    public static event Action<List<List<Transform>>> OnUpdateTeamList; 

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
        PlayerSpawnSystem.PlayerSpawned += AddPlayerTransform;

        GenerateTeamAmount();
    }
    
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //base.OnServerAddPlayer(conn);
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        NetworkServer.AddPlayerForConnection(conn, player);
        //player.GetComponent<RealPlayer>().ChangeName(playerName);
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
    
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        DisconectPlayer(conn);
    }

    private void DisconectPlayer(NetworkConnection conn)
    {
        print("disconnect");
        playerList.Remove(conn);
        for (int i = 0; i < teamLists.Count; i++)
        {
            foreach (Transform player in teamLists[i])
            {
                if (player.GetComponent<NetworkIdentity>().connectionToServer == conn)
                {
                    print(player);
                    playerTransformList.Remove(player);
                    teamLists[i].Remove(player);
                    OnlineGameManager.RemoveTarget(player.gameObject);
                    OnUpdateTeamList?.Invoke(teamLists);
                }
            }
        }
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        
        NetworkServer.DisconnectAll();
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

        for (int i = 0; i < playerTransformList.Count; i++)
        {
            if (i % n == 0 && i != 0)
            {
                teamNumber++;
            }
            int randomNb = random.Next(0, removeList.Count - 1);
            player = removeList[randomNb];
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
