using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Thrower : MonoBehaviour
{
    [SerializeField] private GameObject _throwableObject;
    
    [SerializeField] private Transform _throwPosition;

    [SerializeField] private float _minimalRandomTimeThrow;
    [SerializeField] private float _maximalRandomTimeThrow;
    private float _timer;
    

    private void Awake()
    {
        _timer = Random.Range(_minimalRandomTimeThrow, _maximalRandomTimeThrow);
    }

    private void Update()
    {
        print(_timer);
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        if (_timer <= 0)
        {
            _timer = Random.Range(_minimalRandomTimeThrow, _maximalRandomTimeThrow);
            
            ThrowableObject throwableObject = Instantiate(_throwableObject, _throwPosition)
                .GetComponent<ThrowableObject>();
            
            throwableObject.Throw(gameObject, transform.forward, Random.Range(10, 50));
        }
    }
}
