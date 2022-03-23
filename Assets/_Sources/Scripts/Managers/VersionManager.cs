using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class VersionManager : SingletonBehaviour<VersionManager>
{
    [SerializeField] [CanBeNull] private TextMeshProUGUI versionText;
    [SerializeField] public VersionData versionData;
    
    private void Start()
    {
        if (versionText != null)
        {
            versionText.text = versionData.ToString();
        }
    }
}