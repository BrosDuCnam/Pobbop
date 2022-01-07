using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickDropUpSystem : MonoBehaviour
{
    [Header("PickUp Settings")]
    [SerializeField] private float _pickUpDistance = 1.5f;
    [SerializeField] bool _autoPickUpGround = true;
    [SerializeField] private Transform _pickUpPoint;
    [SerializeField] [CanBeNull] private PickableObject _pickableObject;

    [Header("Needed Components")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Player _player;

    public PickableObject PickableObject { get => _pickableObject; }
    
    private void Start()
    {
        _player = GetComponent<Player>();
        _camera = _player.Camera;
    }

    private void Update()
    {
        if (_pickableObject != null)
        {
            _pickableObject.GetComponent<Rigidbody>().MovePosition(_pickUpPoint.position);
        }
    }

    public void TogglePickupDrop(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (_pickableObject != null) // If the player has an object in hand
            {
                print("Drop");
                _pickableObject.Drop();
                _pickableObject = null;
            }
            else
            {
                print("Pickup");
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

                    PickableObject closestPickableObject = pickableObjects.OrderBy(pickableObject =>
                            Vector3.Distance(pickableObject.transform.position, _player.Camera.transform.position))
                        .ToArray()[0]; // Take the closest object

                    _pickableObject = closestPickableObject;
                    _pickableObject.PickUp();
                }
            }
        }
    }
    
}