using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar] private float currentHealth;
    [SyncVar] public string username = "Noob";
    
    [SyncVar]
    private bool _isDead = false;
    public bool isDead { get { return _isDead;  } protected set { _isDead = value; } }
    private int _kills = 0;
    public int kills { get { return _kills; } set { _kills = value; } }

    private int _deaths = 0;
    public int deaths { get { return _deaths; } set { _deaths = value; } }
    
    [ClientRpc]
    public void RpcTakeDamage(float damage, string sourceID)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0) Die(sourceID);
    }
    
    private void Die(string sourceID)
    {

        Player sourcePlayer = GameManager.GetPlayer(sourceID);
        if (sourcePlayer != null)
        {
            sourcePlayer.kills++;
            GameManager.instance.onPlayerKilledCallback.Invoke(username, sourcePlayer.username);
        }
        deaths++;


        isDead = true;

        StartCoroutine(Respawn());
    }
    
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2);
        
        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }
    
    
}
