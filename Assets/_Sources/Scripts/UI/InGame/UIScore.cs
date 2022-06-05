using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIScore : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    
    [SerializeField] private List<UIScoreTeam> teams = new List<UIScoreTeam>();
    
    RoomProperties roomProperties;
    GameManager gameManager;

    private void Start()
    {
        roomProperties = RoomProperties.instance;
        gameManager = GameManager.instance;
        
        
        // Disable the score UI if there are no teams
        for (int i = 0; i < teams.Count; i++)
        {
            print(teams[i].name);
            if (i >= gameManager.teams.Count)
            {
                teams[i].gameObject.SetActive(false);
            }
            else
            {
                teams[i].maxScore = roomProperties.scoreLimit;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < gameManager.teams.Values.Count; i++)
        {
            teams[i].Score = gameManager.teams.Values.ToList()[i];
        }

        if (roomProperties.gameLimitMode != RoomProperties.GameLimitModes.Score)
        {
            _fillImage.fillAmount = 1 - gameManager.timer / roomProperties.timerLimit;
        }
        else
        {
            _fillImage.fillAmount = 1;
        }
    }
    
}