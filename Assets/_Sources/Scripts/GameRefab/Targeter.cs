using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class Targeter : MonoBehaviour
{
    private Player _player;
    [SerializeField] private List<GameObject> _targets = new List<GameObject>();
    [SerializeField] private List<Player> _targetPlayers = new List<Player>();
    [SerializeField] private List<Player> _friendlyPlayers = new List<Player>();
    [NotNull] public GameObject CurrentTarget { get; private set; }
    [SerializeField] private float _targetRange = 100f;
    [SerializeField] private bool DEBUG;

    void Start()
    {
        _player = GetComponent<Player>();
        UpdateTargets();
        GameManager.instance.onPlayerJoinedCallback += UpdateTargets;
        GameManager.instance.onPlayerLeftCallback += UpdateTargets;
    }

    /// <summary>
    /// Get all players except the current player (self)
    /// </summary>
    private void UpdateTargets(string name = "")
    {
        foreach (Player player in GameManager.GetAllPlayers())
        {
            if (player.teamId == _player.teamId && player != _player) _friendlyPlayers.Add(player);
            else _targetPlayers.Add(player);
        }
        _targets = _targetPlayers.Select(x => x.gameObject).ToList();
    }
    
    private void Update()
    {
        if (_player.IsCharging == false && _targets.Count > 0) // If player is not charger we can search for targets
        {
            List<GameObject> visibleTargets = GetVisibleTargets(_targets, _targetPlayers); // Get all visible targets
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
                for (int i = 0; i < _targets.Count; i++)
                {
                    var color = Color.red;
                    if (visibleTargets.Contains(_targets[i]))
                    {
                        if (_targets[i] == visibleTargets[0])
                        {
                            color = Color.green;
                        }
                        else
                        {
                            color = Color.yellow;
                        }
                    }
                    Debug.DrawLine(_player.targetPoint.position, _targetPlayers[i].targetPoint.position, color);
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
        return targets.AsEnumerable().OrderBy(target => Utils.GetDistanceFromCenterOfScreen(target, _player.playerCam)).ToList();
    }
    
    /// <summary>
    /// Function to get all visible targets
    /// </summary>
    /// <param name="targets">List of targets</param>
    /// <returns>List of visible target</returns>
    public List<GameObject> GetVisibleTargets(List<GameObject> targets, List<Player> players)
    {
        List<GameObject> visibleTargets = new List<GameObject>();
        //foreach (GameObject target in targets)
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null) continue;
            // Check if target is in field of view of player camera
            if (Utils.IsVisibleByCamera(players[i].targetPoint, _player.playerCam))
            {
                // If object obstructs the view of the player, it is not visible
                RaycastHit[] hits = Physics.RaycastAll(_player.targetPoint.position, players[i].targetPoint.position - _player.targetPoint.position,
                    _targetRange);

                hits = hits.Where(x => !x.transform.gameObject.CompareTag("Ball")).ToArray();
                hits = hits.OrderBy(x => x.distance).ToArray();

                if (hits.Length > 0 && (hits[0].collider.gameObject == targets[i] 
                                        || hits[0].transform.root.gameObject == targets[i] 
                                        || hits[0].transform.root.GetComponentsInChildren<Transform>().Contains(targets[i].transform)))
                {
                    visibleTargets.Add(targets[i]);
                }
            }
        }
        return visibleTargets;
    }

    /// <summary>
    /// Gets the friendly player who is the closest to the center of the screen
    /// </summary>
    /// <returns></returns>
    public GameObject GetDesiredFriend()
    {
        if (_friendlyPlayers.Count == 0) return null;
        List<GameObject> visibleFriendlies = GetVisibleTargets(_friendlyPlayers.Select(x => x.gameObject).ToList(), _friendlyPlayers);
        if (visibleFriendlies.Count == 0) return null;
        return OrderByDistanceToCenterOfScreen(visibleFriendlies)[0];
    }
}
