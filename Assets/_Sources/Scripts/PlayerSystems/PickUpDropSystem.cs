using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class PickUpDropSystem : NetworkBehaviour
{
    [Header("PickUp Settings")]
    [SerializeField] private float _pickUpDistance = 1.5f;
    [SerializeField] private PickupMode _pickupMode = PickupMode.Auto;
    [SerializeField] private ColliderTriggerHandler _pickupTriggerHandler;
    [SerializeField] private Transform _pickUpPoint;
    [SerializeField] [CanBeNull] private PickableObject _pickableObject;
    [SerializeField] private float _stoneTime;
    
    [Header("Needed Components")]
    [SerializeField] private BasePlayer basePlayer;
    
    private float _timeToStone = 0;

    public bool IsStone
    {
        get => _timeToStone > 0;
        set
        {
            if (value) _timeToStone = _stoneTime;
            else _timeToStone = 0;
        }
    }
    
    public UnityEvent OnPickUp;

    /// <summary>
    /// Return the current PickableObject in hand, null if there is none
    /// </summary>
    public PickableObject PickableObject
    {
        get => _pickableObject;
        set
        {
            if(_pickableObject == value) return;

            if (_pickableObject != null) _pickableObject.IsPicked = false;
            _pickableObject = value;
            
            if (_pickableObject != null) _pickableObject.IsPicked = true;
            OnPickUp.Invoke();
            
            if (PickableObject != null)
                CmdAssignAuthority(PickableObject.gameObject);
        }
    }

    [Command]
    private void CmdAssignAuthority(GameObject ball)
    {
        ball.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        ball.GetComponent<NetworkIdentity>()
            .AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
        //print("changed authority");
    }
    
    
    private void Start()
    {
        basePlayer = GetComponent<BasePlayer>();

        _pickupTriggerHandler.OnTriggerStayEvent.AddListener(OnColliderStay);
        CapsuleCollider sphereCollider = (CapsuleCollider) _pickupTriggerHandler.collider;
        sphereCollider.radius = _pickUpDistance;
        OnPickUp.AddListener(ChangeBallLayer);
    }

    private void ChangeBallLayer()
    {
        if (_pickableObject == null) return;
        _pickableObject.gameObject.layer = _pickableObject.IsPicked ? 
            LayerMask.NameToLayer("Always On Top") : 
            LayerMask.NameToLayer("Default");
    }
    
    


    private void Update()
    {
        if (PickableObject != null)
        {
            //PickableObject.GetComponent<Rigidbody>().MovePosition(_pickUpPoint.position);
            PickableObject.transform.position = _pickUpPoint.position;
            //PickableObject.GetComponent<NetworkIdentity>().AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient); //On a l'authorité sur l'object qu'on à en main
        }

        if (_timeToStone > 0) _timeToStone -= Time.deltaTime;
    }

    /// <summary>
    /// Function call by InputSystem to toggle the pick up of drop
    /// </summary>
    public void TogglePickupDrop()
    {
        if (PickableObject != null) Drop();
        else TryToPickUp();
    }

    /// <summary>
    /// Function to drop the current PickableObject
    /// </summary>
    private void Drop()
    {
        print("drop");
        if (PickableObject != null) // If the player has an object in hand
        {
            print("RemoveAuthority");
            PickableObject.Drop();
            //RemoveAuthority(PickableObject.gameObject);
            //PickableObject.GetComponent<NetworkIdentity>().RemoveClientAuthority(); //On perd l'authorité sur l'ogject qu'on a drop
            PickableObject = null;
        }
    }
    

    /// <summary>
    /// Function called when collider enter the trigger
    /// </summary>
    /// <param name="other">The collider who enter in range</param>
    private void OnColliderStay(Collider other)
    {
        if (PickableObject != null) return; // If the player has an object in hand
        //print("AAAAAAAAAA");
        // If the object is not pickable
        PickableObject pickableObject = other.GetComponent<PickableObject>();
        if ((pickableObject == null) || pickableObject == PickableObject || !pickableObject.IsPickable) return;
        Debug.Log("BBBBBBBBBBBB", other.gameObject);
        ThrowableObject throwableObject = other.GetComponent<ThrowableObject>();
        if (throwableObject != null && throwableObject.ThrowState != ThrowState.Idle) return; //TODO - change condition 
        Debug.Log("CCCCCCCCCCCCCC - "+throwableObject.ThrowState, other.gameObject);
        if (_pickupMode == PickupMode.Auto)
        {
            PickableObject = pickableObject;
        }
        else if (_pickupMode == PickupMode.SemiAuto)
        {
            if (Utils.IsVisibleByCamera(pickableObject.gameObject, basePlayer.Camera))
            {
                PickableObject = pickableObject;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsStone) return;
        if (PickableObject != null) return; // If the player has an object in hand
        
        // If the object is not pickable
        PickableObject pickableObject = other.GetComponent<PickableObject>();
        if ((pickableObject == null) || pickableObject == PickableObject || !pickableObject.IsPickable) return;
        
        ThrowableObject throwableObject = other.GetComponent<ThrowableObject>();
        if (throwableObject && throwableObject.ThrowState == ThrowState.Rebound && throwableObject.Owner == basePlayer.gameObject)
        {
            PickableObject = pickableObject;
        }
    }

    /// <summary>
    /// Function to try to pick up an object
    /// </summary>
    private void TryToPickUp()
    {
        if (IsStone) return;
        
        RaycastHit[] hits = Physics.SphereCastAll(basePlayer.Camera.transform.position, _pickUpDistance, 
            basePlayer.Camera.transform.forward, _pickUpDistance); // Get all objects in range
        if (hits.Length > 0) // If the list of object is not empty
        {
            PickableObject[] pickableObjects =
                hits.Where(hit => hit.collider.GetComponent<PickableObject>() != null)
                    .Select(hit => hit.collider.GetComponent<PickableObject>())
                    .ToArray(); //Take only pickable objects

            pickableObjects = pickableObjects.Where(pickableObject =>
                Utils.IsVisibleByCamera(pickableObject.transform.position, basePlayer.Camera) &&
                pickableObject.IsPickable).ToArray(); // Take only visible objects

            pickableObjects = pickableObjects.OrderBy(pickableObject =>
                    Vector3.Distance(pickableObject.transform.position, basePlayer.Camera.transform.position))
                .ToArray(); // Take the closest object

            if (pickableObjects.Length > 0) // If there is at least one object in range
            {
                PickableObject closestPickableObject = pickableObjects[0]; // Take the closest object
                
                
                // If the player is not in Manual mode or if the closest object is thrown (To catch the thrown object)
                ThrowableObject throwableObject = closestPickableObject.GetComponent<ThrowableObject>();
                if (_pickupMode != PickupMode.Manual || (throwableObject && throwableObject.ThrowState != ThrowState.Idle)) return;
                
                PickableObject = closestPickableObject;
                PickableObject.PickUp();
            }
        }
    }
}

public enum PickupMode
{
    Auto,
    SemiAuto,
    Manual
}