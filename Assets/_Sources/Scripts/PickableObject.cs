using System;
using JetBrains.Annotations;
using UnityEngine;

public class PickableObject : MonoBehaviour
{
    [SerializeField] [CanBeNull] private Collider _collider;
    [SerializeField] [CanBeNull] private Rigidbody _rigidbody;
    [SerializeField] private bool _isPickable = true;
    [SerializeField] private bool _isPicked = false;
    
    public bool IsPicked { get { return _isPicked; } }
    public bool IsPickable { get { return _isPickable; } }

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void PickUp()
    {
        if (!_isPickable)
            return;

        _isPickable = false;
        _isPicked = true;
        if (_collider != null) _collider.enabled = false;
        if (_rigidbody != null) _rigidbody.isKinematic = true;
    }
    
    public void Drop()
    {
        _isPickable = true;
        _isPicked = false;
        if (_collider != null) _collider.enabled = true;
        if (_rigidbody != null) _rigidbody.isKinematic = false;
    }
    
}