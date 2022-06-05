using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScore : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    
    List<UIScoreTeam> teams = new List<UIScoreTeam>();
    
    RoomProperties roomProperties;
    GameManager gameManager;

    private void Start()
    {
        roomProperties = RoomProperties.instance;
        gameManager = GameManager.instance;
        
        gameManager.
    }

    private void Update()
    {
        
    }
    
}