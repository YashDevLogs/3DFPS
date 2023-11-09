using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the movement of the player with given input from the input manager
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The Speed at which Player moves")]
    public float moveSpeed = 2f;
    [Tooltip("The Speed at which Player rotates to look left and right (calculated in degrees)")]
    public float lookSpeed = 60f;
    [Tooltip("The Power with which the player jumps")]
    public float jumpPower = 8f;
    [Tooltip("The strength of gravity")]
    public float gravity = 9.81f;
    [Header("jump time leniency")]
    public float jumpTimeLeniency = 0.1f;
    float timeToStopBeingLineant = 0;
    [Header("Required Reference")]
    [Tooltip("The player shooter script that shoots projectile")]
    public Shooter playerShooter;
    public Health playerHealth;
    public List<GameObject> disabledWhileDead;
    bool doubleJumpAvailable = false;

    // the character controller component on player
    private CharacterController controller;
    private InputManager inputManager;

    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update call
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Start()
    {
        setUpCharacterController();
        setUpInputManager();
    }

    private void setUpCharacterController() 
    {
        controller = GetComponent<CharacterController>();
        if (controller== null) 
        {
            Debug.LogError("The player controller sscript does not have character controller on the same game object");
        }
    }

    void setUpInputManager() 
    {
        inputManager = InputManager.instance;
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once every frame
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Update()
    {
        if (playerHealth.currentHealth <= 0)
        {
            foreach ( GameObject inGameObject in disabledWhileDead) 
            {
                inGameObject.SetActive(false);
            }
            return;
           
        }
        else
        {
            foreach (GameObject inGameObject in disabledWhileDead)
            {
                inGameObject.SetActive(true);
            }
        }

        ProcessMovement();
        ProcessRotation();
    }

    Vector3 moveDirection;
    void ProcessMovement()
    {
        // get the input from input manager
        float leftRightInput = inputManager.horizontalMoveAxis;
        float forwardBackwardInput = inputManager.verticalMoveAxis;
        bool jumpPressed = inputManager.jumpPressed;

        // handle the control of player while player is on ground
        if (controller.isGrounded)
        {
            doubleJumpAvailable = true;

            timeToStopBeingLineant = Time.time + jumpTimeLeniency;
            //set movement direction to recieved input, set y = 0 as we are on ground
            moveDirection = new Vector3(leftRightInput, 0, forwardBackwardInput);
            //set the move direction in relation to transform
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection = moveDirection * moveSpeed;

            if(jumpPressed) 
            {
                moveDirection.y = jumpPower;
            }
        }
        else
        {
            moveDirection = new Vector3 (leftRightInput * moveSpeed, moveDirection.y, forwardBackwardInput * moveSpeed);
            moveDirection = transform.TransformDirection(moveDirection);

            if (jumpPressed && Time.time < timeToStopBeingLineant)
            {
                moveDirection.y = jumpPower;
            }

            else if(jumpPressed && doubleJumpAvailable)
            {
                moveDirection.y = jumpPower;
                doubleJumpAvailable = false;
            }

        }

        moveDirection.y -= gravity * Time.deltaTime;

        if (controller.isGrounded && moveDirection.y<0)
        {
            moveDirection.y = -3f;
        }

        controller.Move(moveDirection * Time.deltaTime);
    
     }

    void ProcessRotation()
    {
        float horizontalLookInput = inputManager.horizontalLookAxis;
        Vector3 playerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(playerRotation.x, playerRotation.y + horizontalLookInput * lookSpeed * Time.deltaTime, playerRotation.z));
    }
}
