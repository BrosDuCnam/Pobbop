using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ce scripts est mis sur un objet qui peut être un target
/// </summary>
public class Target : MonoBehaviour
{
    private void Start()
    {
        OnlineGameManager.AddTarget(transform.gameObject);
    }
}
