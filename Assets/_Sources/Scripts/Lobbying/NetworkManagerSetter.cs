using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerSetter : MonoBehaviour
{
    protected NetworkManagerLobby networkManager;
    

    private void Start()
    {
        networkManager = GetComponent<NetworkManagerLobby>();
    }
}
