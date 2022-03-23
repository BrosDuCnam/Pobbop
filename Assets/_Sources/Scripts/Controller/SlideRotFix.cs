using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SlideRotFix : MonoBehaviour
{
    [SerializeField] private float yRotOffset = 90;
    private GameObject playerModel;
    private Rigidbody rb;
    private bool sliding;
    private Animator animator;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    private void Update()
    {
        if (sliding)
        {
            transform.LookAt(transform.position + new Vector3(rb.velocity.x, 0, rb.velocity.z));
            transform.Rotate(Vector3.up, yRotOffset);
        }
        else
        {
            transform.localRotation = quaternion.Euler(0, yRotOffset * Mathf.Deg2Rad, 0);
        }
    }

    public void SlideStart() { sliding = true; }
    public void SlideStop() { sliding = false; }
}
