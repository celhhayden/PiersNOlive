using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;

    [Header("Basic Movement")]
    public float movementSpeed = 5.5f;      // basic movement speed
    public float jumpForce = 10f;           // strength of jump takeoff
    public float jumpHeightSmoother = 2f;   // for when player releases jump preemptively
    public float gravityScale = 2f;         // strength of gravity
    public float rotateSpeed = 10f;         // for turning player

    [Header("Sprint Movement")]
    public float runSpeed = 10f;            // running/charging movement speed
    public float jumpForceRunning = 5f;     // strength of jump takeoff when running
    public float gravityScaleRunning = 2f;  // strength of gravity when running
    public float runTurnSpeed = 120f;         // for turning player while running

    [Header("Glide Movement")]
    public float glidingFallSpeed = 2.2f;   // reduced gravity strength for gliding
    public float glideEndBoost = 1f;        // the boost force when fluttering at end of glide
    public float glideTurnSpeed = 90f;       // for turning player while gliding
    public float flutterJumpHeightSmoother = 7f;   // for when player flutter jumps at end of glide
    public float flutterJumpSpeed = 3f;   // for when player flutter jumps at end of glide

    [Header("Other")]
    // for turning player
    public Transform pivot;
    float turnSmoothVelocity;

    public GameObject playerModel;
    public GameObject playerColliders;
    public GameObject buddyAnchors;

    private Vector3 moveDirection;

    public float knockBackForce;
    public float knockBackTime;
    private float knockBackCounter;
    
    [Header("Flags")]
    public bool isRunning = false;
    // flags for gliding
    private bool hasJumped = false;
    private bool hasFluttered = false;
    private bool canGlide = false;
    public bool isGliding = false;
    private bool ceilingBump = false;      // if player bumped into ceiling they should fall (and eventually have animation)

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: make rate of turning more strict while running
        // TODO: make rate of turning slightly more strict while gliding

        // TODO: when player rotates quickly backwards, should temporarily halt motion;
        //       this might be better done when adding animations

        // All player movement controls are held within this conditional
        // if the player has taken damage, then all movement input should be nullified

        isRunning = Input.GetButton("Charge");
        float currSpeed = movementSpeed;
        float currJumpForce = jumpForce;
        float currGravityScale = gravityScale;

        // if holding the zoom button on the ground, do not allow any movement aside from turning
        if (controller.isGrounded && Input.GetButton("Flutter Zoom"))
        {
            //playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, newRotation, rotateSpeed * Time.deltaTime);
            //playerColliders.transform.rotation = buddyAnchors.transform.rotation = playerModel.transform.rotation;

            //transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);
            //playerColliders.transform.rotation = buddyAnchors.transform.rotation = playerModel.transform.rotation;

            //transform.Rotate(0f, Input.GetAxis("Mouse X"), 0f);
        }
        else if (knockBackCounter <= 0)
        {
            // maintain prev y val to avoid losing it after normalizing later
            float yDirectionPrev = moveDirection.y;
            moveDirection = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));
            moveDirection = moveDirection.normalized * currSpeed;
            // while held, player is running, affecting certain values
            // this still needs controller input from left button as well
            // PLAYER X - Z MOVEMENT SPEED
            if (isRunning && !isGliding)
{
                // values are different for when running
                currSpeed = runSpeed;
                currJumpForce = jumpForceRunning;
                currGravityScale = gravityScaleRunning;

                Charge();

                // TODO: limit speed left and right when sprinting
            }
            // if player fluttered, reduce speed
            else if (hasFluttered)
            {
                currSpeed = flutterJumpSpeed;

                moveDirection = (transform.forward * currSpeed * Input.GetAxis("Vertical")) + (transform.right * currSpeed * Input.GetAxis("Horizontal"));
            }
            // END PLAYER X - Z MOVEMENT SPEED

            moveDirection = moveDirection.normalized * currSpeed;
            moveDirection.y = yDirectionPrev;

            // PLAYER ON GROUND ACTIONS
            if (controller.isGrounded)
            {
                // if grounded these flags should not be true and .y speed should be 0
                hasJumped = false;
                hasFluttered = false;
                canGlide = false;
                isGliding = false;
                ceilingBump = false;

                moveDirection.y = 0f;

                if (Input.GetButton("Jump New"))
                {
                    moveDirection.y = currJumpForce;
                    hasJumped = true;
                }
            }
            // END PLAYER ON GROUND ACTIONS

            // PLAYER IN-AIR ACTIONS
            // when player is in the air
            else
            {
                if (!hasFluttered)
                {
                    // if player has previously jumped and released the jump button
                    if (hasJumped && Input.GetButtonUp("Jump New"))
                    {
                        canGlide = true;
                    }

                    // if above conditions are met and jump is held, player should fall slower
                    if (canGlide && Input.GetButton("Jump New"))
                    {
                        Glide();
                    }
                    else
                    {
                        isGliding = false;
                    }

                    // if player releases jump button early, or begins to run, begin descent
                    if (((!isGliding && Input.GetButtonUp("Jump New")) || Input.GetKeyDown(KeyCode.LeftShift)) && moveDirection.y > jumpHeightSmoother)
                    {
                        // this will ensure player doesn't just immediately stop dropping after release for a smoother appearance
                        moveDirection.y = jumpHeightSmoother;
                    }
                }

                // make player jump stop if bumping into something above it that keeps it from moving
                // detects flags from the player controller
                if ((controller.collisionFlags & CollisionFlags.Above) != 0 && !ceilingBump)
                {
                    moveDirection.y = jumpHeightSmoother;
                    ceilingBump = true;
                }
            }
            // END PLAYER IN-AIR ACTIONS

            // PLAYER MOVEMENT DIRECTION ROTATION
            // move player in different directions based on camera look direction
            // as long as vert n horiz aren't 0 means trying to move player meaning rotation needed
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                // want to get same rotation from pivot to apply to player
                //transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);
                // if gliding or running clamp rotation speed of player
                if (isGliding)
                {

                }
                else if (isRunning)
                {

                }
                else
                    transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);

                // LookRotation is given point in space and says to look/face this direction
                Quaternion newRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0f, moveDirection.z));
                // apply new rotation to player with slerp
                // MoveTowards will use equal move speed the whole way eg 5 then 5 then 5
                // lerp linear interpolation uses percents between values added per 'unit' moved eg 5 then 80% of 5 so on
                // slerp rotation specific, is like lerp but goes along an arc
                // slerp(start, target, how long it will take)
                // want to rotate the visual aspect rather than player entity
                playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, newRotation, rotateSpeed * Time.deltaTime);
                playerColliders.transform.rotation = buddyAnchors.transform.rotation = playerModel.transform.rotation;
            }
            // END PLAYER MOVEMENT DIRECTION ROTATION

        }
        // END PLAYER INPUT-BASED MOVEMENT
        else if (knockBackCounter > 0)
        {
            knockBackCounter -= Time.deltaTime;
        }


        moveDirection.y = moveDirection.y + (Physics.gravity.y * currGravityScale * Time.deltaTime);
        controller.Move(moveDirection * Time.deltaTime);
    }

    public void Walk()
    {

    }

    // TODO: have a damaging hitbox that interacts with entities in level
    // though this will probably be handled in player collider manager
    public void Charge()
    {
        float rotatingSpeed = Input.GetAxis("Horizontal") * Time.deltaTime * runTurnSpeed;
        moveDirection = transform.forward * runSpeed;
        transform.Rotate(0f, rotatingSpeed, 0f);
    }

    // handles gliding slow fall and the subsequent "flutter jump"
    public void Glide()
    {
        isGliding = true;
        ceilingBump = false;    // set this flag false so player can ceiling bump for when flutter is implemented

        float rotatingSpeed = Input.GetAxis("Horizontal") * Time.deltaTime * glideTurnSpeed;
        moveDirection = transform.forward * movementSpeed;
        transform.Rotate(0f, rotatingSpeed, 0f);

        // for constant y transform downwards
        moveDirection.y = -glidingFallSpeed;

        // TODO: limit speed left and right when gliding, no perpendicular movement to camera

        // if gliding, then player can do a flutter jump to end the glide
        if(!hasFluttered && Input.GetButtonDown("Flutter Zoom"))
        {
            canGlide = false;
            isGliding = false;
            hasFluttered = true;

            moveDirection.y = flutterJumpHeightSmoother;
        }
    }

    // knockback upon taking damage
    public void Knockback(Vector3 direction)
    {
        knockBackCounter = knockBackTime;

        moveDirection = direction * knockBackForce;
        moveDirection.y = knockBackForce;
    }

    // can be used for things like respawn
    public void Teleport(Vector3 newPosition)
    {
        GameObject player = GameObject.Find("Player");
        CharacterController charController = player.GetComponent<CharacterController>();

        charController.enabled = false;
        transform.position = newPosition;
        charController.enabled = true;

    }

    // testing different movement structure using rotation like tank controls
    public void OldMovement()
    {
        // TODO: make rate of turning more strict while running
        // TODO: make rate of turning slightly more strict while gliding

        // TODO: when player rotates quickly backwards, should temporarily halt motion;
        //       this might be better done when adding animations

        // All player movement controls are held within this conditional
        // if the player has taken damage, then all movement input should be nullified

        isRunning = Input.GetButton("Charge");
        float currSpeed = movementSpeed;
        float currJumpForce = jumpForce;
        float currGravityScale = gravityScale;

        // if holding the zoom button on the ground, do not allow any movement aside from turning
        if (controller.isGrounded && Input.GetButton("Flutter Zoom"))
        { }
        else if (knockBackCounter <= 0)
        {
            // maintain prev y val to avoid losing it after normalizing later
            float yDirectionPrev = moveDirection.y;
            moveDirection = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));
            moveDirection = moveDirection.normalized * currSpeed;
            // while held, player is running, affecting certain values
            // this still needs controller input from left button as well
            // PLAYER X - Z MOVEMENT SPEED
            if (isRunning && !isGliding)
            {
                // values are different for when running
                currSpeed = runSpeed;
                currJumpForce = jumpForceRunning;
                currGravityScale = gravityScaleRunning;

                moveDirection = (transform.forward * currSpeed * Input.GetAxis("Vertical")) + (transform.right * currSpeed * Input.GetAxis("Horizontal"));

                //if player isn't holding in forward, then force them forward in direction of player model
                if (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0)
                {
                    moveDirection = (playerModel.transform.forward * currSpeed) + (transform.right * currSpeed * Input.GetAxis("Horizontal"));
                }

                // TODO: limit speed left and right when sprinting
            }
            // if player fluttered, reduce speed
            else if (hasFluttered)
            {
                currSpeed = flutterJumpSpeed;

                moveDirection = (transform.forward * currSpeed * Input.GetAxis("Vertical")) + (transform.right * currSpeed * Input.GetAxis("Horizontal"));
            }
            // END PLAYER X - Z MOVEMENT SPEED

            moveDirection = moveDirection.normalized * currSpeed;
            moveDirection.y = yDirectionPrev;

            // PLAYER ON GROUND ACTIONS
            if (controller.isGrounded)
            {
                // if grounded these flags should not be true and .y speed should be 0
                hasJumped = false;
                hasFluttered = false;
                canGlide = false;
                isGliding = false;
                ceilingBump = false;

                moveDirection.y = 0f;

                if (Input.GetButton("Jump New"))
                {
                    moveDirection.y = currJumpForce;
                    hasJumped = true;
                }
            }
            // END PLAYER ON GROUND ACTIONS

            // PLAYER IN-AIR ACTIONS
            // when player is in the air
            else
            {
                if (!hasFluttered)
                {
                    // if player has previously jumped and released the jump button
                    if (hasJumped && Input.GetButtonUp("Jump New"))
                    {
                        canGlide = true;
                    }

                    // if above conditions are met and jump is held, player should fall slower
                    if (canGlide && Input.GetButton("Jump New"))
                    {
                        Glide();
                    }
                    else
                    {
                        isGliding = false;
                    }

                    // if player releases jump button early, or begins to run, begin descent
                    if (((!isGliding && Input.GetButtonUp("Jump New")) || Input.GetKeyDown(KeyCode.LeftShift)) && moveDirection.y > jumpHeightSmoother)
                    {
                        // this will ensure player doesn't just immediately stop dropping after release for a smoother appearance
                        moveDirection.y = jumpHeightSmoother;
                    }
                }

                // make player jump stop if bumping into something above it that keeps it from moving
                // detects flags from the player controller
                if ((controller.collisionFlags & CollisionFlags.Above) != 0 && !ceilingBump)
                {
                    moveDirection.y = jumpHeightSmoother;
                    ceilingBump = true;
                }
            }
            // END PLAYER IN-AIR ACTIONS

            // PLAYER MOVEMENT DIRECTION ROTATION
            // move player in different directions based on camera look direction
            // as long as vert n horiz aren't 0 means trying to move player meaning rotation needed
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                // want to get same rotation from pivot to apply to player
                //transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);
                // if gliding or running clamp rotation speed of player
                if (isGliding)
                {

                }
                else if (isRunning)
                {

                }
                else
                    transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);

                // LookRotation is given point in space and says to look/face this direction
                Quaternion newRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0f, moveDirection.z));
                // apply new rotation to player with slerp
                // MoveTowards will use equal move speed the whole way eg 5 then 5 then 5
                // lerp linear interpolation uses percents between values added per 'unit' moved eg 5 then 80% of 5 so on
                // slerp rotation specific, is like lerp but goes along an arc
                // slerp(start, target, how long it will take)
                // want to rotate the visual aspect rather than player entity
                playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, newRotation, rotateSpeed * Time.deltaTime);
                playerColliders.transform.rotation = buddyAnchors.transform.rotation = playerModel.transform.rotation;
            }
            // END PLAYER MOVEMENT DIRECTION ROTATION

        }
        // END PLAYER INPUT-BASED MOVEMENT
        else if (knockBackCounter > 0)
        {
            knockBackCounter -= Time.deltaTime;
        }


        moveDirection.y = moveDirection.y + (Physics.gravity.y * currGravityScale * Time.deltaTime);
        controller.Move(moveDirection * Time.deltaTime);
    }

    // handles gliding slow fall and the subsequent "flutter jump"
    public void OldGlide()
    {
        isGliding = true;
        ceilingBump = false;    // set this flag false so player can ceiling bump for when flutter is implemented

        // for mandatory forward motion while gliding
        moveDirection = (transform.forward * movementSpeed * Input.GetAxis("Vertical")) + (transform.right * movementSpeed * Input.GetAxis("Horizontal"));
        // TEST:
        //moveDirection = transform.forward * movementSpeed * Input.GetAxis("Vertical");
        //transform.Rotate(0.0f, -Input.GetAxis("Horizontal") * glideTurnSpeed, 0.0f);

        //if player isn't holding in forward, then force them forward in direction of player model
        if (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0)
        {
            //moveDirection = (playerModel.transform.forward * movementSpeed) + (transform.right * movementSpeed * Input.GetAxis("Horizontal"));
            moveDirection = (playerModel.transform.forward * movementSpeed) + (transform.right * movementSpeed * Input.GetAxis("Horizontal"));

        }
        moveDirection = moveDirection.normalized * movementSpeed;
        // for constant y transform downwards
        moveDirection.y = -glidingFallSpeed;

        // TODO: limit speed left and right when gliding, no perpendicular movement to camera

        // if gliding, then player can do a flutter jump to end the glide
        if (!hasFluttered && Input.GetButtonDown("Flutter Zoom"))
        {
            canGlide = false;
            isGliding = false;
            hasFluttered = true;

            moveDirection.y = flutterJumpHeightSmoother;
        }
    }
}
