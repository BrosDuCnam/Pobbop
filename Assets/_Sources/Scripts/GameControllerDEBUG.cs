using System.Collections.Generic;
using UnityEngine;

public class GameControllerDEBUG : MonoBehaviour
{
    [SerializeField] private List<GameObject> _targets;

    public List<GameObject> Targets { get => _targets; }
}