using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlayerOnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().Die(null, 0);
        }
    }
}
