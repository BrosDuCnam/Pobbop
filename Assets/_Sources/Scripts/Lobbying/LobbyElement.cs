using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyElement : MonoBehaviour
{
    [SerializeField] private Text lobbyName;
    [SerializeField] private Text lobbyPlayerCount;

    public CSteamID lobbySteamID;

    public void JoinLobby()
    {
        SteamLobby.instance.JoinLobby(lobbySteamID);
    }
    
    public void SetLobbyName(string _lobbyName)
    {
        lobbyName.text = _lobbyName;
    }
    
    public void SetPlayerCount(string _lobbyPlayerCount, string maxLobbyPlayers)
    {
        lobbyPlayerCount.text = "Players : " + _lobbyPlayerCount + "/" + maxLobbyPlayers;
    }
}
