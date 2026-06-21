using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerCombat playerCombat;

    public InputActionAsset InputActions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private InputAction rollAction;
    private float moveActionValue;

    private Rigidbody rb;
    private BoxCollider coll;

    private GameObject opponentPlayer;

    // EDITABLE //
    private const float WALK_SPEED = 4;
    private const float CROUCH_MULTIPLIER = 0.5f;
    private const float JUMP_STRENGTH = 650;
    private const float CROUCH_SIZE = 0.9f;
    private const float CROUCH_CENTER = 1.35f;
    private const float UNCROUCH_SIZE = 1.8f;
    private const float UNCROUCH_CENTER = 0.9f;
    // EDITABLE //

    private bool isGrounded;
    private bool isCrouched;
    private bool isRolling;

    private bool canDoubleJump;

    //private float timer;



    // Enables the inputs whenever the player object is active
    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    // Sets all neccessary variables
    private void Awake()
    {
        if (gameObject.name == "Player1")
        {
            opponentPlayer = GameObject.Find("Player2");
        }
        else
        {
            opponentPlayer = GameObject.Find("Player1");
        }

        playerCombat = gameObject.GetComponent<PlayerCombat>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        crouchAction = InputSystem.actions.FindAction("Crouch");
        rollAction = InputSystem.actions.FindAction("Roll");

        rb = gameObject.GetComponent<Rigidbody>();
        coll = gameObject.GetComponent<BoxCollider>();
    }

    // Checks whether the player has pressed an input, then do the thing
    void Update()
    {
        if (Time.timeScale != 0)
        {
            moveActionValue = moveAction.ReadValue<float>();

            if (isGrounded)
            {
                Move();
            }

            if (jumpAction.WasPressedThisFrame() && isGrounded && !isCrouched && !isRolling)
            {
                canDoubleJump = true;
                Jump();
            }
            else if (jumpAction.WasPressedThisFrame() && !isGrounded && canDoubleJump)
            {
                canDoubleJump = false;
                Jump();
            }

            if (crouchAction.WasPressedThisFrame() && isGrounded && !isCrouched && !isRolling)
            {
                Crouch();
            }
            else if (crouchAction.WasPressedThisFrame() && isGrounded && isCrouched && !isRolling)
            {
                UnCrouch();
            }

            if (rollAction.WasPressedThisFrame() && moveActionValue > 0 && isGrounded && !isRolling)
            {
                StartCoroutine(Roll(1)); 
            }
            else if (rollAction.WasPressedThisFrame() && moveActionValue < 0 && isGrounded && !isRolling)
            {
                StartCoroutine(Roll(-1));
            }
        }
    }

    // Checks whether the player is on the ground
    private void OnCollisionEnter(Collision collision)
    {
        if (!isGrounded && collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = true;
            //Debug.Log(timer);
            //timer = 0;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (isGrounded && collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = false;
            canDoubleJump = true;
            //StartCoroutine(Timer());
        }
    }


    // FUNCTIONS //
    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForceAtPosition(Vector3.up * JUMP_STRENGTH, Vector3.up, ForceMode.Impulse);
    }

    void Move()
    {
        float speedMultiplier;
        Vector3 velocity = rb.linearVelocity;

        // Player rotation
        if (opponentPlayer.transform.position.x < gameObject.transform.position.x && MathF.Round(gameObject.transform.rotation.y, 1) == 0.7f /* to right */)
        {
            moveActionValue = -moveActionValue;
            gameObject.transform.rotation = Quaternion.Euler(0, -90, 0);
            Debug.Log("Left");
        }
        else if (opponentPlayer.transform.position.x > gameObject.transform.position.x && MathF.Round(gameObject.transform.rotation.y, 1) == -0.7f /* to left */)
        {
            gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
            Debug.Log("Right");
        }

        // Action 1 + A or B (B.1 or B.2) //

        // Action 1
        if (isCrouched)
        {
            speedMultiplier = CROUCH_MULTIPLIER;
        }
        else
        {
            speedMultiplier = 1f;
        }

        // Action A
        if (playerCombat.isParrying || playerCombat.isBlocking)
        {
            rb.linearVelocity = new Vector3(PlayerCombat.PARRY_OR_BLOCK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }

        // Action B.1
        if (playerCombat.isLightAttacking)
        {
            rb.linearVelocity = new Vector3(PlayerCombat.LIGHT_ATTACK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }
        // Action B.2
        else if (playerCombat.isHeavyAttacking)
        {
            rb.linearVelocity = new Vector3(PlayerCombat.HEAVY_ATTACK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }
        else
        {
            rb.linearVelocity = new Vector3(WALK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }
    }

    void Crouch()
    {
        isCrouched = true;

        coll.size = new Vector3(coll.size.x, CROUCH_SIZE, coll.size.z);
        coll.center = new Vector3(coll.center.x, CROUCH_CENTER, coll.center.z);
        transform.position -= new Vector3(0, 0.9f, 0);
        Debug.Log("Crouch");
    }
    void UnCrouch()
    {
        isCrouched = false;

        coll.size = new Vector3(coll.size.x, UNCROUCH_SIZE, coll.size.z);
        coll.center = new Vector3(coll.center.x, UNCROUCH_CENTER, coll.center.z);
        transform.position += new Vector3(0, 0.9f, 0);
        Debug.Log("Uncrouch");
    }

    IEnumerator Roll(float direction)
    {
        isRolling = true;

        float time = 0;
        float duration = 0.4f;

        Crouch();
        rb.linearDamping = 0;
        while (time < duration)
        {
            rb.AddForce(new Vector3(13000 * direction, 0, 0), ForceMode.Force);
            yield return new WaitForSeconds(0.01f);
            time += 0.01f;
        }
        UnCrouch();
        rb.linearDamping = 0.6f;
        rb.linearVelocity = Vector3.zero;

        isRolling = false;
    }

    /*
    IEnumerator Timer()
    {
        while (!isGrounded)
        {
            yield return new WaitForSeconds(0.01f);
            timer += 0.01f;
        }
    }
    */
}