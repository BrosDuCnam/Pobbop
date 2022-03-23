using UnityEngine;
using UnityEngine.UI;

public class UIButton : UIObject
{
    private OutlinedObject outline;
    private Button button;

    private void Start()
    {
        outline = GetComponent<OutlinedObject>();
        if (outline == null) outline = gameObject.AddComponent<OutlinedObject>();
        Color color = ThemeManager.theme.color;
        color.a = 0;
        outline.background.color = color;
        
        //color
        //outline.outline.sprite = ThemeManager.instance.fadeImage;
        
        button = GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        

    }

    protected override void OnSkinUI()
    {
        
    }
}