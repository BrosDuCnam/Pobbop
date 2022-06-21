using System;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class StepSound : NetworkBehaviour
{
    [SerializeField] private AudioClip[] _stepSounds;
    [SerializeField] private AudioSource _audioSource;
    
    [SerializeField] private float _stepInterval = 0.3f;
    private float _stepIntervalTimer;

    public bool CanStepSound
    {
        get => _stepIntervalTimer <= 0;
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            _audioSource.spatialBlend = 0;
        }
    }
    
    private void Update()
    {
        if (_stepIntervalTimer > 0)
        {
            _stepIntervalTimer -= Time.deltaTime;
        }
    }

    // On Step animation event
    public void Step()
    {
        if (!CanStepSound) return;
        
            _audioSource.clip = _stepSounds[Random.Range(0, _stepSounds.Length)];
        _audioSource.pitch = Random.Range(0.9f, 1.1f);
        _audioSource.PlayOneShot(_audioSource.clip);
        
        _stepIntervalTimer = _stepInterval;
    }
    
    public void Sprint()
    {
        if (!CanStepSound) return;
        
        _audioSource.clip = _stepSounds[Random.Range(0, _stepSounds.Length)];
        _audioSource.pitch = Random.Range(1.1f, 1.5f);
        _audioSource.PlayOneShot(_audioSource.clip);
        
        _stepIntervalTimer = _stepInterval;
    }
}