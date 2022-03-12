using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HostMenu : MonoBehaviour
    {
        [SerializeField] public int teamAmount = 2;

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
        
        private void Start()
        {
            UpdateTeamUI();
        }

        private void Update()
        {
            _teamAmountText.text = teamAmount.ToString();
        }
        
        
        public void IncreaseTeamSize()
        {
            if (teamAmount < 4)
            {
                teamAmount++;
                UpdateTeamUI();
            }
        }
        
        public void DecreaseTeamSize()
        {
            if (teamAmount > 2)
            {
                teamAmount--;
                UpdateTeamUI();
            }
        }
        
        private void UpdateTeamUI()
        {
            switch (teamAmount)
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
        }
        
        private void SetLayout(RectTransformData[] layout)
        {
            for (int i = 0; i < _teamsObjects.Length; i++)
            {
                if (i < layout.Length)
                {   
                    _teamsObjects[i].SetActive(true);
                    
                    if (i > teamAmount - 1)
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