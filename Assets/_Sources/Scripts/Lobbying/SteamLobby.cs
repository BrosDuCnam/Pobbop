using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] private bool filterLobbies;
    
    public static SteamLobby instance;

    protected NetworkManager networkManager;
    protected const string HostAdressKey = "HostAdress";
    protected string lobbyName = "Default name";
    
    protected List<CSteamID> lobbyIDS = new List<CSteamID>();

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyMatchList_t> lobbyListRetrieved;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdated;
    

    protected  virtual void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Can't access to steam networks");
            return;
        }
        MakeInstance();
        
        networkManager = GetComponent<NetworkManager>();

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyListRetrieved = Callback<LobbyMatchList_t>.Create(OnLobbyListRetrieved);
        lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);

    }

    protected void HostLobby(ELobbyType lobbyType, int maxPlayers)
    {
        SteamMatchmaking.CreateLobby(lobbyType, maxPlayers);
    }

    public virtual void StartJoinLobby()
    {
        if (filterLobbies)
        {
            SteamMatchmaking.AddRequestLobbyListStringFilter("game", "pobbop", ELobbyComparison.k_ELobbyComparisonEqual);
        }
        SteamAPICall_t tryGetList = SteamMatchmaking.RequestLobbyList();
    }
    

    private void OnLobbyListRetrieved(LobbyMatchList_t callback)
    {
        lobbyIDS.Clear();
        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDS.Add(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }

    protected virtual void OnGetLobbyInfo(LobbyDataUpdate_t callback)
    {
        //Stuff
    }

    protected virtual void OnLobbyCreated(LobbyCreated_t callback)
    {
        networkManager.StartHost();

        if (lobbyName == "Default name")
            lobbyName = SteamFriends.GetPersonaName() + "'s Lobby";
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", lobbyName);
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "game", "pobbop");
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey,
            SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyJoinable(new CSteamID(callback.m_ulSteamIDLobby), true);
    }

    protected void SetLobbyName(string _lobbyName)
    {
        lobbyName = _lobbyName;
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    protected virtual void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) {return;}

        string hostAdress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey);
        networkManager.networkAddress = hostAdress;
        networkManager.StartClient();
    }

    public void JoinLobby(CSteamID lobbyId)
    {
        SteamMatchmaking.JoinLobby(lobbyId);
    }
    
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
}
