using UnityEngine;

public class ThemeManager : SingletonBehaviour<ThemeManager>
{
    [SerializeField] private UITheme _uiTheme;
    public static UITheme theme { get { return Instance._uiTheme; } }
    public Sprite gradientPure;
    public Sprite gradientSmall;
    
}