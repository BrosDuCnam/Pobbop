using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetAllBindings : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputAction;

    public void ResetBindings()
    {
        foreach (InputActionMap map in _inputAction.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
    }
}
