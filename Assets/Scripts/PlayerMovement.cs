using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset InputActions;

    private Rigidbody rigidBodyPlayer;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private float moveActionValue;

    private const float WALK_SPEED = 3.5f;
    private const float CROUCH_SPEED = WALK_SPEED / 2;
    private const float JUMP_STRENGTH = 5;
    private const float CROUCH_SIZE = 0.4f;
    private const float UNCROUCH_SIZE = 1;

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

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        crouchAction = InputSystem.actions.FindAction("Crouch");

        rigidBodyPlayer = gameObject.GetComponent<Rigidbody>();
    }

    // Player movement: jump, move, crouch
    void Update()
    {
        moveActionValue = moveAction.ReadValue<float>();

        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            Jump();
        }

        if (crouchAction.WasPressedThisFrame() && isGrounded)
        {
            Crouch();
        }
        if (!isGrounded && isCrouched)
        {
            UnCrouch();
        }
    }
    void FixedUpdate()
    {
        if (isGrounded)
        {
            Move();
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
        rigidBodyPlayer.AddForceAtPosition(Vector3.up * JUMP_STRENGTH, Vector3.up, ForceMode.Impulse);
    }
    void Move()
    {
        if (!isCrouched)
        {
            Vector3 velocity = rigidBodyPlayer.linearVelocity;
            rigidBodyPlayer.linearVelocity = new Vector3(WALK_SPEED * moveActionValue, velocity.y, velocity.z);
        }
        else if (isCrouched)
        {
            Vector3 velocity = rigidBodyPlayer.linearVelocity;
            rigidBodyPlayer.linearVelocity = new Vector3(CROUCH_SPEED * moveActionValue, velocity.y, velocity.z);
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
