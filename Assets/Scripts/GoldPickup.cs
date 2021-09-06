using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    // the worth of this pickup
    public int value = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ObtainPickup()
    {
        FindObjectOfType<GameManager>().AddGold(value);

        // removes from world entirely
        Destroy(gameObject);
    }

    // unity built in for ___ collider trigger box
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Buddy")
        {
            Debug.Log("Touching " + this.name);
            ObtainPickup();
        }
        else if(other.tag == "Player")
        {
            // only be picked up by inner pickup radius
            if (other.gameObject.name == "SmallerCollider")
                ObtainPickup();
        }
    }
}
