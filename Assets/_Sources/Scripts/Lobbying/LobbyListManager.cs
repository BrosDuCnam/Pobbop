using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListManager : MonoBehaviour
{
    [SerializeField] private GameObject lobbyElementGO;


    public void DisplayLobbies(List<CSteamID> lobbyIDS, LobbyDataUpdate_t callback)
    {
        for (int i = 0; i < lobbyIDS.Count; i++)
        {
            if (lobbyIDS[i].m_SteamID == callback.m_ulSteamIDLobby)
            {
                string lobbyName = SteamMatchmaking.GetLobbyData((CSteamID) lobbyIDS[i].m_SteamID, "name");
                GameObject go = Instantiate(lobbyElementGO, gameObject.transform);
                
                Debug.Log("Lobby " + i + " :: " + "name : " + lobbyName + " | game : " + SteamMatchmaking.GetLobbyData((CSteamID) lobbyIDS[i].m_SteamID, "game"));
                LobbyElement lobbyElement = go.GetComponent<LobbyElement>();
                lobbyElement.lobbySteamID = (CSteamID) lobbyIDS[i].m_SteamID;
                lobbyElement.SetLobbyName(lobbyName);
                lobbyElement.SetPlayerCount(SteamMatchmaking.GetNumLobbyMembers((CSteamID) lobbyIDS[i].m_SteamID).ToString(),
                    (SteamMatchmaking.GetLobbyMemberLimit((CSteamID) lobbyIDS[i].m_SteamID).ToString()));
            }
        }
    }
}
