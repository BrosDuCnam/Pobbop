using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SBStateInfo : FSMStateInfo
{
    public BotPlayer bot;

    public BotController controller
    {
        get => (BotController) bot._controller;
    }
    
    public Dictionary<Player, Vector3> playerHistory = new Dictionary<Player, Vector3>();
    public Dictionary<Ball, Vector3> ballHistory = new Dictionary<Ball, Vector3>();
    
    public List<Player> playerList = new List<Player>();
    public List<Ball> ballList = new List<Ball>();
}