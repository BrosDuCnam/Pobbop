using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private const string playerIdPrefix = "Player";
    private const string teamIdPrefix = "Team";
    private static Dictionary<string, Player> players = new Dictionary<string, Player>(); 
    public static Dictionary<string, int> teams = new Dictionary<string, int>();
    public static GameManager instance;

    public delegate void OnPlayerKilledCallback(string player, string source);

    public OnPlayerKilledCallback onPlayerKilledCallback;

    public delegate void OnPlayerJoinedCallback(string player);

    public OnPlayerJoinedCallback onPlayerJoinedCallback;

    public delegate void OnPlayerLeftCallback(string player);

    public OnPlayerLeftCallback onPlayerLeftCallback;

    private static bool gameStarted;
    private static int scoreLimit;
    [HideInInspector] public float timerLimit;
    private static RoomProperties.GameLimitModes gameLimitMode;

    private void Awake()
    {
        if (instance == null) instance = this;
        DontDestroyOnLoad(gameObject);

        NetworkManagerRefab.OnStartGame += OnGameStarted;
        
        teams.Add("Team0", 0);
        teams.Add("Team1", 0);
    }

    public static void RegisterPlayer(string netID, Player player)
    {
        string playerId = playerIdPrefix + netID;
        players.Add(playerId, player);
        player.transform.name = playerId + $" ({player.username})";
        if (instance != null && instance.onPlayerJoinedCallback != null)
            instance.onPlayerJoinedCallback.Invoke(playerId);
    }

    public static void UnRegisterPlayer(string playerId)
    {
        players.Remove(playerId);
        instance.onPlayerLeftCallback.Invoke(playerId);
    }

    public static void RegisterTeam(int id)
    {
        string teamId = teamIdPrefix + id;
        teams.Add(teamId, 0);
    }

    public static void UnregisterTeam(int id)
    {
        string teamId = teamIdPrefix + id;
        teams.Remove(teamId);
    }

    public static Player GetPlayer(string playerId)
    {
        return players[playerId];
    }

    public static Player[] GetAllPlayers()
    {
        return players.Values.ToArray();
    }

    public static string GetPlayerId(Player player)
    {
        string playerId = null;
        foreach (string id in players.Keys)
        {
            if (players[id] == player)
            {
                playerId = id;
                break;
            }
        }

        return playerId;
    }

    public static void ChangeTeamKills(int id, bool increase)
    {
        string teamId = teamIdPrefix + id;
        if (increase)
        {
            teams[teamId]++;
        }
        else
        {
            teams[teamId]--;
        }
        CheckScore(teamId);
    }

    public static void ChangeTeamKills(int id, bool increase, Player killedPlayer, Player source)
    {
        string teamId = teamIdPrefix + id;
        if (increase)
        {
            teams[teamId]++;
        }
        else
        {
            teams[teamId]--;
        }

        string killedPlayerId = GetPlayerId(killedPlayer);
        string sourceId = GetPlayerId(source);
        // instance.onPlayerKilledCallback.Invoke(killedPlayerId, sourceId);
        CheckScore(teamId);
    }


    private void Update()
    {
        if (gameStarted && (gameLimitMode == RoomProperties.GameLimitModes.Timer || gameLimitMode == RoomProperties.GameLimitModes.ScoreTimer))
        {
            if (timerLimit > 0)
            {
                timerLimit -= Time.deltaTime / 60;
                print(timerLimit);
            }
            else
            {
                OnGameEnded();
            }
        }
    }

    private static void CheckScore(string teamId)
    {
        if (gameStarted && (gameLimitMode == RoomProperties.GameLimitModes.Score || gameLimitMode == RoomProperties.GameLimitModes.ScoreTimer))
        {
            if (teams[teamId] >= scoreLimit)
            {
                OnGameEnded();
            }
        }
    }

    private void OnGameStarted()
    {
        SetGameLimitMode();
        SetScore();
        SetTimer();
        gameStarted = true;
    }

    private static void OnGameEnded()
    {
        NetworkManagerRefab.instance.EndGame();
        gameStarted = false;
    }

    private void SetScore()
    {
        scoreLimit = RoomProperties.instance.scoreLimit;
    }

    private void SetTimer()
    {
        timerLimit = RoomProperties.instance.timerLimit;
    }

    private void SetGameLimitMode()
    {
        gameLimitMode = RoomProperties.instance.gameLimitMode;
        print(gameLimitMode);
    }
}