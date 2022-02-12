using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class OnlineGameManager : NetworkBehaviour
{
    private static List<GameObject> allTargets = new List<GameObject>();
    public static event Action<List<GameObject>> OnTargetUpdate;

    /// <summary>
    /// Cette fonction ajoute un objet à la liste des targets
    /// </summary>
    /// <param name="target"></param>
    public static void AddTarget(GameObject target)
    {
        allTargets.Add(target);
        OnTargetUpdate?.Invoke(allTargets);
    }

    /// <summary>
    /// Cette fonction enlève un objet à la liste des targets
    /// </summary>
    /// <param name="target"></param>
    public static void RemoveTarget(GameObject target)
    {
        allTargets.Remove(target);
        OnTargetUpdate?.Invoke(allTargets);
    }
}