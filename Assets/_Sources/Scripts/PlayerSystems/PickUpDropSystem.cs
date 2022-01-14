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
    [SerializeField] bool _autoPickUpGround = true;
    [SerializeField] private Transform _pickUpPoint;
    [SerializeField] [CanBeNull] private PickableObject _pickableObject;

    [Header("Needed Components")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Player _player;

    public UnityEvent OnPickUp;

    public PickableObject PickableObject
    {
        get => _pickableObject;
        set
        {
            if(_pickableObject == value) return;
            if (_pickableObject != null) _pickableObject.IsPicked = false;
            _pickableObject = value;
            OnPickUp.Invoke();
        }
    }
    
    private void Start()
    {
        _player = GetComponent<Player>();
        _camera = _player.Camera;
    }

    private void Update()
    {
        if (PickableObject != null)
        {
            PickableObject.GetComponent<Rigidbody>().MovePosition(_pickUpPoint.position);
        }
    }

    public void TogglePickupDrop(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (PickableObject != null) Drop();
            else TryToPickUp();
        }
    }

    private void Drop()
    {
        if (PickableObject != null) // If the player has an object in hand
        {
            print("Drop");
            PickableObject.Drop();
            PickableObject = null;
        }
    }

    private void TryToPickUp()
    {
        RaycastHit[] hits = Physics.SphereCastAll(_player.Camera.transform.position, _pickUpDistance, 
            _player.Camera.transform.forward, _pickUpDistance); // Get all objects in range

        if (hits.Length > 0) // If the list of object is not empty
        {

            PickableObject[] pickableObjects =
                hits.Where(hit => hit.collider.GetComponent<PickableObject>() != null)
                    .Select(hit => hit.collider.GetComponent<PickableObject>())
                    .ToArray(); //Take only pickable objects

            pickableObjects = pickableObjects.Where(pickableObject =>
                Utils.IsVisibleByCamera(pickableObject.transform.position, _player.Camera) &&
                pickableObject.IsPickable).ToArray(); // Take only visible objects

            try
            {
                PickableObject closestPickableObject = pickableObjects.OrderBy(pickableObject =>
                        Vector3.Distance(pickableObject.transform.position, _player.Camera.transform.position))
                    .ToArray()[0]; // Take the closest object

                PickableObject = closestPickableObject;
                PickableObject.PickUp();
            }
            catch (Exception e)
            {
            }
        }
    }
}