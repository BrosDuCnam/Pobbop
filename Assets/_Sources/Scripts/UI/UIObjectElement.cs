using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UIObjectElement : UIObject {
	[Header("Parameters")]
	Color outline;
	Image image;
	GameObject message;
	
	//public enum OutlineStyle {solidThin, solidThick, dottedThin, dottedThick};
	public bool hasImage = false;
	public bool isText = false;

	protected override void OnSkinUI(){
		base.OnSkinUI();

		if(hasImage){
			image = GetComponent<Image>();
			image.color = ThemeManager.theme.color;
		}

		message = gameObject;

		if(isText){
			message.GetComponent<TextMeshProUGUI>().color = ThemeManager.theme.textColor;
			message.GetComponent<TextMeshProUGUI>().font = ThemeManager.theme.font;
		}
	}
}