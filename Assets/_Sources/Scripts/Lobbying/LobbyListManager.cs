using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListManager : MonoBehaviour
{
    [SerializeField] private GameObject lobbyElement;


    public void DisplayLobbies(List<CSteamID> lobbyIDS, LobbyDataUpdate_t callback)
    {
        for (int i = 0; i < lobbyIDS.Count; i++)
        {
            if (lobbyIDS[i].m_SteamID == callback.m_ulSteamIDLobby)
            {
                string lobbyName = SteamMatchmaking.GetLobbyData((CSteamID) lobbyIDS[i].m_SteamID, "name");
                Debug.Log("Lobby " + i + " :: " + lobbyName);
                GameObject go = Instantiate(lobbyElement, gameObject.transform);

                go.GetComponentInChildren<Text>().text = lobbyName;
            }
        }
    }
}
