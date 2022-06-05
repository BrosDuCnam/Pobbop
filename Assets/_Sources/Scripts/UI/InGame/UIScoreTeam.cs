using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScoreTeam : MonoBehaviour
{
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

     public void UpdateUI()
     {
          _fillImage.fillAmount = (float) Score / maxScore;
          _scoreText.text = Score.ToString();
     }
}
