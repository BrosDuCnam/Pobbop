using System;
using UnityEngine;

namespace UI.Host
{
    [Serializable]
    public struct HostMenuPlayerData
    {
        public int PlayerId;
        public string Name;
        public int Ping;
        public int TeamIndex;
    }
}