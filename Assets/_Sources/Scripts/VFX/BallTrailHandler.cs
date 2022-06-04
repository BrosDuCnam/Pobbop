using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BallTrailHandler : MonoBehaviour
{
    private VisualEffect trailEffect;
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        trailEffect = GetComponentInChildren<VisualEffect>();
    }

    void Update()
    {
        Vector3 vel = rb.velocity;
        float magnitude = vel.magnitude;
        magnitude = Mathf.Clamp(magnitude - 5f,0, Mathf.Infinity);
        vel = vel.normalized * magnitude;
        // trailEffect.SetVector3("Velocity", vel);
    }
}
