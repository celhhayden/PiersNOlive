using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Player Health")]
    [Range(1, 5)]
    public int maxHealth;
    [Range(1, 5)]
    public int currHealth;

    [Header("Other")]
    public PlayerController thePlayer;
    public BuddyController theBuddy;
    public Transform thePlayerLocation;

    public float invincibilityLength;
    private float invincibilityCounter;

    private bool isRespawning;
    private Vector3 respawnPosition;
    public Transform currRespawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        currHealth = maxHealth;

       // thePlayer = FindObjectOfType<PlayerController>();

        respawnPosition = currRespawnPoint.transform.position;

        theBuddy.HealthSet(currHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if(invincibilityCounter > 0)
        {
            invincibilityCounter -= Time.deltaTime;
        }
    }

    public void HurtPlayer(int damage, Vector3 direction)
    {
        if (invincibilityCounter <= 0)
        {
            currHealth -= damage;

            if (currHealth <= 0)
            {
                Debug.Log("before all transform" + thePlayer.transform.position);
                Respawn();

                // DEBUG: player position will change but not really idk why the FUKCOWR WONT DO WHAT THE CODE SAYS TOD O
                Debug.Log("after all transform" + thePlayer.transform.position);
            }
            else
            {
                thePlayer.Knockback(direction);

                invincibilityCounter = invincibilityLength;
            }
        }

        theBuddy.HealthSet(currHealth);

        Debug.Log("end of HurtPlayer()" + thePlayer.transform.position);
    }

    public void Respawn()
    {
        thePlayer.Teleport(respawnPosition);

        currHealth = maxHealth;
    }

    public void SetRespawn(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }

    public void HealPlayer(int recover)
    {
        currHealth += recover;
        if (currHealth > maxHealth) currHealth = maxHealth;

        theBuddy.HealthSet(currHealth);
    }
}
