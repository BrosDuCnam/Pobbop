using System;
using UnityEngine;

namespace UI.Host
{
    public class HostMenuPlayerData
    {
        public int PlayerId;
        public string Name;
        public int Ping;
        public int TeamIndex;
        public RoomPlayer RoomPlayer;
        
        public HostMenuPlayerData(int playerId, string name, int ping, int teamIndex, RoomPlayer roomPlayer)
        {
            PlayerId = playerId;
            Name = name;
            Ping = ping;
            TeamIndex = teamIndex;
            RoomPlayer = roomPlayer;
        }
    }
}