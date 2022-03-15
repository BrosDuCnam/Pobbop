using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ClientActivations : NetworkBehaviour
{
    [Header("Behaviour to activate or deactivate if the player is the owner")]
    [SerializeField] private Behaviour[] behavioursToActivate;
    [SerializeField] private GameObject[] gameObjectsToActivate;
    [SerializeField] private GameObject[] gameObjectsToDectivate;

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

        if (gameObjectsToDectivate != null)
        {
            foreach (GameObject gameObject in gameObjectsToDectivate)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
