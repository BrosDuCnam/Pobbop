using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject lobbyList;
    [SerializeField] private bool filterLobbies = false;
    [SerializeField] private bool SearchLobby = false;

    
    public static SteamLobby instance;

    private NetworkManager networkManager;
    private const string HostAdressKey = "HostAdress";
    
    private List<CSteamID> lobbyIDS = new List<CSteamID>();

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyMatchList_t> lobbyListRetrieved;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdated;

    private void Update()
    {
        //Debug
        if (SearchLobby)
        {
            JoinLobby();
            SearchLobby = false;
        }
    }

    private void Start()
    {
        if (!SteamManager.Initialized) {return;}
        MakeInstance();
        
        networkManager = GetComponent<NetworkManager>();

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyListRetrieved = Callback<LobbyMatchList_t>.Create(OnLobbyListRetrieved);
        lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
        
        
        lobbyList.SetActive(false);
    }

    public void HostLobby()
    {
        buttons.SetActive(false);
        
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, networkManager.maxConnections);
    }

    public void JoinLobby()
    {
        buttons.SetActive(false);
        lobbyList.SetActive(true);
        if (filterLobbies)
        {
            SteamMatchmaking.AddRequestLobbyListStringFilter("game", "pobbop", ELobbyComparison.k_ELobbyComparisonEqual);
        }
        SteamAPICall_t tryGetList = SteamMatchmaking.RequestLobbyList();
    }

    private void OnLobbyListRetrieved(LobbyMatchList_t callback)
    {
        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDS.Add(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }

    private void OnGetLobbyInfo(LobbyDataUpdate_t callback)
    {
        lobbyList.GetComponent<LobbyListManager>().DisplayLobbies(lobbyIDS, callback);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            buttons.SetActive(true);
            return;
        }
        
        networkManager.StartHost();

        SteamMatchmaking.SetLobbyJoinable(new CSteamID(callback.m_ulSteamIDLobby), true);
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", "Default Name");
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "game", "pobbop");
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey,
            SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyJoinable(new CSteamID(callback.m_ulSteamIDLobby), true);
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) {return;}

        string hostAdress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey);
        networkManager.networkAddress = hostAdress;
        networkManager.StartClient();
        
        buttons.SetActive(false);
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
