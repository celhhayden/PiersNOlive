using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Objects")]
    public Transform target;        // the entire target the camera is looking at
    public Transform targetHead;    // the target's head for when zooming
    public Transform pivotX;        // used to help with left/right camera rotations w/o rotating target
    public Transform pivotY;        // used to help with up/down camera rotations w/o rotating target

    public GameObject playerModel;
    public CharacterController controller;

    private PlayerController playerController;

    // how far from player should be
    [Header("Camera Offsets")]
    public Vector3 offset;
    public float zoomOffsetMultiplier = 0.25f;
    public float chargeOffsetMultiplier = 0.75f;

    private Vector3 zoomOffset;
    private Vector3 chargeOffset;
    private Vector3 currOffset;

    public bool useOffsetValues;

    [Header("Camera Speeds")]
    public float zoomSpeed = 5f;

    public float rotateSpeed = 1f;
    public float rotateSnapSpeed = 3f;

    [Header("Angles")]
    public float maxViewAngle = 65f;    // for normal walking, up and down bounds
    public float minViewAngle = -55f;
    public float maxZoomAngle = 60f;    // for zooming, up and down bounds
    public float minZoomAngle = -40f;
    public float runAngle = 60f;        // for running, left and right bounds, on top of up and down from normal walking
    public float glideAngle = 60f;      // for gliding, left and right bounds, on top of up and down from normal walking


    public float autoRotateSpeed = 1f;
    public float autoRunRotateSpeed = 2f;

    [Header("Flags")]
    public bool isAuto = true;          // toggle if camera will tend to settle behind player while moving or not
    public bool turnAuto = true;        // will always follow behind player when charging and gliding
    bool isSettled = true;              // flag if player meets condition to temporarily disable auto during standstill
    public bool invertY = false;        // toggle to flip y rotation direction
    bool isSnap = false;                // prohibits any other camera movement while true after snap button is pressed; false when done
    bool isZoom = false;                // flag for when player is holding the zoom in



    // Start is called before the first frame update
    void Start()
    {
        // get player script to reference player states
        GameObject thePlayer = GameObject.Find("Player");
        playerController = thePlayer.GetComponent<PlayerController>();

        if (!useOffsetValues)
        {
            offset = target.position - transform.position;
        }

        zoomOffset = zoomOffsetMultiplier * offset;
        chargeOffset = chargeOffsetMultiplier * offset;

        currOffset = offset;

        // move pivot to where player head is
        pivotX.transform.position = targetHead.transform.position;
        pivotY.transform.position = targetHead.transform.position;
        
        // need pivot separate from camera but will keep as child until code executes in case want to use
        pivotX.transform.parent = null;
        pivotY.transform.parent = null;

        // hides cursor immediately upon game start & hides it
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    // call late update to make sure this happens after everything else for consistency
    void LateUpdate()
    {
        // TODO: when player holds in certain button, should zoom in closer to rear of player for stationary look-arounds

        // (DONE) TODO: make slight zoom in on character when charging

        // TODO: for controller/gamepad, have hold triggers rotate camera at steady rate

        // TODO: make sure camera doesn't collide with terrain
        //       will work on this later when some actual object are introduced

        isZoom = Input.GetKey(KeyCode.LeftAlt);

        // put the pivots exactly where target is
        pivotX.transform.position = targetHead.transform.position;
        pivotY.transform.position = targetHead.transform.position;

        // default axis control to analog stick. if 0, will check other inputs
        // TODO: more a general todo, but get controller inputs to work properly
        //       input support from DualShock is kinda fucked in general soooo gotta figure that out at some point
        float axisX = Input.GetAxis("Mouse X");
        float axisY = Input.GetAxis("Mouse Y");

        /*
        // get axis values for mouse/analog stick
        if(axisX == 0f)
        {
            axisX = Input.GetAxis("Trigger X");
        }
        else if (axisX == 0f)
        {
            axisX = Input.GetAxis("JoyR X");
        }

        if(axisY == 0f)
        {
            axisY = Input.GetAxis("JoyR Y");
        }
        */

        // get x pos of mouse and rotate target, rotate left/right
        float horizontal = axisX * rotateSpeed;
        pivotX.Rotate(0f, horizontal, 0f);

        // get Y pos of mouse & rotate the pivot
        float vertical = axisY * rotateSpeed;

        // invert based on settings
        if(!invertY)
        {
            vertical *= -1;
        }
        // pivot up/down
        pivotY.Rotate(vertical, 0f, 0f);

        // pivot has now been rotated based on mouse movement

        // limit up/down camera rotation bounds
        if (pivotY.rotation.eulerAngles.x > maxViewAngle && pivotY.rotation.eulerAngles.x < 180f)
        {
            pivotY.rotation = Quaternion.Euler(maxViewAngle, 0f, 0f);
        }

        if (pivotY.rotation.eulerAngles.x > 180f && pivotY.rotation.eulerAngles.x < 360f + minViewAngle)
        {
            pivotY.rotation = Quaternion.Euler(360f + minViewAngle, 0f, 0f);
        }

        // if the payer is holding "triangle" then zoom the cam to behind player head into
        // near-first-person perspective, where player head will move in direction the cam faces
        // while the the player pos does not change
        CloseZoom();

        // if player is running then camera should be at a different distance behind player
        if (Input.GetButton("Charge") && !playerController.isGliding)
        {
            currOffset = Vector3.Lerp(currOffset, chargeOffset, zoomSpeed * Time.deltaTime);
        }
        // if player is not running and cam has yet to return to normal, calculate lerp to do so
        else if(currOffset != offset)
        {
            currOffset = Vector3.Lerp(currOffset, offset, zoomSpeed * Time.deltaTime);
        }

        // move camera based on curr rotation of pivots & original offset
        float desiredXAngle = pivotY.eulerAngles.x;
        float desiredYAngle = pivotX.eulerAngles.y;
        // euler converts quaternion vector vec4 into more easily understood vec3 since don't need w

        Quaternion rotation = Quaternion.Euler(desiredXAngle, desiredYAngle, 0f);
        transform.position = pivotX.position - (rotation * currOffset);

        // prevents camera from going into the ground
        // TODO (here?): end up using cam collision with terrain
        if (transform.position.y < target.position.y - 0.5f)
        {
            transform.position = new Vector3(transform.position.x, target.position.y - 0.5f, transform.position.z);
        }

        // makes the cam tend towards' player rear at all times when moving
        ActiveCam();

        // turn cam always behind player if player holds in run or glide
        TurnCam();

        // makes the camera snap to the rear of the player after pressing the appropriate key
        SnapCam();

        transform.LookAt(target);
    }

    // moves camera to rear of player's head, almost like just barely a first person perspective
    // activates upon pressing "triangle" (currently F key)
    // this also prevents player from moving at all, and the model head looks in the direction of the cam look
    private void CloseZoom()
    {
        // if zooming while standing still
        if (controller.isGrounded && Input.GetButton("Flutter Zoom"))
        {
            // slow zoom to player
            currOffset = Vector3.Lerp(currOffset, zoomOffset, zoomSpeed * Time.deltaTime);
        }
    }

    // used for an active camera, makes it always tend to the player's rear if they are 
    // not standing still and manually moving the camera
    private void ActiveCam()
    {
        // if not holding movement (any for now) key and mouse has been moved, then do not auto move camera
        if (!Input.anyKey && (Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f))
        {
            isSettled = true;
        }

        // the moment a key is pressed then player is no longer settled
        else if (Input.anyKey) isSettled = false;

        // will make camera's rotation tend towards player's rear if auto camera is desired
        if (isAuto && !isSettled && !Input.GetButton("Flutter Zoom"))
        {
            float currRotateSpeed = autoRotateSpeed;

            if (Input.GetButton("Charge"))
            {
                currRotateSpeed = autoRunRotateSpeed;
            }

            // if player is walking backwards then rotate camera differently
            if (Input.GetAxis("Vertical") > -0.8f)
            {

                // this will slowly rotate the pivot to face the same direction as the player model
                pivotX.transform.rotation = Quaternion.Slerp(pivotX.transform.rotation, playerModel.transform.rotation, currRotateSpeed * Time.deltaTime);
                pivotY.transform.rotation = Quaternion.Slerp(pivotY.transform.rotation, playerModel.transform.rotation, currRotateSpeed * Time.deltaTime);

            }
        }
    }

    private void TurnCam()
    {
        // will make camera's rotation tend towards player's rear if auto camera is desired
        if (turnAuto && playerController.isRunning)
        {
            //pivotX.transform.rotation = Quaternion.Slerp(pivotX.transform.rotation, target.rotation, rotateSnapSpeed * Time.deltaTime);
            if (pivotX.rotation.eulerAngles.y > runAngle + target.rotation.y && pivotX.rotation.eulerAngles.y < 180f + target.rotation.y)
            {
                pivotX.rotation = Quaternion.Slerp(pivotX.rotation, Quaternion.Euler(0f, runAngle + target.rotation.y, 0f), rotateSnapSpeed * Time.deltaTime);
            }

            if (pivotX.rotation.eulerAngles.y > 180f + target.rotation.y && pivotX.rotation.eulerAngles.y < 360f - runAngle + target.rotation.y)
            {
                pivotX.rotation = Quaternion.Slerp(pivotX.rotation, Quaternion.Euler(0f, 360f - runAngle + target.rotation.y, 0f), rotateSnapSpeed * Time.deltaTime);
            }
        }
        // will simply limit cam rotation to bounds when gliding
        if (turnAuto && playerController.isGliding)
        {
            pivotX.transform.rotation = Quaternion.Slerp(pivotX.transform.rotation, target.rotation, rotateSnapSpeed * Time.deltaTime);
        }
    }

    // makes the camera snap to the rear of the player
    private void SnapCam()
    {
        if ((isSnap || Input.anyKey) && !Input.GetButton("Snap Cam"))
        {
            isSnap = false;
        }
        else if (Input.GetButtonDown("Snap Cam") || isSnap)
        {
            isSnap = true;
            pivotX.transform.rotation = Quaternion.Slerp(pivotX.transform.rotation, playerModel.transform.rotation, rotateSnapSpeed * Time.deltaTime);
            pivotY.transform.rotation = Quaternion.Slerp(pivotY.transform.rotation, playerModel.transform.rotation, rotateSnapSpeed * Time.deltaTime);
            // if pivot rotation matches playerModel rotation, snap is done
            if ((pivotX.transform.rotation.y == target.transform.rotation.y) && (pivotY.transform.rotation.x == target.transform.rotation.x))
            {
                isSnap = false;
            }
        }
    }
}

