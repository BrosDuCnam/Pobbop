using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HostMenu : MonoBehaviour
    {
        [SerializeField] public Host.HostMenuData hostMenuData;
        [SerializeField] public List<Host.HostMenuPlayerData> hostMenuPlayerData;

        [Header("Team UI Objects")] [SerializeField] [Tooltip("The list of team UI objects, don't change do not exceed 4")]
        private GameObject[] _teamsObjects;
        
        [Tooltip("Do not exceed 2 teams")]
        [SerializeField] private RectTransformData[] _2teamLayout;
        [Tooltip("Do not exceed 3 teams")]
        [SerializeField] private RectTransformData[] _3teamLayout;
        [Tooltip("Do not exceed 4 teams")]
        [SerializeField] private RectTransformData[] _4teamLayout;

        [Header("UI Objects")] 
        [SerializeField] private TextMeshProUGUI _teamAmountText;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;
        
        private void Start()
        {
            hostMenuData.TeamAmount = 2;

            UpdateUI();
        }

        public void UpdateUI()
        {
            _teamAmountText.text = hostMenuData.TeamAmount.ToString();

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

            for (var i = 0; i < hostMenuPlayerData.Count; i++)
            {
                GameObject player = GetPlayerObject(hostMenuPlayerData[i].PlayerId);
                if (player != null) //If player has already been instantied
                {
                    int actualTeam = Array.IndexOf(_teamsObjects, player.transform.parent.parent); //Get the actual team
                    if (actualTeam != hostMenuPlayerData[i].TeamIndex) //If the team has changed
                    {
                        player.transform.SetParent(_teamsObjects[hostMenuPlayerData[i].TeamIndex].transform.GetChild(1)); //Move the player to the new team
                    }
                }
                else
                {
                    player = Instantiate(_playerPrefab, _teamsObjects[hostMenuPlayerData[i].TeamIndex].transform.GetChild(1)); //Instantiate the player
                    player.name = "Player_" + hostMenuPlayerData[i].PlayerId; //Set the name of the player
                    player.GetComponentsInChildren<TextMeshProUGUI>()[0].text = hostMenuPlayerData[i].Name; //Set the player name
                    player.GetComponentsInChildren<TextMeshProUGUI>()[1].text = hostMenuPlayerData[i].Ping.ToString(); //Set the player ping
                }
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
                        layout[i].PushToTransform(_teamsObjects[i].GetComponent<RectTransform>(), 1f, Ease.OutExpo, null, () =>
                        {
                            _teamsObjects[index].SetActive(false);
                        });
                    }
                    else
                    {
                        int index = i;

                        layout[i].PushToTransform(_teamsObjects[i].GetComponent<RectTransform>(), 1f, Ease.OutExpo, () =>
                        {
                            _teamsObjects[index].SetActive(true);
                        });
                    }
                }
            }
        }
    }
}