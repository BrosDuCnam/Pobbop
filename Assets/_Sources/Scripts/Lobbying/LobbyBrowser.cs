using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class LobbyBrowser : SteamLobby
{
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject lobbyList;

    protected override void Start()
    {
        base.Start();
        lobbyList.SetActive(false);
    }

    public void HostMyLobby()
    {
        buttons.SetActive(false);
        HostLobby(ELobbyType.k_ELobbyTypePublic, networkManager.maxConnections);
    }

    public override void StartJoinLobby()
    {
        buttons.SetActive(false);
        lobbyList.SetActive(true);
        base.StartJoinLobby();
    }

    protected override void OnGetLobbyInfo(LobbyDataUpdate_t callback)
    {
        base.OnGetLobbyInfo(callback);
        lobbyList.GetComponent<LobbyListManager>().DisplayLobbies(lobbyIDS, callback);
    }

    protected override void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            buttons.SetActive(true);
            return;
        }
        base.OnLobbyCreated(callback);
    }

    protected override void OnLobbyEntered(LobbyEnter_t callback)
    {
        base.OnLobbyEntered(callback);
        buttons.SetActive(false);
    }
}
