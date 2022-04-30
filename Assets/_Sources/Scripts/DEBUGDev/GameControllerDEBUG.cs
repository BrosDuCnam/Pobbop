using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class GameControllerDEBUG : MonoBehaviour
{
    [SerializeField] private List<GameObject> _targets;
    [SerializeField] private List<BotPlayer> _bots;

    public List<GameObject> Targets { get => _targets; }

    public static GameControllerDEBUG Instance;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public static void AddPlayer(Player player)
    {
       foreach (BotPlayer bot in Instance._bots) 
       {
           bot.fsmStateInfo.playerList.Add(player);
       }
    }
}