using System;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class TargetDev : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private HealthSystem _healthSystem;

    private void Start()
    {
        _healthSystem = GetComponent<HealthSystem>();
        
        _healthSystem.OnHealthChanged.AddListener((() => Debug.Log("Health changed")));
        _healthSystem.OnHealthZero.AddListener((() => gameObject.SetActive(false)));
    }
}