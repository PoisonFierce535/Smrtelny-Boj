using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerCombat playerCombat;

    public InputActionAsset InputActions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private float moveActionValue;

    private float speedMultiplier;

    private Rigidbody rigidBody;

    // EDITABLE //
    private const float WALK_SPEED = 3.5f;
    private const float CROUCH_SPEED = WALK_SPEED / 2; // divided by 2
    private const float JUMP_STRENGTH = 5;
    private const float CROUCH_SIZE = 0.4f;
    private const float UNCROUCH_SIZE = 1;
    // EDITABLE //

    private bool isGrounded;
    private bool isCrouched;



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
        playerCombat = gameObject.GetComponent<PlayerCombat>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        crouchAction = InputSystem.actions.FindAction("Crouch");

        rigidBody = gameObject.GetComponent<Rigidbody>();
    }

    // Checks whether the player has pressed an input, then do the thing
    void Update()
    {
        moveActionValue = moveAction.ReadValue<float>();

        if (isGrounded)
        {
            Move();
        }

        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            Jump();
        }

        if (crouchAction.WasPressedThisFrame() && isGrounded)
        {
            Crouch();
        }
        else if (!isGrounded && isCrouched)
        {
            UnCrouch();
        }
    }

    // Checks whether the player is on the ground
    private void OnCollisionEnter(Collision collision)
    {
        if (!isGrounded && collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (isGrounded && collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = false;
        }
    }

    // FUNCTIONS //
    void Jump()
    {
        rigidBody.AddForceAtPosition(Vector3.up * JUMP_STRENGTH, Vector3.up, ForceMode.Impulse);
    }
    void Move()
    {
        Vector3 velocity = rigidBody.linearVelocity;
        // Action 1 + A or B (B.1 or B.2)

        // Action 1 (half of the WALK_SPEED)
        if (isCrouched)
        {
            speedMultiplier = 0.5f;
        }
        else
        {
            speedMultiplier = 1f;
        }

        // Action A
        if (playerCombat.isParrying || playerCombat.isBlocking)
        {
            rigidBody.linearVelocity = new Vector3(PlayerCombat.PARRY_OR_BLOCK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }

        // Action B.1
        if (playerCombat.isLightAttacking)
        {
            rigidBody.linearVelocity = new Vector3(PlayerCombat.LIGHT_ATTACK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }
        // Action B.2
        else if (playerCombat.isHeavyAttacking)
        {
            rigidBody.linearVelocity = new Vector3(PlayerCombat.HEAVY_ATTACK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }
        else
        {
            rigidBody.linearVelocity = new Vector3(WALK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }

    }
    void Crouch()
    {
        isCrouched = true;

        transform.localScale = new Vector3(transform.localScale.x, CROUCH_SIZE, transform.localScale.x);
        transform.position -= new Vector3(0, 1 - CROUCH_SIZE, 0);
    }
    void UnCrouch()
    {
        isCrouched = false;

        transform.localScale = new Vector3(transform.localScale.x, UNCROUCH_SIZE, transform.localScale.x);
        transform.position += new Vector3(0, 1 - CROUCH_SIZE, 0);
    }
}
