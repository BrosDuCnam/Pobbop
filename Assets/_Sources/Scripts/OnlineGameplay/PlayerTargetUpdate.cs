using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerTargetUpdate : MonoBehaviour
{
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
        GetComponent<Targeter>().Targets = new List<GameObject>(newTargetList);
        GetComponent<Targeter>().Targets.Remove(transform.gameObject);
    }
}
