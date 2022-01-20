using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PickUpDropSystem : MonoBehaviour
{
    [Header("PickUp Settings")]
    [SerializeField] private float _pickUpDistance = 1.5f;
    [SerializeField] private PickupMode _pickupMode = PickupMode.Auto;
    [SerializeField] private ColliderTriggerHandler _pickupTriggerHandler;
    [SerializeField] private Transform _pickUpPoint;
    [SerializeField] [CanBeNull] private PickableObject _pickableObject;
    
    [Header("Needed Components")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Player _player;

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
        }
    }
    
    private void Start()
    {
        _player = GetComponent<Player>();
        _camera = _player.Camera;
        
        _pickupTriggerHandler.OnTriggerStayEvent.AddListener(OnColliderStay);
        CapsuleCollider sphereCollider = (CapsuleCollider) _pickupTriggerHandler.collider;
        sphereCollider.radius = _pickUpDistance;
    }

    private void Update()
    {
        if (PickableObject != null)
        {
            PickableObject.GetComponent<Rigidbody>().MovePosition(_pickUpPoint.position);
        }
    }

    /// <summary>
    /// Function calle by InputSystem to toggle the pick up of drop
    /// </summary>
    /// <param name="ctx">The context of input</param>
    public void TogglePickupDrop(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (PickableObject != null) Drop();
            else TryToPickUp();
        }
    }

    /// <summary>
    /// Function to drop the current PickableObject
    /// </summary>
    private void Drop()
    {
        if (PickableObject != null) // If the player has an object in hand
        {
            PickableObject.Drop();
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
        
        // If the object is not pickable
        PickableObject pickableObject = other.GetComponent<PickableObject>();
        if ((pickableObject == null) || pickableObject == PickableObject || !pickableObject.IsPickable) return;
        
        ThrowableObject throwableObject = other.GetComponent<ThrowableObject>();
        if (throwableObject != null && throwableObject.IsThrown) return; //TODO - change condition 

        if (_pickupMode == PickupMode.Auto)
        {
            PickableObject = pickableObject;
        }
        else if (_pickupMode == PickupMode.SemiAuto)
        {
            if (Utils.IsVisibleByCamera(pickableObject.gameObject, _player.Camera))
            {
                PickableObject = pickableObject;
            }
        }
    }

    /// <summary>
    /// Function to try to pick up an object
    /// </summary>
    private void TryToPickUp()
    {
        RaycastHit[] hits = Physics.SphereCastAll(_player.Camera.transform.position, _pickUpDistance, 
            _player.Camera.transform.forward, _pickUpDistance); // Get all objects in range

        if (hits.Length > 0) // If the list of object is not empty
        {
            if (_pickupMode != PickupMode.Manual) return;

            PickableObject[] pickableObjects =
                hits.Where(hit => hit.collider.GetComponent<PickableObject>() != null)
                    .Select(hit => hit.collider.GetComponent<PickableObject>())
                    .ToArray(); //Take only pickable objects

            pickableObjects = pickableObjects.Where(pickableObject =>
                Utils.IsVisibleByCamera(pickableObject.transform.position, _player.Camera) &&
                pickableObject.IsPickable).ToArray(); // Take only visible objects

            pickableObjects = pickableObjects.OrderBy(pickableObject =>
                    Vector3.Distance(pickableObject.transform.position, _player.Camera.transform.position))
                .ToArray(); // Take the closest object

            if (pickableObjects.Length > 0) // If there is at least one object in range
            {
                PickableObject closestPickableObject = pickableObjects[0]; // Take the closest object

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