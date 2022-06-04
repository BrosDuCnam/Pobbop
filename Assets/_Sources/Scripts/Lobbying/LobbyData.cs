using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyData : MonoBehaviour
{
    public List<RoomPlayer> roomPlayers { get; } = new List<RoomPlayer>();
    public static event Action OnRoomUpdate;

    public static LobbyData instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void AddRoomPlayer(RoomPlayer roomPlayer)
    {
        roomPlayers.Add(roomPlayer);
        OnRoomUpdate?.Invoke();
    }
    
    public void RemoveRoomPlayer(RoomPlayer roomPlayer)
    {
        if (!roomPlayers.Contains(roomPlayer)) return;
        roomPlayers.Remove(roomPlayer);
        OnRoomUpdate?.Invoke();
    }
}
