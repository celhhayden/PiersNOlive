using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuddyController : MonoBehaviour
{
    public GameObject theBuddy;
    public GameObject buddyModel;

    public GameObject playerModel;

    // THIS IS NOT STAYING LIKE THIS
    // eventually will have a lil bird model and access different textures within a certain filepath, or similar
    public Material one;
    public Material two;
    public Material three;
    public Material four;
    public Material five;

    // speed at which buddy should move about
    public float flySpeed = 10f;

    //public Transform idleAnchor;    // used for reseting model's center position
    public Transform restAnchor;      // when player has 1 hit left
    public Transform burstAnchor;     // location buddy goes to when bursting

    public GameObject idleAnchorParent;
    private Transform[] idleAnchors;

    private Renderer buddyRenderer;

    private HealthManager healthManager;

    private bool lastHit = false;

    public Queue<GameObject> goldPickupQueue;
    private GameObject currFetchTarget;

    // flags for status of buddy
    private bool isFetching = false;
    private bool isBursting = false;

    // values for flight idling
    public float minIdleTime = 1.2f;
    public float maxIdleTime = 5.2f;
    private float currIdleTime = 0f;
    private int currIdleIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        buddyRenderer = buddyModel.GetComponent<Renderer>();

        //modelAnchor = playerModel.transform;

        GameObject gameManager = GameObject.Find("GameManager");
        HealthManager healthManager = gameManager.GetComponent<HealthManager>();

        goldPickupQueue = new Queue<GameObject>();

        // get idle anchors
        idleAnchors = idleAnchorParent.GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    // using late update in case inconsistencies with update order
    void LateUpdate()
    {
        // TODO: have buddy hover, flit around player when idle
        // TODO: have buddy trail behind player when moving/charging
        // TODO: once burst attack implemented, have buddy come to front of player for this
        // TODO: have a check for healing/respawn points and pickups in a radius
        // TODO: check for nearest pickup and point in that direction

        // have buddy rotate with player model instead of camera
        //buddyModel.transform.rotation = playerModel.transform.rotation;

        if (lastHit)
        {
            // buddy status flags should have no effect, since fetching cant happen
            Rest();
        }
        else
        {
            // first check for burst, then check pickup queue
            if (currFetchTarget == null && goldPickupQueue.Count > 0)
            {
                currFetchTarget = goldPickupQueue.Dequeue();

                if (currFetchTarget.GetComponent<GoldPickup>() != null)
                {
                    isFetching = true;
                }
            }
            else if(isFetching)
            {
                FetchPickup();
            }
            else if(isBursting)
            {
                Burst();
            }
            else
                Idle();
        }
    }

    // any time player takes damage or heals, health buddy should change appearance
    // when player is at 1 hit left, it will rest on player instead of fly around
    public void HealthSet(int amount)
    {
        // based on player's current health, change the buddy's appearance
        switch (amount)
        {
            case 1:
                buddyRenderer.material = one;
                lastHit = true;
                break;
            case 2:
                buddyRenderer.material = two;
                lastHit = false;
                break;
            case 3:
                buddyRenderer.material = three;
                lastHit = false;
                break;
            case 4:
                buddyRenderer.material = four;
                lastHit = false;
                break;
            default:
                buddyRenderer.material = five;
                lastHit = false;
                break;
        }
    }

    // if the buddy is not being used for anything, this makes sure it remains near the player
    // and handles at 1 hp as well
    void Idle()
    {
        // TODO: have a buddy goto player idle area, as well as an idle 'anim' once reached
        // will randomly sit at a spot for an amount of time before going to the next one
        // there will be a list of predetermined spots the buddy will fly to during idle

        // get random length of time buddy will remain at this position
        // once this time has passed, randomly select a new position to idle at
        // curr location inclusive

        // the index of which location buddy should target during idle

        // continue conting down timer
        if (currIdleTime > 0)
            currIdleTime -= Time.deltaTime;
        // if timer has reached 0, the select a new spot and new time
        else
        {
            currIdleIndex = Random.Range(1, idleAnchors.Length - 1);

            currIdleTime = Random.Range(minIdleTime, maxIdleTime);
        }

        theBuddy.transform.position = Vector3.Lerp(theBuddy.transform.position, idleAnchors[currIdleIndex].position, flySpeed * Time.deltaTime);
        theBuddy.transform.rotation = playerModel.transform.rotation;

    }

    // handles buddy location for burst action
    void Burst()
    {
        // forget all queued items from pickup, and prioritize burst
        // after all, can only be in one place at a time
        goldPickupQueue.Clear();
        isFetching = false;
        currFetchTarget = null;

        theBuddy.transform.position = burstAnchor.position;
        theBuddy.transform.rotation = playerModel.transform.rotation;
    }

    void Rest()
    {
        // ensure buddy doesnt fetch when its supposed to be resting
        goldPickupQueue.Clear();
        isFetching = false;
        currFetchTarget = null;

        theBuddy.transform.position = restAnchor.position;
        theBuddy.transform.rotation = playerModel.transform.rotation;
    }

    // if a pickup is within a certain radius of the player, have buddy fly over and pick it up
    // if player is at 1 hit left, buddy will not do this
    void FetchPickup()
    {
        if (currFetchTarget != null)
        {
            theBuddy.transform.position = Vector3.Lerp(theBuddy.transform.position, currFetchTarget.transform.position, flySpeed * Time.deltaTime);

            // have buddy only rotate around y axis by making LookAt think the currFetchTarget is at same pos.y
            // have .x and .z be same as fetch target and .y same as buddy
            Vector3 lookTarget = new Vector3(currFetchTarget.transform.position.x, theBuddy.transform.position.y, currFetchTarget.transform.position.z);
            // rotate buddy's front to face the pickup target
            theBuddy.transform.LookAt(lookTarget);
        }
        else
        {
            isFetching = false;
            currFetchTarget = null;
        }
    }

    // while holding a certain key, buddy will point in the direction of the nearest pickup
    void FindPickup()
    {

    }

    // called by an external class to add a pickup to this buddy's queue
    public void AddPickup(GameObject toAdd)
    {
        // only add if the compnent is of type gold pickup
        // and is not already in queue
        if (toAdd.GetComponent<GoldPickup>() != null && !goldPickupQueue.Contains(toAdd))
        {
            Debug.Log("adding pickup " + toAdd.name);
            goldPickupQueue.Enqueue(toAdd);
        }
    }

    // buddy box triggers
    private void OnTriggerEnter(Collider other)
    {
        // if the buddy is touching the pickup its searching for, grab it
        if (isFetching && other.gameObject == currFetchTarget && other.tag == "Pickup" && other.gameObject.GetComponent<GoldPickup>() != null)
        {
            GoldPickup pickupTarget = other.gameObject.GetComponent<GoldPickup>();

            isFetching = false;
            currFetchTarget = null;
            pickupTarget.ObtainPickup();
        }
    }
}
