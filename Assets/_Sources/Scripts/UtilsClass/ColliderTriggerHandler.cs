using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ColliderTriggerHandler : MonoBehaviour
{
    public Collider collider;
    
    public ColliderEvent OnTriggerEnterEvent;
    public ColliderEvent OnTriggerExitEvent;
    public ColliderEvent OnTriggerStayEvent;
    
    public CollisionEvent OnCollisionEnterEvent;
    public CollisionEvent OnCollisionExitEvent;
    public CollisionEvent OnCollisionStayEvent;

    private void OnEnable()
    {
        collider = GetComponent<Collider>();

        OnTriggerEnterEvent = new ColliderEvent();
        OnTriggerExitEvent = new ColliderEvent();
        OnTriggerStayEvent = new ColliderEvent();
        
        OnCollisionEnterEvent = new CollisionEvent();
        OnCollisionExitEvent = new CollisionEvent();
        OnCollisionStayEvent = new CollisionEvent();
    }

    public void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterEvent.Invoke(other);
    }
    
    public void OnTriggerExit(Collider other)
    {
        OnTriggerExitEvent.Invoke(other);
    }
    
    public void OnTriggerStay(Collider other)
    {
        OnTriggerStayEvent.Invoke(other);
    }
    
    public void OnCollisionEnter(Collision other)
    {
        OnCollisionEnterEvent.Invoke(other);
    }
    
    public void OnCollisionExit(Collision other)
    {
        OnCollisionExitEvent.Invoke(other);
    }
    
    public void OnCollisionStay(Collision other)
    {
        OnCollisionStayEvent.Invoke(other);
    }
}

[Serializable]
public class ColliderEvent : UnityEvent<Collider>
{
}

[Serializable]
public class CollisionEvent : UnityEvent<Collision>
{
}