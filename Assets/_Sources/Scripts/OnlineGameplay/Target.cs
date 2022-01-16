using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Target : MonoBehaviour
{
    private void Awake()
    {
        OnlineGameManager.AddTarget(transform.gameObject);
    }
}
