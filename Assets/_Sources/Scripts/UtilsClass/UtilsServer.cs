using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UtilsServer : NetworkBehaviour
{
    public static UtilsServer Instance;

    private void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    
    public void SetHost()
    {
        
    }
    
    public void GetHost()
    {
        
    }

    [Command (requiresAuthority = false)]
    public static void SyncValue<T>(ref T parentValue, T value)
    {
        RpcSyncValue(ref parentValue, value);
    }
    
    [ClientRpc]
    private static void RpcSyncValue<T>(ref T parentValue, T value)
    {
        parentValue = value;
    }
}
