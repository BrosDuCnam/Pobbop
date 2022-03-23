using UnityEngine;

[CreateAssetMenu(fileName = "Version", menuName = "SO/Version", order = 1)]
public class VersionData : ScriptableObject
{
    public int major;
    public int minor;
    public int patch;
    
    public string ToString()
    {
        return $"{major}.{minor}.{patch}";
    }
}