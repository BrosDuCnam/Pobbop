using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerRefab : NetworkManager
{
    [Scene] [SerializeField] private string menuScene = string.Empty;

    [Header("Room")]
    [SerializeField] private RoomPlayer roomPlayerPrefab;
    
    public static NetworkManagerRefab instance;
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;


    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        base.OnStartClient();
    }
    
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect() {
        base.OnClientDisconnect();
        OnClientDisconnected?.Invoke();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        
        if (SceneManager.GetActiveScene().name == menuScene)
        {
            bool isLeader = LobbyData.instance.roomPlayers.Count == 0;

            RoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);
            
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
            print("addedRoomPlayer");
        }
    }
    
    public Transform GetRespawnPosition()
    {
        float distance = float.MinValue;
        Transform spawnPoint = new GameObject().transform;
        List<Transform> allPlayers = GameManager.GetAllPlayers().Select(x => x.transform).ToList();
        //Get the spawnpoint that id the furthest away from all players
        foreach (Transform spawnPointTransform in startPositions)
        {
            float tempDistance = 0;
            foreach (Transform player in allPlayers)
            {
                tempDistance += Vector3.Distance(spawnPointTransform.position, player.position);
            }
            if (tempDistance > distance)
            {
                distance = tempDistance;
                spawnPoint = spawnPointTransform;
            }
        }
        return spawnPoint;
    }

}
