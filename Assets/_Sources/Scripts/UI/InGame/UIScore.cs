using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIScore : MonoBehaviour
{
    [SerializeField] private Color _warningColor;
    private Color _defaultColor;
    private bool _isWarning;
    
    [SerializeField] private AnimationCurve _warningCurve;
    private float _warningTimer;
    private float _warningTimerLimit;
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
            if (_warningTimer > _warningTimerLimit)
            {
                _warningTimer = 0;
                _warningTimerLimit = _warningCurve.Evaluate(gameManager.timer)/2;
                _isWarning = !_isWarning;
                _fillImage.color = Color.Lerp(Color.red, Color.white, _warningCurve.Evaluate(_warningTimer / _warningTimerLimit));
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