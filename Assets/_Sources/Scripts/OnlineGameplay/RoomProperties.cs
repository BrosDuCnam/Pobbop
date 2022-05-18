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

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        DontDestroyOnLoad(this);
    }

    [Command]
    public void CmdChangeScoreLimit(int score)
    {
        RpcChangeScoreLimit(score);
    }

    [ClientRpc]
    private void RpcChangeScoreLimit(int score)
    {
        scoreLimit = score;
    }

    [Command]
    public void CmdChangeTimerLimit(float timer)
    {
        RpcChangeTimerLimit(timer);
    }

    [ClientRpc]
    private void RpcChangeTimerLimit(float timer)
    {
        timerLimit = timer;
    }
}
