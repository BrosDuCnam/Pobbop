using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "UITheme", menuName = "SO/UI/Theme", order = 1)]
[System.Serializable]
public class UITheme : ScriptableObject
{
    public Color color;
    public TMP_FontAsset font;
    public Color textColor;
    public float outlineSize;
}