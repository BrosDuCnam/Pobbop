using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class OnlineGameManager : MonoBehaviour
{
    [SerializeField] private static List<GameObject> allTargets = new List<GameObject>();

    public static void AddTarget(GameObject target)
    {
        allTargets.Add(target);
        PlayerTargetUpdate.UpdateTargets(allTargets);
    }

    public static void RemoveTarget(GameObject target)
    {
        allTargets.Remove(target);
        PlayerTargetUpdate.UpdateTargets(allTargets);
    }
}
