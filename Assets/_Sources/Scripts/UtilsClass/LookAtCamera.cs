using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform camera;
    [SerializeField] private float size = 2;
    [SerializeField] private float distanceScale = 0.2f;
    [SerializeField] private float maxScale = 20f;
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float verticalOffset = 0.1f;
    private RectTransform rectTransform;
    private float originalY;
    private float originalX;
    private float originalZ;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalY = transform.localPosition.y;

    }

    private void Update()
    {
        if (camera == null) return;
        
        transform.LookAt(camera.position);
        float distance = Vector3.Distance(transform.position, camera.position);
        rectTransform.localScale = Vector3.one * size * 0.001f * Mathf.Clamp(distance * distanceScale, minScale, maxScale);
        transform.localPosition = new Vector3(transform.localPosition.x, originalY + distance * verticalOffset, transform.localPosition.z);
    }
}
