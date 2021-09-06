using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderManager : MonoBehaviour
{
    public GameObject thePlayer;
    private CharacterController playerController;

    public HealthManager healthManager;

    public GameObject theBuddy;
    private BuddyController buddyController;

    public GameObject pickupCollider;
    public GameObject burstCollider;

    // Start is called before the first frame update
    void Start()
    {
        playerController = thePlayer.GetComponent<CharacterController>();
        buddyController = theBuddy.GetComponent<BuddyController>();
    }

    // Update is called once per frame
    void Update()
    {
        // if the player is in the air or their health is at 1, deactivate the pickup boxes
        if (!playerController.isGrounded || healthManager.currHealth <= 1)
        {
            pickupCollider.SetActive(false);
        }
        else
            pickupCollider.SetActive(true);

    }

}
