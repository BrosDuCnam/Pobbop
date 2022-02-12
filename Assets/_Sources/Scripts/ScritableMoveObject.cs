using System;
using UnityEngine;

public class ScritableMoveObject : MonoBehaviour
{
    private enum loopingWay
    {
        Loop,
        Restart,
        OneTime
    }

    private Vector3? _destination;
    private int _additive = 1;


    [SerializeField] private loopingWay _way;
    [SerializeField] private Vector3[] _path;
    [SerializeField] private float _speed = 1;

    private void Start()
    {
        transform.position = _path[0];
        _destination = _path[1];
    }

    private void Update()
    {
        if (_destination == null) return;

        //transform.position = Vector3.Lerp(transform.position, (Vector3) _destination, _curve.Evaluate(interpolation));
        transform.position += (-(transform.position - (Vector3)_destination).normalized * (Time.deltaTime * _speed));
        
        if (Vector3.Distance(transform.position, (Vector3) _destination) < 0.1f)
        {
            int index = Array.IndexOf(_path, _destination);
            index += _additive;
            if (index < _path.Length && index >= 0)
            {
                _destination = _path[index];
            }
            else
            {
                if (_way == loopingWay.Loop)
                {
                    _destination = _path[0];
                }

                if (_way == loopingWay.Restart)
                {
                    transform.position = _path[0];
                }

                if (_way == loopingWay.OneTime)
                {
                    _destination = null;
                }
            }
        }
    }
}