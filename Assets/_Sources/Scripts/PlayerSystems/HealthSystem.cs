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

    [CanBeNull] private GameObject lastPlayerDamage;
    

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
        OnHealthZero.AddListener(Eliminated); // On définit la fonction éliminer sur l'event OnHealthZero
    }

    public void TakeDamage(int damage, GameObject playerDamage)
    {
        Health -= damage;
        lastPlayerDamage = playerDamage;
    }

    private void Eliminated()
    {
        int teamNumber = transform.GetComponent<BasePlayer>().teamNumber;
        int enemyTeam = lastPlayerDamage.GetComponent<BasePlayer>().teamNumber;
        NetworkManagerLobby.AddPoint(enemyTeam);
        PlayerSpawnSystem.PlayerRemoveTransform(transform, teamNumber);
        PlayerSpawnSystem.Respawn(transform, teamNumber);
    }
}