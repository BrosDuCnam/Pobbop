using System;
using UnityEngine;
using UnityEngine.UI;

public class OutlinedObject : MonoBehaviour
{
    private GameObject parent;
    public Image background;
    private GameObject outlineContainer;
    public Image outline;
    public float outlineSize;
    
    private void Awake()
    {
        parent = InstantiateFullGameObject(gameObject);

        GameObject backgroundObj = InstantiateFullGameObject(parent.gameObject);
        backgroundObj.AddComponent<Image>();
        background = backgroundObj.GetComponent<Image>();
        background.color = new Color(0, 0, 0, 0);
        
        outlineContainer = InstantiateFullGameObject(parent.gameObject);
        outlineContainer.AddComponent<Image>();
        outlineContainer.AddComponent<Mask>();
        outlineContainer.GetComponent<Mask>().showMaskGraphic = false;

        GameObject outlineObj = InstantiateFullGameObject(outlineContainer);
        outlineObj.AddComponent<CutoutMaskUI>();
        outline = outlineObj.GetComponent<Image>();
        
        UpdateUI();
    }

    private GameObject InstantiateFullGameObject(GameObject parent)
    {
        GameObject obj = Instantiate(new GameObject(), parent.transform);
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null)
        {
            obj.AddComponent<RectTransform>();
            rect = obj.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("No RectTransform,OutlinedObject only work in canvas environment");
                return null;
            }
        }
        rect.anchorMax = Vector2.one;
        rect.anchorMin = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        return obj;
    }

    public void UpdateUI()
    {
        outlineContainer.GetComponent<RectTransform>().offsetMax = new Vector2(-outlineSize, -outlineSize);
        outlineContainer.GetComponent<RectTransform>().offsetMin = new Vector2(outlineSize, outlineSize);

        outline.GetComponent<RectTransform>().offsetMax = new Vector2(outlineSize, outlineSize);
        outline.GetComponent<RectTransform>().offsetMin = new Vector2(-outlineSize, -outlineSize);
    }
}