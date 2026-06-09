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

    private Rigidbody plrRigidBody;
    private BoxCollider plrCollider;

    // EDITABLE //
    private const float WALK_SPEED = 3.5f;
    private const float CROUCH_MULTIPLIER = 0.5f;
    private const float JUMP_STRENGTH = 5;
    private const float CROUCH_SIZE = 0.9f;
    private const float CROUCH_CENTER = 1.35f;
    private const float UNCROUCH_SIZE = 1.8f;
    private const float UNCROUCH_CENTER = 0.9f;
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

        plrRigidBody = gameObject.GetComponent<Rigidbody>();
        plrCollider = gameObject.GetComponent<BoxCollider>();
    }

    // Checks whether the player has pressed an input, then do the thing
    void Update()
    {
        if (Time.timeScale == 1)
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

            if (crouchAction.WasPressedThisFrame() && isGrounded && !isCrouched)
            {
                Crouch();
            }
            else if (crouchAction.WasPressedThisFrame() && isGrounded && isCrouched)
            {
                UnCrouch();
            }
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
        plrRigidBody.AddForceAtPosition(Vector3.up * JUMP_STRENGTH, Vector3.up, ForceMode.Impulse);
    }

    void Move()
    {
        float speedMultiplier;
        Vector3 velocity = plrRigidBody.linearVelocity;

        /*
        Debug.Log(moveActionValue + " " + MathF.Round(gameObject.transform.rotation.y, 1));
        if (moveActionValue < 0 && MathF.Round(gameObject.transform.rotation.y, 1) == 0.7f)
        {
            Debug.Log("Left");
            gameObject.transform.Rotate(0, 180, 0);
        }
        else if (moveActionValue > 0 && MathF.Round(gameObject.transform.rotation.y, 1) == -0.7f)
        {
            Debug.Log("Right");
            gameObject.transform.Rotate(0, 180, 0);
        }
        */

        // Action 1 + A or B (B.1 or B.2)

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
            plrRigidBody.linearVelocity = new Vector3(PlayerCombat.PARRY_OR_BLOCK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }

        // Action B.1
        if (playerCombat.isLightAttacking)
        {
            plrRigidBody.linearVelocity = new Vector3(PlayerCombat.LIGHT_ATTACK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }
        // Action B.2
        else if (playerCombat.isHeavyAttacking)
        {
            plrRigidBody.linearVelocity = new Vector3(PlayerCombat.HEAVY_ATTACK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }
        else
        {
            plrRigidBody.linearVelocity = new Vector3(WALK_SPEED * speedMultiplier * moveActionValue, velocity.y, velocity.z);
            return;
        }
    }
    void Crouch()
    {
        isCrouched = true;

        plrCollider.size = new Vector3(plrCollider.size.x, CROUCH_SIZE, plrCollider.size.z);
        plrCollider.center = new Vector3(plrCollider.center.x, CROUCH_CENTER, plrCollider.center.z);
        transform.position -= new Vector3(0, 0.9f, 0);
        Debug.Log("Crouch");
    }
    void UnCrouch()
    {
        isCrouched = false;

        plrCollider.size = new Vector3(plrCollider.size.x, UNCROUCH_SIZE, plrCollider.size.z);
        plrCollider.center = new Vector3(plrCollider.center.x, UNCROUCH_CENTER, plrCollider.center.z);
        transform.position += new Vector3(0, 0.9f, 0);
        Debug.Log("Uncrouch");
    }
}