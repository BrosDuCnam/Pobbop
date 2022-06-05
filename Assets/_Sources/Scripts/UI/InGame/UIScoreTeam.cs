using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScoreTeam : MonoBehaviour
{
     private RoomProperties roomProperties;
     [SerializeField] private int _score;
     public int Score
     {
          get { return _score; }
          set
          {
               _score = value;
               UpdateUI();
          }
     }

     [SerializeField] private TextMeshProUGUI _scoreText;
     [SerializeField] private Image _fillImage;
     public int maxScore;

     private void Start()
     {
          roomProperties = RoomProperties.instance;

          if (roomProperties.gameLimitMode == RoomProperties.GameLimitModes.Timer)
          {
               _fillImage.fillAmount = 1;
          }
          
          Score = 0;
     }
     
     public void UpdateUI()
     {
          if (roomProperties == null) roomProperties = RoomProperties.instance;
          
          if (roomProperties.gameLimitMode != RoomProperties.GameLimitModes.Timer)
          {
               _fillImage.fillAmount = (float) Score / maxScore;
          }
          _scoreText.text = Score.ToString();
     }
}
