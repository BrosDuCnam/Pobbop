using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerTargetUpdate : MonoBehaviour
{
    private static List<GameObject> targets = new List<GameObject>();
    private static bool updateTarget = false;
    private static GameObject player;
    private static int test = 1;


    private void Awake()
    {
        OnlineGameManager.OnTargetUpdate += UpdateTargets;
    }
    
    /// <summary>
    /// Cette fonction update la liste de target du joueur
    /// </summary>
    /// <param name="newTargetList"></param>
    public void UpdateTargets(List<GameObject> newTargetList)
    {
        GetComponent<TargetSystem>().Targets = newTargetList;
        GetComponent<TargetSystem>().Targets.Remove(transform.gameObject);
    }
}
