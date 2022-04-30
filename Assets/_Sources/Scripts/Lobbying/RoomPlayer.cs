using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UI;
using UI.Host;
using UnityEngine;

public class RoomPlayer : NetworkBehaviour
{
    public string username = "Player ";
    public int myId;
    public int teamId;

    private HostMenuPlayerData me;

    private void Start()
    {
        myId = (int) GetComponent<NetworkIdentity>().netId;
        username += myId;
        HostMenu.instance.AddPlayer(myId, username, 0, teamId, this);
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
