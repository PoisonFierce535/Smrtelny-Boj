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

    public Transform meshBone;

    private GameObject opponentPlayer;

    private Animator anim;
    private int speedPar;

    // EDITABLE //
    private const float WALK_SPEED = 4;
    private const float JUMP_STRENGTH = 650;
    private const float CROUCH_SIZE = 0.9f;
    private const float CROUCH_CENTER = 0.45f;
    private const float UNCROUCH_SIZE = 1.8f;
    private const float UNCROUCH_CENTER = 0.9f;
    // EDITABLE //

    public bool isGrounded;
    public bool isCrouched;
    public bool isRolling;
    public bool isLanding;

    private bool canDoubleJump;



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

        anim = gameObject.GetComponent<Animator>();
        speedPar = Animator.StringToHash("Speed");
    }

    // Checks whether the player has pressed an input, then do the thing
    void Update()
    {
        if (Time.timeScale != 0)
        {
            moveActionValue = moveAction.ReadValue<float>();

            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("A|Jump_Land"))
            {
                isLanding = true;
            }
            else
            {
                isLanding = false;
            }

            anim.SetFloat(speedPar, rb.linearVelocity.x);
            anim.SetBool("isGrounded", isGrounded);
            if (rb.linearVelocity.y < 0)
            {
                anim.SetBool("isFalling", true);
            }
            else
            {
                anim.SetBool("isFalling", false);
            }


            if (isGrounded && !isRolling)
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

            if (rollAction.WasPressedThisFrame() && rb.linearVelocity.x > 1 && isGrounded && !isRolling)
            {
                StartCoroutine(Roll(1)); 
            }
            else if (rollAction.WasPressedThisFrame() && rb.linearVelocity.x < -1 && isGrounded && !isRolling)
            {
                StartCoroutine(Roll(-1));
            }
        }
    }
    private void LateUpdate()
    {
        if (meshBone == null) return;

        meshBone.localPosition = new Vector3(0f, meshBone.localPosition.y, 0f);
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
            canDoubleJump = true;
        }
    }


    // FUNCTIONS //
    void Jump()
    {
        if (!isLanding)
        {
            anim.SetTrigger("Jump");
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForceAtPosition(Vector3.up * JUMP_STRENGTH, Vector3.up, ForceMode.Impulse);
        }
    }

    void Move()
    {
        Vector3 velocity = rb.linearVelocity;

        if (isCrouched && (rb.linearVelocity.x > 1 || rb.linearVelocity.x < -1))
        {
            UnCrouch();
        }

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

        // A or B (B.1 or B.2) //

        // A
        if (playerCombat.isParrying || playerCombat.isBlocking)
        {
            rb.linearVelocity = new Vector3(PlayerCombat.PARRY_OR_BLOCK_SPEED * moveActionValue, velocity.y, velocity.z);
            return;
        }

        // B.1
        if (playerCombat.isLightAttacking)
        {
            rb.linearVelocity = new Vector3(PlayerCombat.LIGHT_ATTACK_SPEED * moveActionValue, velocity.y, velocity.z);
            return;
        }
        // B.2
        else if (playerCombat.isHeavyAttacking)
        {
            rb.linearVelocity = new Vector3(PlayerCombat.HEAVY_ATTACK_SPEED * moveActionValue, velocity.y, velocity.z);
            return;
        }
        else
        {
            rb.linearVelocity = new Vector3(WALK_SPEED * moveActionValue, velocity.y, velocity.z);
            return;
        }
    }

    void Crouch()
    {
        isCrouched = true;

        coll.size = new Vector3(coll.size.x, CROUCH_SIZE, coll.size.z);
        coll.center = new Vector3(coll.center.x, CROUCH_CENTER, coll.center.z);
    }
    void UnCrouch()
    {
        isCrouched = false;

        coll.size = new Vector3(coll.size.x, UNCROUCH_SIZE, coll.size.z);
        coll.center = new Vector3(coll.center.x, UNCROUCH_CENTER, coll.center.z);
    }

    IEnumerator Roll(float direction)
    {
        isRolling = true;

        if (direction == 1)
        {
            anim.SetTrigger("F_Roll");
        }
        else
        {
            anim.SetTrigger("B_Roll");
        }

        float time = 0;
        float duration = 0.8f;

        coll.size = new Vector3(coll.size.x, CROUCH_SIZE, coll.size.z);
        coll.center = new Vector3(coll.center.x, CROUCH_CENTER, coll.center.z);

        meshBone.position += new Vector3(0, 0.9f, 0);
        rb.linearDamping = 0;
        while (time < duration)
        {
            rb.AddForce(new Vector3(1200 * direction, 0, 0), ForceMode.Force);
            yield return new WaitForSeconds(0.01f);
            time += 0.01f;
        }
        coll.size = new Vector3(coll.size.x, UNCROUCH_SIZE, coll.size.z);
        coll.center = new Vector3(coll.center.x, UNCROUCH_CENTER, coll.center.z);

        meshBone.position -= new Vector3(0, 0.9f, 0);
        rb.linearDamping = 0.6f;
        rb.linearVelocity = Vector3.zero;

        isRolling = false;
    }

}
