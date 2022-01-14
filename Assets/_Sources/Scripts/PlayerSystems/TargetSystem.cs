﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class TargetSystem : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private float _targetRange = 100f;
    [SerializeField] private bool DEBUG;

    [Header("Needed Components")]
    [SerializeField] private Player _player;
    [SerializeField] private List<GameObject> _targets;

    public List<GameObject> Targets
    {
        get { return _targets; }
        set { _targets = value; }
    }
    [NotNull] public GameObject CurrentTarget { get; private set; }
    
    private void Start()
    {
        _player = GetComponent<Player>();
    }

    private void Update()
    {
        List<GameObject> visibleTargets = GetVisibleTargets(_targets);
        visibleTargets = OrderByDistanceToCenterOfScreen(visibleTargets);
        if (visibleTargets.Count > 0)
        {
            CurrentTarget = visibleTargets[0];
        }
        else
        {
            CurrentTarget = null;
        }

        if (DEBUG)
        {
            foreach (GameObject target in _targets)
            {
                var color = Color.red;
                if (visibleTargets.Contains(target))
                {
                    if (target == visibleTargets[0])
                    {
                        color = Color.green;
                    }
                    else
                    {
                        color = Color.yellow;
                    }
                }
                Debug.DrawLine(transform.position, target.transform.position, color);
            }
        }
    }

    private List<GameObject> OrderByDistanceToCenterOfScreen(List<GameObject> targets)
    {
        return targets.AsEnumerable().OrderBy(target => Utils.GetDistanceFromCenterOfScreen(target, _player.Camera)).ToList();
    }
    
    private List<GameObject> GetVisibleTargets(List<GameObject> targets)
    {
        List<GameObject> visibleTargets = new List<GameObject>();
        foreach (GameObject target in targets)
        {
            // Check if target is in field of view of player camera
            if (Utils.IsVisibleByCamera(target, _player.Camera))
            {
                // If object obstructs the view of the player, it is not visible
                RaycastHit[] hits = Physics.RaycastAll(transform.position, target.transform.position - transform.position,
                    _targetRange);

                hits.Select(x => x.transform.gameObject != _player.HoldingObject.gameObject);
                
                if (hits.Length > 0 && hits[0].collider.gameObject == target)
                {
                    visibleTargets.Add(target);
                }
            }
        }

        return visibleTargets;
    }
}