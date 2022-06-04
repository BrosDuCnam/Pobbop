using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Mirror;
using TMPro;
using UI.Host;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace UI
{
    public class HostMenu : NetworkBehaviour
    {
        [SerializeField] public Host.HostMenuData hostMenuData;
        [SerializeField] public List<Host.HostMenuPlayerData> hostMenuPlayerData = new List<HostMenuPlayerData>();

        [Header("Team UI Objects")]
        [SerializeField]
        [Tooltip("The list of team UI objects, don't change do not exceed 4")]
        private GameObject[] _teamsObjects;

        [Tooltip("Do not exceed 2 teams")] [SerializeField]
        private RectTransformData[] _2teamLayout;

        [Tooltip("Do not exceed 3 teams")] [SerializeField]
        private RectTransformData[] _3teamLayout;

        [Tooltip("Do not exceed 4 teams")] [SerializeField]
        private RectTransformData[] _4teamLayout;

        [Header("UI Objects")] [SerializeField]
        private TextMeshProUGUI _teamAmountText;

        [Header("Prefabs")] [SerializeField] private GameObject _playerPrefab;

        [Header("Deactivate On Client")] [SerializeField]
        private GameObject _startGame;

        [SerializeField] private GameObject _teamAmount;
        [SerializeField] private GameObject _gameLimit;

        [Header("MenuPanels")] [SerializeField]
        private GameObject HostPanel;

        [SerializeField] private GameObject JoinPanel;

        [HideInInspector] public MainMenu _mainMenu;

        public static HostMenu instance;

        private int _scoreLimit;
        private float _timerLimit;
        private List<TextMeshProUGUI> lastTextUpdated = new List<TextMeshProUGUI>();
        private RoomProperties _roomProperties;

        [SerializeField] private GameObject networkManager;

        private void Awake()
        {
            if (instance == null) instance = this;
            _mainMenu = GetComponent<MainMenu>();
            _roomProperties = networkManager.GetComponent<RoomProperties>();
        }
        
        private void Start()
        {
            hostMenuData.TeamAmount = 2;

            UpdateUI();
        }

        public void HostGame()
        {
            NetworkManagerRefab.instance.StartHost();
            UpdateUI();
        }

        private void HostMenuShowOnServer()
        {
            _startGame.SetActive(isServer);
            _teamAmount.SetActive(isServer);
            _gameLimit.SetActive(isServer);
        }

        public void AddPlayer(int playerId, string name, int ping, int teamIndex, RoomPlayer roomPlayer)
        {
            HostMenuPlayerData player = new HostMenuPlayerData(playerId, name, ping, teamIndex, roomPlayer);
            hostMenuPlayerData.Add(player);
            UpdateTeamUI();
        }


        public void UpdateUI()
        {
            _teamAmountText.text = hostMenuData.TeamAmount.ToString();
            HostMenuShowOnServer();
            UpdateTeamUI();
        }

        public void IncreaseTeamSize()
        {
            if (hostMenuData.TeamAmount < 4)
            {
                hostMenuData.TeamAmount++;
                UpdateUI();
            }
        }

        public void DecreaseTeamSize()
        {
            if (hostMenuData.TeamAmount > 2)
            {
                hostMenuData.TeamAmount--;
                UpdateUI();
            }
        }

        private void UpdateTeamUI()
        {
            switch (hostMenuData.TeamAmount)
            {
                case 2:
                    SetLayout(_2teamLayout);
                    break;
                case 3:
                    SetLayout(_3teamLayout);
                    break;
                case 4:
                    SetLayout(_4teamLayout);
                    break;
            }

            foreach (GameObject teamObject in _teamsObjects)
            {
                Transform layout = teamObject.transform.GetChild(1).transform;
                for (int i = 0; i < layout.childCount; i++)
                {
                    Destroy(layout.GetChild(i).gameObject);
                }
            }

            foreach (HostMenuPlayerData playerData in hostMenuPlayerData)
            {
                GameObject player = GetPlayerObject(playerData.PlayerId);

                player = Instantiate(_playerPrefab,
                    _teamsObjects[playerData.TeamIndex].transform.GetChild(1)); //Instantiate the player
                player.name = "Player_" + playerData.PlayerId; //Set the name of the player
                player.GetComponentsInChildren<TextMeshProUGUI>()[0].text = playerData.Name; //Set the player name
                player.GetComponentsInChildren<TextMeshProUGUI>()[1].text =
                    playerData.Ping.ToString(); //Set the player ping
            }
        }

        public GameObject GetPlayerObject(int playerID)
        {
            for (int i = 0; i < hostMenuData.TeamAmount; i++)
            {
                foreach (Transform player in _teamsObjects[i].transform.GetChild(1).transform)
                {
                    if (player.name == "Player_" + playerID)
                    {
                        return player.gameObject;
                    }
                }
            }

            return null;
        }

        public void ChangeUsernameById(int id, string username)
        {
            foreach (HostMenuPlayerData playerData in hostMenuPlayerData)
            {
                if (playerData.PlayerId == id)
                {
                    playerData.Name = username;
                    UpdateTeamUI();
                }
            }
        }

        private void SetLayout(RectTransformData[] layout)
        {
            for (int i = 0; i < _teamsObjects.Length; i++)
            {
                if (i < layout.Length)
                {
                    _teamsObjects[i].SetActive(true);

                    if (i > hostMenuData.TeamAmount - 1)
                    {
                        int index = i;
                        layout[i].PushToTransform(_teamsObjects[i].GetComponent<RectTransform>(), 1f, Ease.OutExpo,
                            null, () => { _teamsObjects[index].SetActive(false); });
                    }
                    else
                    {
                        int index = i;

                        layout[i].PushToTransform(_teamsObjects[i].GetComponent<RectTransform>(), 1f, Ease.OutExpo,
                            () => { _teamsObjects[index].SetActive(true); });
                    }
                }
            }
        }

        public void ChangeLocalPlayerTeam(int teamId)
        {
            RoomPlayer localPlayer = NetworkClient.localPlayer.GetComponent<RoomPlayer>();
            localPlayer.CmdChangeTeam(teamId);
        }

        public void ChangePlayerTeam(int playerId, int teamId)
        {
            HostMenuPlayerData playerData = hostMenuPlayerData.Find(x => x.PlayerId == playerId);
            playerData.TeamIndex = teamId;
            print(playerData.TeamIndex);
            UpdateUI();
        }

        public void ChangeTeamNum(bool increase)
        {
            RoomPlayer localPlayer = NetworkClient.localPlayer.GetComponent<RoomPlayer>();
            localPlayer.CmdChangeTeamNum(increase);
            UpdateUI();
        }

        public void StartGame()
        {
            if (isServer)
            {
                NetworkManagerRefab.instance.StartGame();
            }
        }

        public void ClearPlayers()
        {
            hostMenuPlayerData = new List<HostMenuPlayerData>();
        }

        public void RedirectOnHostPage()
        {
            _mainMenu.OpenSubMenu(JoinPanel, true);
            _mainMenu.OpenSubMenuAnimated(HostPanel);
        }

        public void OnChangeGameLimitMode(GameObject gameLimitModes)
        {
            _scoreLimit = 0;
            _timerLimit = 0f;

            TMP_Dropdown dropdown = _gameLimit.GetComponentInChildren<TMP_Dropdown>();

            foreach (TextMeshProUGUI textArea in lastTextUpdated)
            {
                textArea.text = "0";
            }

            for (int i = 0; i < gameLimitModes.transform.childCount; i++)
            {
                if (dropdown.value == i)
                {
                    gameLimitModes.transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    gameLimitModes.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        public void IncreaseScoreLimit(TextMeshProUGUI textArea)
        {
            _scoreLimit += 5;
            textArea.text = _scoreLimit.ToString();
            _roomProperties.CmdChangeScoreLimit(_scoreLimit);
            lastTextUpdated.Remove(textArea);
            lastTextUpdated.Add(textArea);
        }
        
        public void DecreaseScoreLimit(TextMeshProUGUI textArea)
        {
            if (_scoreLimit > 0)
            {
                _scoreLimit -= 5;
            }
            textArea.text = _scoreLimit.ToString();
            _roomProperties.CmdChangeScoreLimit(_scoreLimit);
            lastTextUpdated.Remove(textArea);
            lastTextUpdated.Add(textArea);
        }
        
        public void IncreaseTimerLimit(TextMeshProUGUI textArea)
        {
            _timerLimit += 5;
            textArea.text = _timerLimit.ToString();
            _roomProperties.CmdChangeTimerLimit(_timerLimit);
            lastTextUpdated.Remove(textArea);
            lastTextUpdated.Add(textArea);
        }
        
        public void DecreaseTimerLimit(TextMeshProUGUI textArea)
        {
            if (_timerLimit > 0)
            {
                _timerLimit -= 5;
            }
            textArea.text = _timerLimit.ToString();
            _roomProperties.CmdChangeTimerLimit(_timerLimit);
            lastTextUpdated.Remove(textArea);
            lastTextUpdated.Add(textArea);
        }

        public bool IsServer()
        {
            return isServer;
        }
    }
}