using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class PickableObject : MonoBehaviour
{
    [SerializeField] [CanBeNull] private Collider _collider;
    [SerializeField] [CanBeNull] private Rigidbody _rigidbody;
    [SerializeField] private bool _isPicked = false;
    
    public UnityEvent OnStateChanged;
    public bool IsPicked
    {
        get => _isPicked;
        set
        {
            if (_isPicked == value) return;
            if (value) PickUp();
            else Drop();
            _isPicked = value;
            OnStateChanged.Invoke();
        }
    }

    public bool IsPickable
    {
        get => !_isPicked;
    }

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void PickUp()
    {
        if (!IsPickable)
            return;
        
        _isPicked = true;
        if (_collider != null) _collider.enabled = false;
        if (_rigidbody != null) _rigidbody.isKinematic = true;
    }
    
    public void Drop()
    {
        _isPicked = false;
        if (_collider != null) _collider.enabled = true;
        if (_rigidbody != null) _rigidbody.isKinematic = false;
    }
    
}