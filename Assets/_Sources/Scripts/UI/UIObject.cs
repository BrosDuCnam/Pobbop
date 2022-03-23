using UnityEngine;


/*[ExecuteInEditMode()]*/
[System.Serializable]
public class UIObject : MonoBehaviour
{
	protected virtual void OnSkinUI()
	{

	}

	public virtual void Start()
	{
		OnSkinUI();
	}

	public virtual void Update()
	{
		OnSkinUI();
	}
}