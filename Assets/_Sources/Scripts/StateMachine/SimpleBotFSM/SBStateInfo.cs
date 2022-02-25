using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SBStateInfo : FSMStateInfo
{
    public BotPlayer bot;

    public BotController controller
    {
        get => (BotController) bot.controller;
    }
    
    public Dictionary<BasePlayer, Vector3> playerHistory = new Dictionary<BasePlayer, Vector3>();
    public Dictionary<ThrowableObject, Vector3> ballHistory = new Dictionary<ThrowableObject, Vector3>();
    
    public List<BasePlayer> playerList = new List<BasePlayer>();
    public List<ThrowableObject> ballList = new List<ThrowableObject>();
}