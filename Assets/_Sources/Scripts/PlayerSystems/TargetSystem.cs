using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BasePlayer))]
public class TargetSystem : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private float _targetRange = 100f;
    [SerializeField] private bool DEBUG;

    [Header("Needed Components")]
    [SerializeField] private BasePlayer basePlayer;

    //La liste est en static pour quelle puisse être modifié
    [SerializeField] private List<GameObject> _targets;

    public List<GameObject> Targets
    {
        get { return _targets; }
        set { _targets = value; }
    }
    [NotNull] public GameObject CurrentTarget { get; private set; }
    
    private void Start()
    {
        basePlayer = GetComponent<BasePlayer>();
    }

    private void Update()
    {
        if (!basePlayer.IsCharging) // If player is not charger we can search for targets
        {
            List<GameObject> visibleTargets = GetVisibleTargets(_targets); // Get all visible targets
            visibleTargets =
                OrderByDistanceToCenterOfScreen(
                    visibleTargets); // Order visible targets by distance to center of screen
            if (visibleTargets.Count > 0)
            {
                CurrentTarget = visibleTargets[0]; // Set current target to the closest target
            }
            else
            {
                CurrentTarget = null; // Else set current target to null
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
    }

    /// <summary>
    /// Function to order targets by distance to center of screen
    /// </summary>
    /// <param name="targets">List of tagets</param>
    /// <returns>An ordered list of target</returns>
    private List<GameObject> OrderByDistanceToCenterOfScreen(List<GameObject> targets)
    {
        return targets.AsEnumerable().OrderBy(target => Utils.GetDistanceFromCenterOfScreen(target, basePlayer.Camera)).ToList();
    }
    
    /// <summary>
    /// Function to get all visible targets
    /// </summary>
    /// <param name="targets">List of targets</param>
    /// <returns>List of visible target</returns>
    private List<GameObject> GetVisibleTargets(List<GameObject> targets)
    {
        List<GameObject> visibleTargets = new List<GameObject>();
        foreach (GameObject target in targets)
        {
            // Check if target is in field of view of player camera
            if (Utils.IsVisibleByCamera(target, basePlayer.Camera))
            {
                // If object obstructs the view of the player, it is not visible
                RaycastHit[] hits = Physics.RaycastAll(transform.position, target.transform.position - transform.position,
                    _targetRange);

                hits.Select(x => x.transform.gameObject != basePlayer.HoldingObject.gameObject);
                
                if (hits.Length > 0 && hits[0].collider.gameObject == target)
                {
                    visibleTargets.Add(target);
                }
            }
        }
        return visibleTargets;
    }
    
}