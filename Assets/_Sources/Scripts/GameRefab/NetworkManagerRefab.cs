using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class NetworkManagerRefab : NetworkManager
{
    public Transform GetRespawnPosition(Transform playerToRespawn)
    {
        float distance = float.MaxValue;
        Transform spawnPoint = new GameObject().transform;
        List<Transform> allPlayers = GameManager.GetAllPlayers().Select(x => x.transform).ToList();
        //Get the spawnpoint that id the furthest away from all players
        foreach (Transform spawnPointTransform in startPositions)
        {
            float tempDistance = 0;
            foreach (Transform player in allPlayers)
            {
                tempDistance += Vector3.Distance(spawnPointTransform.position, player.position);
            }
            if (tempDistance > distance)
            {
                distance = tempDistance;
                spawnPoint = spawnPointTransform;
            }
        }
        return spawnPoint;
    }
}
