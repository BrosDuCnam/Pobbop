using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ClientActivations : NetworkBehaviour
{
    [SerializeField] private Behaviour[] behavioursToActivate;
    [SerializeField] private GameObject[] gameObjectsToActivate;

    public override void OnStartAuthority()
    {
        if (behavioursToActivate != null)
        {
            foreach (Behaviour behaviour in behavioursToActivate)
            {
                behaviour.enabled = true;
            }
        }

        if (gameObjectsToActivate != null)
        {
            foreach (GameObject gameObject in gameObjectsToActivate)
            {
                gameObject.SetActive(true);
            }
        }
    }
}
