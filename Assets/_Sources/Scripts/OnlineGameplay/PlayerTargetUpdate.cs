using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetUpdate : MonoBehaviour
{
    private static List<GameObject> targets = new List<GameObject>();
    private static bool updateTarget = false;
    private static GameObject player;
    private static int test = 1;


    private void Awake()
    {
        player = transform.gameObject;
    }
    
    /// <summary>
    /// Cette fonction update la liste de target du joueur
    /// </summary>
    /// <param name="newTargetList"></param>
    public static void UpdateTargets(List<GameObject> newTargetList)
    {
        targets = newTargetList;
        targets.Remove(player);
        updateTarget = true;
    }

    private void Update()
    {
        if (updateTarget)
        {
            GetComponent<TargetSystem>().Targets = targets;
            updateTarget = false;
        }
    }
}
