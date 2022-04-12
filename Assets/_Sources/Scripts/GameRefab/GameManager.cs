using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string playerIdPrefix = "Player";
    private static Dictionary<string, Player> players = new Dictionary<string, Player>();
    public static GameManager instance;
    public delegate void OnPlayerKilledCallback(string player, string source);
    public OnPlayerKilledCallback onPlayerKilledCallback;
    private void Awake()
    {
        if (instance == null) instance = this; return;
        Debug.LogError("Multiple game managers");
    }
    
    
    public static void RegisterPlayer(string netID,  Player player)
    {
        string playerId = playerIdPrefix + netID;
        players.Add(playerId, player);
        player.transform.name = playerId;
    }
    
    public static void UnRegisterPlayer(string playerId)
    {
        players.Remove(playerId);
    }
    
    public static Player GetPlayer(string playerId)
    {
        return players[playerId];
    }

    public static Player[] GetAllPlayers()
    {
        return players.Values.ToArray();
    }
    
}
