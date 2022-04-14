using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BallRefab : NetworkBehaviour
{
    public GameObject owner;
    public Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public enum BallStateRefab
    {
        Free,
        Picked,
        Curve
    }

    public BallStateRefab _ballState = BallStateRefab.Free;

    public void ChangeBallState(BallStateRefab ballState, GameObject _owner = null)
    {
        _ballState = ballState;
        owner = _owner;
    }

    
    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 40;
        GUILayout.Label("Ball State: " + _ballState, style);
    }
}
