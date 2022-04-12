using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] Behaviour[] compToDisableOnClients;
    [SerializeField] Behaviour[] compToDisableOnLocal;
    [SerializeField] private GameObject[] gameObjectsToDeactivateOnClients;
    [SerializeField] private GameObject[] gameObjectsToDeactivateOnLocal;
    
    private void Start()
    {
        if (!isLocalPlayer)
        {
            DisableOnClients();
        }
        else
        {
            DisableOnLocal();
            CmdSetUsername(transform.name, "default");
        }
    }
    private void DisableOnLocal()
    {
        foreach (GameObject gameObject in gameObjectsToDeactivateOnLocal)
        {
            gameObject.SetActive(false);
        }
        foreach (Behaviour behaviour in compToDisableOnLocal)
        {
            behaviour.enabled = false;
        }
    }
    
    private void DisableOnClients()
    {
        foreach (GameObject gameObject in gameObjectsToDeactivateOnClients)
        {
            gameObject.SetActive(false);
        }
        foreach (Behaviour behaviour in compToDisableOnClients)
        {
            behaviour.enabled = false;
        }
    }
    

    [Command]
    void CmdSetUsername(string playerID, string username)
    {
        Player player = GameManager.GetPlayer(playerID);
        if (player != null)
        {
            Debug.Log(username + " joined");
            player.username = username;
        }
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        string netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player player = GetComponent<Player>();

        GameManager.RegisterPlayer(netID, player);
    }
    
    private void OnDisable()
    {
        GameManager.UnRegisterPlayer(transform.name);
    }
}
