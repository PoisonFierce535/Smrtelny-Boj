using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset InputActions;

    private Rigidbody rigidBodyPlayer;

    private InputAction moveAction;
    private InputAction jumpAction;
    private float moveActionValue;

    private const float WALK_SPEED = 3.5f;
    private const float JUMP_STRENGTH = 5;

    private bool isGrounded;



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

        rigidBodyPlayer = gameObject.GetComponent<Rigidbody>();
    }

    // Makes the player move or jump on player's command
    void Update()
    {
        moveActionValue = moveAction.ReadValue<float>();

        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            Jump();
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
        Vector3 velocity = rigidBodyPlayer.linearVelocity;
        rigidBodyPlayer.linearVelocity = new Vector3(WALK_SPEED * moveActionValue, velocity.y, velocity.z);        
    }
}
