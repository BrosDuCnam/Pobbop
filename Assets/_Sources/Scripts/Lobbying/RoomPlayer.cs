using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UI;
using UI.Host;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class RoomPlayer : NetworkBehaviour
{
    [SyncVar] public string username;
    public int myId;
    public int teamId;

    private HostMenuPlayerData me;

    private void Start()
    {
        if (isLocalPlayer)
        {
            if (FindObjectOfType<UiSceneSteamLobby>() != null) //For local (when steam isn't initialized)
                username = FindObjectOfType<UiSceneSteamLobby>().steamUsername;
        }
        
        
        myId = (int) GetComponent<NetworkIdentity>().netId;
        HostMenu.instance.AddPlayer(myId, username, 0, teamId, this);
    }
    
    
    [Command]
    private void CmdChangePlayerUsername(int id, string username)
    {
        RpcChangePlayerUsername(id, username);
    }
    [ClientRpc]
    private void RpcChangePlayerUsername(int id, string username)
    {
        HostMenu.instance.ChangeUsernameById(id, username);
    }
    
    [Command]
    public void CmdChangeTeam(int teamId)
    {
        RpcChangeTeam(myId, teamId);
    }
    [ClientRpc]
    public void RpcChangeTeam(int playerId, int teamId)
    {
        this.teamId = teamId;
        HostMenu.instance.ChangePlayerTeam(playerId, teamId);
    }

    [Command]
    public void CmdChangeTeamNum(bool increase)
    {
        RpcChangeTeamNum(increase);
    }
    [ClientRpc]
    private void RpcChangeTeamNum(bool increase)
    {
        if(increase)
            HostMenu.instance.IncreaseTeamSize();
        else
            HostMenu.instance.DecreaseTeamSize();
        
    }
}
