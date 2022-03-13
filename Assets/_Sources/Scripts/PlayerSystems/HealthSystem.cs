using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _maxHealth = 3;
    private int _currentHealth = 3;
    
    [Header("Events")]
    public UnityEvent OnHealthChanged; 
    public UnityEvent OnHealthZero;

    [CanBeNull] public GameObject LastPlayerDamage;
    

    public int Health
    {
        get => _currentHealth;
        private set
        {
            _currentHealth = value;
            OnHealthChanged.Invoke();
            if (_currentHealth <= 0)
            {
                OnHealthZero.Invoke();
            }
        }
    }
    public int MaxHealth
    {
        get => _maxHealth;
    }

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int damage, GameObject playerDamage)
    {
        Health -= damage;
        LastPlayerDamage = playerDamage;
        print("damage");
    }

    public void SetHealth(int healt)
    {
        _currentHealth = healt;
    }
}