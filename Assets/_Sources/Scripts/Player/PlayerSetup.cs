using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] Behaviour[] compToDisableOnClients;
    [SerializeField] Behaviour[] compToDisableOnLocal;
    [SerializeField] private GameObject[] gameObjectsToDeactivateOnClients;
    [SerializeField] private GameObject[] gameObjectsToDeactivateOnLocal;
    [SerializeField] private RawImage teamArrow;
    [SerializeField] private Transform playerCam;
    
    private void Start()
    {
        if (!isLocalPlayer)
        {
            DisableOnClients();
        }
        else
        {
            DisableOnLocal();
        }
        
        //Set team arrow color based on team
        Player player = GetComponent<Player>();
        Player localPlayer = NetworkClient.localPlayer.GetComponent<Player>();
        int localId = localPlayer.teamId;
        teamArrow.color = localId == player.teamId ? Color.blue : Color.red;
        LookAtCamera lookAtCam = teamArrow.canvas.GetComponent<LookAtCamera>();
        lookAtCam.camera = localPlayer.playerCam.transform;
        lookAtCam.usernameText.text = localPlayer.username;
        if (player == localPlayer)
        {
            teamArrow.canvas.gameObject.SetActive(false);
        }
    }
    
    public void Disconnect()
    {
        // TODO: Disconnect player and change scene
        // GetComponent<NetworkIdentity>().connectionToServer.Disconnect();
        // SceneManager.LoadScene(0);
        NetworkManagerRefab.instance.ReturnToMenu();
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

    public override void OnStartClient()
    {
        base.OnStartClient();

        string netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player player = GetComponent<Player>();
        GameManager.instance.RegisterPlayer(netID, player);

    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        
        string playerID = "Player" + GetComponent<NetworkIdentity>().netId;
        GameManager.instance.UnRegisterPlayer(playerID);
        SceneManager.LoadScene(0);
    }
    

    private void OnDisable()
    {
        GameManager.instance.UnRegisterPlayer(transform.name);
    }
}
