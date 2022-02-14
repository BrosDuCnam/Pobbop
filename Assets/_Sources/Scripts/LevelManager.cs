using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public List<GameObject> targets;

    public void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        targets = GameObject.FindGameObjectsWithTag("Target").ToList();
    }
}
