using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayTestTarget : MonoBehaviour
{
    [SerializeField] private float _timeToRespawn;
    
    private HealthSystem _healthSystem;
    private Target _target;

    private float _timeWhenDeath;
    private float _timer;
    
    private void Awake()
    {
        _healthSystem = GetComponentInChildren<HealthSystem>();
        _target = GetComponentInChildren<Target>();
        
        _healthSystem.OnHealthZero.AddListener(() =>
        {
            _target.gameObject.SetActive(false);
            _timeToRespawn = Time.time;
            _timer = _timeToRespawn;
        });
    }

    private void Update()
    {
        if (_timer >= 0)
        {
            _timer -= Time.deltaTime;
            if (_timer < 0)
            {
                _healthSystem.gameObject.SetActive(true);
                _healthSystem.SetHealth(_healthSystem.MaxHealth);
            }
        }
    }
}
