using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerRefab : NetworkManager
{
    [Scene] [SerializeField] private string menuScene = string.Empty;
    [Scene] [SerializeField] private string menuSceneAlt = "default";
    [Scene] [SerializeField] private string gameScene = string.Empty;
    [Scene] [SerializeField] private string tutoScene = string.Empty;
    [Scene] [SerializeField] private string winScene = string.Empty;

    [Header("Room")]
    [SerializeField] private GameObject roomPlayerPrefab;    
    
    [Header("Game")]
    [SerializeField] private GameObject gamePlayerPrefab;

    
    public static NetworkManagerRefab instance;
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    
    public static event Action OnStartGame;
    public static event Action OnEndGame;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        HostMenu.instance.RedirectOnHostPage();
        base.OnStartClient();
    }
    
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        NetworkClient.AddPlayer();
        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect() {
        base.OnClientDisconnect();
        OnClientDisconnected?.Invoke();
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        
        NetworkServer.DisconnectAll();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        print("addPlayer");
        if (SceneManager.GetActiveScene().path == menuScene || SceneManager.GetActiveScene().path == menuSceneAlt)
        {
            GameObject roomPlayerInstance = Instantiate(roomPlayerPrefab);
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
        else if (SceneManager.GetActiveScene().path == tutoScene)
        {
            print("spawn");
            GameObject gamePlayerInstance = Instantiate(gamePlayerPrefab, new Vector3 (-20, 0, 10), new Quaternion());
            NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance.gameObject);
        }
        else if (SceneManager.GetActiveScene().path == gameScene)
        {
            print("spawn");
            GameObject gamePlayerInstance = Instantiate(gamePlayerPrefab);
            NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance.gameObject);
        }
    }
    
    public Transform GetRespawnPosition()
    {
        if (SceneManager.GetActiveScene().path == tutoScene)
        {
            return GameObject.Find("TutoSpawn").transform;
        }
        
        float distance = float.MinValue;
        Transform spawnPoint = new GameObject().transform;
        List<Transform> allPlayers = GameManager.instance.GetAllPlayers().Select(x => x.transform).ToList();
        //Fix for host not registering spawnpoints
        if (startPositions.Count == 0)
        {
            startPositions = FindObjectsOfType<NetworkStartPosition>().Select(x => x.transform).ToList();
        }
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

    public override void ServerChangeScene(string newSceneName)
    {
        if (newSceneName == gameScene || newSceneName == tutoScene)
        {
            List<RoomPlayer> roomPlayers = HostMenu.instance.hostMenuPlayerData.Select(x => x.RoomPlayer).ToList();
            int indexCount = 0;
            foreach (RoomPlayer roomPlayer in roomPlayers)
            {
                NetworkConnection conn = roomPlayer.connectionToClient;
                
                Vector3 spawnPos = newSceneName == gameScene ? startPositions[indexCount].position : new Vector3(-20, 0, 10);
                Quaternion spawnRot = newSceneName == gameScene ? startPositions[indexCount].rotation : new Quaternion();
                GameObject gamePlayerInstance = Instantiate(gamePlayerPrefab, spawnPos, spawnRot);
                RealPlayer player = gamePlayerInstance.GetComponent<RealPlayer>();
                player.teamId = roomPlayer.teamId;
                player.username = roomPlayer.username;
                
                NetworkServer.Destroy(conn.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance);
                    
                indexCount++;
            }
        }
        else if (newSceneName == winScene)
        {
            
        }
        DontDestroyOnLoad(RoomProperties.instance);
        base.ServerChangeScene(newSceneName);
    }

    public void LeaveServer()
    {
        if (NetworkClient.localPlayer != null)
        {

            if (NetworkClient.localPlayer.isServer)
            {
                NetworkClient.Shutdown();
                NetworkServer.Shutdown();
                StopHost();
                StopServer();
            }
            else
            {
                NetworkClient.Shutdown();
                StopClient();
            }
        }
        HostMenu.instance.gameObject.SetActive(true);
    }

    public void ReturnToMenu()
    {
        ServerChangeScene(menuScene);

        if (NetworkClient.localPlayer != null)
        {
            if (NetworkClient.localPlayer.isServer)
            {
                NetworkClient.Shutdown();
                NetworkServer.Shutdown();
                StopHost();
                StopServer();
            }
            else
            {
                NetworkClient.Shutdown();
                StopClient();
            }
        }
        Destroy(gameObject);
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            ServerChangeScene(gameScene);
            OnStartGame?.Invoke();
        }
    }

    public void StartTuto()
    {
        StartHost();

        ServerChangeScene(tutoScene);
        OnStartGame?.Invoke();
    }

    public void EndGame()
    {
        print("GameEnded");
        OnEndGame?.Invoke();
    }
}
