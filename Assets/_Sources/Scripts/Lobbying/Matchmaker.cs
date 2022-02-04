using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Matchmaker : SteamLobby
{

    [SerializeField] private GameInfos.GameModes gameMode;
    
    [SerializeField]
    private float refreshRate = 1f;
    
    private float lastChecked;
    private bool ready;

    private CSteamID[] playerstoConnect;
    
    [SerializeField] private bool D_filterMatch;
    

    public void SetReady()
    {
        ready = true;
        HostLobby(ELobbyType.k_ELobbyTypePublic, GameInfos.GameModesPlayers[gameMode] - 1);
    }

    protected override void OnLobbyCreated(LobbyCreated_t callback)
    {
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "mode", gameMode.ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "game", "pobbop");
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "myPing", "oui");
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey,
            SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyJoinable(new CSteamID(callback.m_ulSteamIDLobby), true);
    }

    private void Update()
    {
        if (ready)
        {
            if (Time.time > lastChecked + refreshRate)
            {
                StartJoinLobby();

                foreach (CSteamID lobbyId in lobbyIDS)
                {
                    Debug.Log("lobby :: " + lobbyId);
                }
                
                //Is there enough players to create a match ?
                if (lobbyIDS.Count >= GameInfos.GameModesPlayers[gameMode])
                {
                    //If yes, decide who is the host then create the connection
                    for (int i = 0; i < GameInfos.GameModesPlayers[gameMode] - 1; i++)
                    {
                        playerstoConnect[i] = lobbyIDS[i];
                    }
                }
                lastChecked = Time.time;
            }
        }
    }

    public override void StartJoinLobby()
    {
        if (D_filterMatch)
            SteamMatchmaking.AddRequestLobbyListStringFilter("mode", gameMode.ToString(), ELobbyComparison.k_ELobbyComparisonEqual);
        base.StartJoinLobby();
    }
}
