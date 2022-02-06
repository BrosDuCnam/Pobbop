using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class OnlineGameManager : MonoBehaviour
{
    [SerializeField] private static List<GameObject> allTargets = new List<GameObject>();

    /// <summary>
    /// Cette fonction ajoute un objet à la liste des targets
    /// </summary>
    /// <param name="target"></param>
    public static void AddTarget(GameObject target)
    {
        allTargets.Add(target);
        PlayerTargetUpdate.UpdateTargets(allTargets);
    }

    /// <summary>
    /// Cette fonction enlève un objet à la liste des targets
    /// </summary>
    /// <param name="target"></param>
    public static void RemoveTarget(GameObject target)
    {
        allTargets.Remove(target);
        PlayerTargetUpdate.UpdateTargets(allTargets);
    }
}
