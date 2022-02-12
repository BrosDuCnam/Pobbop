using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListManager : MonoBehaviour
{
    // UI Prefab to create the list of lobbies 
    [SerializeField] private GameObject lobbyElementGO;

    /// <summary>
    /// Creates a list (UI) of all the lobbies given with the name, the number of players and the max number of players 
    /// </summary>
    /// <param name="lobbyIDS">List of steam lobby ID to display</param>
    /// <param name="callback"></param>
    public void DisplayLobbies(List<CSteamID> lobbyIDS, LobbyDataUpdate_t callback)
    {
        for (int i = 0; i < lobbyIDS.Count; i++)
        {
            if (lobbyIDS[i].m_SteamID == callback.m_ulSteamIDLobby)
            {
                string lobbyName = SteamMatchmaking.GetLobbyData((CSteamID) lobbyIDS[i].m_SteamID, "name");
                GameObject go = Instantiate(lobbyElementGO, gameObject.transform);
                
                LobbyElement lobbyElement = go.GetComponent<LobbyElement>();
                lobbyElement.lobbySteamID = (CSteamID) lobbyIDS[i].m_SteamID;
                lobbyElement.SetLobbyName(lobbyName);
                lobbyElement.SetPlayerCount(SteamMatchmaking.GetNumLobbyMembers((CSteamID) lobbyIDS[i].m_SteamID).ToString(),
                    (SteamMatchmaking.GetLobbyMemberLimit((CSteamID) lobbyIDS[i].m_SteamID).ToString()));
            }
        }
    }
}
