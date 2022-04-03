using System;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using UnityEngine;
using UnityEngine.Events;


//TODO - peut etre moyen d'opti cette class
public class PickableObject : NetworkBehaviour
{
    [SerializeField] [CanBeNull] private Collider _collider;
    [SerializeField] [CanBeNull] private Rigidbody _rigidbody;
    
    [SyncVar]
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
            CmdChangeIsPicked(value);
            if (value) PickUp();
            else Drop();
            _isPicked = value;
            //print(value);
            OnStateChanged.Invoke();
            
            IsPickable = !value;
        }
    }

    private bool _isPickable = true;

    /// <summary>
    /// Returns true if object can be picked
    /// </summary>
    public bool IsPickable
    {
        get => _isPickable;
        set
        {
            CmdChangeIsPickAble(value);
            _isPickable = value;
        }
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
    
    [Command (requiresAuthority = false)]
    private void CmdChangeIsPicked(bool value)
    {
        RpcChangeIsPicked(value);
    }
    [ClientRpc]
    private void RpcChangeIsPicked(bool value)
    {
        IsPicked = value;
    }
    
    [Command (requiresAuthority = false)]
    private void CmdChangeIsPickAble(bool value)
    {
        RpcChangeIsPicked(value);
    }
    [ClientRpc]
    private void RpcChangeIsPickable(bool value)
    {
        IsPickable = value;
    }
    
    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 50;
        style.margin = new RectOffset(0, 500, 0, 0);
        GUILayout.Label("Is Pickable : " + _isPickable, style);
    }
}