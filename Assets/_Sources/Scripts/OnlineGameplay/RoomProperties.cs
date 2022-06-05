using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomProperties : MonoBehaviour
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

        DontDestroyOnLoad(this);
    }
    
    public void RpcChangeScoreLimit(int score)
    {
        print("scoreLimit " + score);
        scoreLimit = score;
    }
    
    public void RpcChangeTimerLimit(float timer)
    {
        print("timerLimit " + timer);
        timerLimit = timer;
    }
    
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