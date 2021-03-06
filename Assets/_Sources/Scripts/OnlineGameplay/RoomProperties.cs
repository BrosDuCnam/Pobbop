using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomProperties : NetworkBehaviour
{
    public static RoomProperties instance;

    public int scoreLimit;
    public float timerLimit;

    public enum GameLimitModes
    {
        Score,
        Timer,
        ScoreTimer
    };

    [HideInInspector] public GameLimitModes gameLimitMode;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

       // DontDestroyOnLoad(this);
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeScoreLimit(int score)
    {
        RpcChangeScoreLimit(score);
    }

    [ClientRpc]
    private void RpcChangeScoreLimit(int score)
    {
        print("scoreLimit " + score);
        instance.scoreLimit = score;
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeTimerLimit(float timer)
    {
        RpcChangeTimerLimit(timer);
    }

    [ClientRpc]
    private void RpcChangeTimerLimit(float timer)
    {
        print("timerLimit " + timer);
        instance.timerLimit = timer;
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeGameLimitMode(int value)
    {
        RpcChangeGameLimitMode(value);    
    }

    [ClientRpc]
    private void RpcChangeGameLimitMode(int value)
    {
        if (value == 0)
        {
            instance.gameLimitMode = GameLimitModes.Score;
        }
        else if (value == 1)
        {
            instance.gameLimitMode = GameLimitModes.Timer;
        }
        else if (value == 2)
        {
            instance.gameLimitMode = GameLimitModes.ScoreTimer;
        }
    }
}