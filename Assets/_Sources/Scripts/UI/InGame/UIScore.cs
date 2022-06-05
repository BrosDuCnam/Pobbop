using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIScore : MonoBehaviour
{
    [SerializeField] private Color _warningColor;
    private Color _defaultColor;

    [SerializeField] private float _startWarningTime;
    private float _warningTimer;
    private float _warningTimerLimit;
    private bool _isWarning;
    
    [SerializeField] private Image _fillImage;
    
    [SerializeField] private List<UIScoreTeam> teams = new List<UIScoreTeam>();
    
    RoomProperties roomProperties;
    GameManager gameManager;

    private void Start()
    {
        roomProperties = RoomProperties.instance;
        gameManager = GameManager.instance;
        
        _defaultColor = _fillImage.color;
        
        // Disable the score UI if there are no teams
        for (int i = 0; i < teams.Count; i++)
        {
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
            
            _warningTimer += Time.deltaTime;
            if (_warningTimer > _warningTimerLimit && gameManager.SecondsUntilGameEnds < _startWarningTime)
            {
                _warningTimer = 0;
                _warningTimerLimit = Utils.Map(_startWarningTime, 0, 1, .1f, gameManager.SecondsUntilGameEnds);
                _isWarning = !_isWarning;
            }
            
            if (_isWarning)
            {
                _fillImage.color = _warningColor;
            }
            else
            {
                _fillImage.color = _defaultColor;
            }
        }
        else
        {
            _fillImage.fillAmount = 1;
        }
    }

}