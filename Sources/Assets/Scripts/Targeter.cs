using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class Targeter : MonoBehaviour
{
    [SerializeField] private float maxDistance;
    [SerializeField] private List<GameObject> targets;
    [SerializeField] private Camera camera;
    [SerializeField] [CanBeNull] private GameObject _currentTarget;
    [CanBeNull] public GameObject CurrentTarget { get => _currentTarget; }

    private void Update()
    {
        Target();

    }

    /// <summary>
    /// Function to find the closest target from center of camera
    /// </summary>
    private void Target()
    {
        GameObject? nextTarget = null;
        
        List<GameObject> visibleTargets = GetVisibleTargets();

        // Sort all visibles target by distance from center of camera
        visibleTargets = visibleTargets.OrderBy(x =>
        {
            Vector3 screenPoint = camera.WorldToViewportPoint(x.transform.position);
            float distance = Vector2.Distance(new Vector2(screenPoint.x, screenPoint.y), new Vector2(0.5f, 0.5f));
            return distance;
        }).ToList();

        foreach (GameObject target in visibleTargets)   
        {
            //Debug.DrawRay(camera.transform.position, target.transform.position - camera.transform.position, Color.green);
            if (Physics.Raycast(camera.transform.position, target.transform.position - camera.transform.position,
                out RaycastHit hit, maxDistance))
            {
                if (target == hit.transform.gameObject)
                {
                    nextTarget = target;
                    break;
                }
            }
        }
        if (nextTarget != _currentTarget)
        {
            nextTarget = _currentTarget;
        }
    }
    
    /// <summary>
    /// Function to get all visibles targets from player Camera
    /// </summary>
    /// <returns>All visibles target from player</returns>
    private List<GameObject> GetVisibleTargets()
    {
        List<GameObject> result = targets.Where(x =>
        {
            Vector3 screenPos = camera.WorldToViewportPoint(x.transform.position);
            return screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1;
        }).ToList();
        
        return result;
    }
}