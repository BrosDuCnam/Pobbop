using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomProperties : NetworkBehaviour
{
    public static RoomProperties instance;

    [HideInInspector] public int scoreLimit;
    [HideInInspector] public float timerLimit;

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
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }

    [ClientRpc]
    public void RpcChangeScoreLimit(int score)
    {
        scoreLimit = score;
    }

    [ClientRpc]
    public void RpcChangeTimerLimit(float timer)
    {
        timerLimit = timer;
    }

    [ClientRpc]
    public void RpcChangeGameLimitMode(int value)
    {
        if (value == 0)
        {
            gameLimitMode = GameLimitModes.Score;
        }
        else if (value == 1)
        {
            gameLimitMode = GameLimitModes.Timer;
        }
        else if (value == 2)
        {
            gameLimitMode = GameLimitModes.ScoreTimer;
        }
    }
}