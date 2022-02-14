﻿using System;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using UnityEngine;
using UnityEngine.Events;


//TODO - peut etre moyen d'opti cette class
public class PickableObject : MonoBehaviour
{
    [SerializeField] [CanBeNull] private Collider _collider;
    [SerializeField] [CanBeNull] private Rigidbody _rigidbody;
    [SerializeField] private bool _isPicked ;
    
    public UnityEvent OnStateChanged;
    
    /// <summary>
    /// Returns true if object is picked
    /// </summary>
    public bool IsPicked
    {
        get => _isPicked;
        set
        {
            if (_isPicked == value) return;
            if (value) PickUp();
            else Drop();
            _isPicked = value;
            //print(value);
            OnStateChanged.Invoke();
        }
    }

    /// <summary>
    /// Returns true if object can be picked
    /// </summary>
    public bool IsPickable
    {
        get => !_isPicked;
    }

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Function to pick up object
    /// </summary>
    public void PickUp()
    {
        if (!IsPickable)
            return;
        
        _isPicked = true;
        if (_collider != null) _collider.enabled = false;
        if (_rigidbody != null) _rigidbody.isKinematic = true;
    }
    
    
    /// <summary>
    /// Function to drop object
    /// </summary>
    public void Drop()
    {
        _isPicked = false;
        if (_collider != null) _collider.enabled = true;
        if (_rigidbody != null) _rigidbody.isKinematic = false;
    }
    
}