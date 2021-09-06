using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterPickupCollider : MonoBehaviour
{

    public GameObject theBuddy;
    private BuddyController buddyController;

    // Start is called before the first frame update
    void Start()
    {
        buddyController = theBuddy.GetComponent<BuddyController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // unity built in for ___ collider trigger box
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Pickup" && other.gameObject.GetComponent("GoldPickup") != null)
        {
            buddyController.AddPickup(other.gameObject);
        }
    }
}
