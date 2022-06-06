using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using JetBrains.Annotations;
using Mirror;
using Steamworks;
using UI;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

public class UiSceneSteamLobby : MonoBehaviour
{
    [SerializeField] private bool filterLobbies;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject camera;

    public string steamUsername = "DefaultName";

    public static UiSceneSteamLobby instance;

    protected NetworkManagerRefab networkManager;
    protected const string HostAdressKey = "HostAdress";
    protected string lobbyName = "Default name";
    
    protected CSteamID currentLobby = new CSteamID();
    protected List<CSteamID> lobbyIDS = new List<CSteamID>();

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyMatchList_t> lobbyListRetrieved;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdated;
    
    protected  virtual void Start()
    {
        camera.SetActive(true);

        if (!SteamManager.Initialized)
        {
            Debug.Log("Can't access to steam networks, steam may be offline");
            
            ErrorThrower.Instance.ThrowError("Can't access to steam networks, steam may be offline.\n" +
                                             "Please check your internet connection and try again, or restart steam");
            
            return;
        }
        MakeInstance();
        
        networkManager = GetComponent<NetworkManagerRefab>();

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyListRetrieved = Callback<LobbyMatchList_t>.Create(OnLobbyListRetrieved);
        lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
        
        steamUsername = SteamFriends.GetPersonaName();

    }

    protected void HostLobby(ELobbyType lobbyType, int maxPlayers)
    {
        SteamMatchmaking.CreateLobby(lobbyType, maxPlayers);
    }

    public void SwitchSceneHost()
    {
        HostLobby(ELobbyType.k_ELobbyTypePublic, 10);
    }
    
    public virtual void StartJoinLobby()
    {
        content.GetComponent<LobbyListManager>().ClearLobbyUIList();
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
        content.GetComponent<LobbyListManager>().DisplayLobbies(lobbyIDS, callback);
    }

    protected virtual void OnLobbyCreated(LobbyCreated_t callback)
    {
        networkManager.StartHost();
        currentLobby = new CSteamID(callback.m_ulSteamIDLobby);

        if (lobbyName == "Default name")
            lobbyName = steamUsername + "'s Lobby";
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
        currentLobby = new CSteamID(callback.m_ulSteamIDLobby);

        networkManager.networkAddress = hostAdress;
        networkManager.StartClient();
    }

    public void JoinLobby(CSteamID lobbyId)
    {
        SteamMatchmaking.JoinLobby(lobbyId);
        HostMenu.instance.HostMenuShowOnServer();
        HostMenu.instance.RequestAllData();
    }

    public void LeaveLobby()
    {
        if (currentLobby == null) return;

        SteamMatchmaking.LeaveLobby(currentLobby);

        if (HostMenu.instance.isServer)
            SteamMatchmaking.DeleteLobbyData(currentLobby, "game");
        
        networkManager.LeaveServer();
        HostMenu.instance.ClearPlayers();
    }

    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
}
