using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class Thrower : NetworkBehaviour
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
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        if (_timer <= 0)
        {
            print("throw");
            _timer = Random.Range(_minimalRandomTimeThrow, _maximalRandomTimeThrow);
            
            ThrowableObject throwableObject = Instantiate(_throwableObject, _throwPosition.position, _throwPosition.rotation)
                .GetComponent<ThrowableObject>();

            
            SpawnBall(throwableObject.gameObject);
            
            throwableObject.Throw(gameObject, transform.forward, Random.Range(1, 5));
        }
    }
    [Command]
    private void SpawnBall(GameObject ball)
    {
        NetworkServer.Spawn(ball);
    }
}
