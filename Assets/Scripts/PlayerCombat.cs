using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{

    // instead of making a "hit", make a "about to get hit", then wait for the other player to repsond
    // whether he want to block or parry or dodge (not sure how would dodge work against this algorithm)
    // and then hit the player after a set amount of time, like 0.25 secs

    //extra (better): wait for exampl. 0.25 secs, then send the raycast and chceck for the player and also check in
    // the meantime whether he was in a "parrying state" or other state blocking or he was not there (so dodging, or simply noit there) 

    public InputActionAsset InputActions;


    private int playerNumber;
    private int opponentNumber;

    private GameObject opponentPlayer;

    private PlayerCombat opponentPlayerCombat;

    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;
    private InputAction parryAndBlockAction;

    [SerializeField] private LayerMask opponentMask;

    private readonly static WaitForSeconds attackDelay = new(ATTACK_DELAY);
    private readonly static WaitForSeconds parryTime = new(PARRY_TIME);
    private readonly static WaitForSeconds parryReloadTime = new(PARRY_RELOAD_TIME);

    // EDITABLE //
    public const float WALK_SPEED = 3.5f;
    public const float LIGHT_ATTACK_SPEED = WALK_SPEED / 2;
    public const float HEAVY_ATTACK_SPEED = WALK_SPEED / 4;
    public const float PARRY_OR_BLOCK_SPEED = WALK_SPEED / 1.5f;
    private const float LIGHT_ATTACK_RANGE = 1.4f;
    private const float HEAVY_ATTACK_RANGE = 1.4f;
    private const float ATTACK_DELAY = 0.2f;
    private const float PARRY_TIME = 0.2f;
    private const float PARRY_RELOAD_TIME = 1f;
    // EDITABLE //

    public bool canParry;
    public bool isParrying;
    public bool isBlocking;
    public bool isLightAttacking;
    public bool isHeavyAttacking;


    // Enables the inputs whenever the player object is active
    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    // Sets all the neccessary variables
    private void Awake()
    {
        if (gameObject.name == "Player1")
        {
            playerNumber = 1;
            opponentNumber = 2;
            opponentPlayer = GameObject.Find("Player2");
        }
        else
        {
            playerNumber = 2;
            opponentNumber = 1;
            opponentPlayer = GameObject.Find("Player1");
        }
        opponentPlayerCombat = opponentPlayer.GetComponent<PlayerCombat>();

        lightAttackAction = InputSystem.actions.FindAction("LightAttack");
        heavyAttackAction = InputSystem.actions.FindAction("HeavyAttack");
        parryAndBlockAction = InputSystem.actions.FindAction("Parry/Block");
    }

    // Checks whether the player has pressed an input, then do the thing
    private void Update()
    {
        // TEMPORARY
        if (opponentNumber == 2)
        {
            if (lightAttackAction.WasPressedThisFrame())
            {
                StartCoroutine(LightAttack());
            }
            else if (heavyAttackAction.WasPressedThisFrame())
            {
                StartCoroutine(HeavyAttack());
            }

            if (parryAndBlockAction.WasPressedThisFrame())
            {
                StartCoroutine(ParryAndBlock());
            }
            else if (parryAndBlockAction.WasReleasedThisFrame())
            {
                StopParryAndBlock();
            }
        }
    }

    // FUNCTIONS //
    private IEnumerator LightAttack()
    {
        isLightAttacking = true;

        yield return attackDelay;

        Vector3 originPos = transform.position + new Vector3(0, 0.5f, 0);
        Vector3 forwardDirection = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(originPos, forwardDirection, LIGHT_ATTACK_RANGE, opponentMask) && opponentPlayerCombat.isParrying)
        {
            //Debug.Log("P" + playerNumber + " " + "Light: Parried");

        }
        else if (Physics.Raycast(originPos, forwardDirection, LIGHT_ATTACK_RANGE, opponentMask) && opponentPlayerCombat.isBlocking)
        {
            //Debug.Log("P" + playerNumber + " " + "Light: Blocked");

        }
        else if (Physics.Raycast(originPos, forwardDirection, LIGHT_ATTACK_RANGE, opponentMask))
        {
            //Debug.Log("P" + playerNumber + " " + "Light: Hit");

        }
        else
        {
            //Debug.Log("P" + playerNumber + " " + "Light: -");

        }

        isLightAttacking = false;
    }
    private IEnumerator HeavyAttack()
    {
        isHeavyAttacking = true;

        yield return attackDelay;

        Vector3 originPos = transform.position + new Vector3(0, 0.5f, 0);
        Vector3 forwardDirection = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(originPos, forwardDirection, HEAVY_ATTACK_RANGE, opponentMask) && opponentPlayerCombat.isParrying)
        {
            //Debug.Log("P" + playerNumber + " " + "Heavy: Parried");

        }
        else if (Physics.Raycast(originPos, forwardDirection, HEAVY_ATTACK_RANGE, opponentMask) && opponentPlayerCombat.isBlocking)
        {
            //Debug.Log("P" + playerNumber + " " + "Heavy: Blocked");

        }
        else if (Physics.Raycast(originPos, forwardDirection, HEAVY_ATTACK_RANGE, opponentMask))
        {
            //Debug.Log("P" + playerNumber + " " + "Heavy: Hit");

        }
        else
        {
            //Debug.Log("P" + playerNumber + " " + "Heavy: -");

        }

        isHeavyAttacking = false;
    }
    
    private IEnumerator ParryAndBlock()
    {
        if (canParry)
        {
            canParry = false;

            Debug.Log("P" + playerNumber + " " + "Parrying");
            isParrying = true;
            isBlocking = false;

            yield return parryTime;
        }

        Debug.Log("P" + playerNumber + " " + "Blocking");
        isParrying = false;
        isBlocking = true;
    }
    private void StopParryAndBlock()
    {
        Debug.Log("P" + playerNumber + " " + "Not Blocking or Parrying");
        isParrying = false;
        isBlocking = false;

        StartCoroutine(ReloadParry());
    }
    private IEnumerator ReloadParry()
    {
        yield return parryReloadTime;

        Debug.LogWarning("P" + playerNumber + " " + "Can Parry");
        canParry = true;
    }
}
