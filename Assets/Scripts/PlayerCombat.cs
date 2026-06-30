using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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

    private PlayerCombat opponentPlayerCombat;

    private UIManager uiManager;
    private PlayerMovement playerMovement;

    private int playerNumber;
    private int opponentNumber;

    private GameObject opponentPlayer;

    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;
    private InputAction parryAndBlockAction;

    [SerializeField] private LayerMask opponentMask;

    private readonly static WaitForSeconds attackDelay = new(ATTACK_DELAY);
    private readonly static WaitForSeconds parryTime = new(PARRY_TIME);
    private readonly static WaitForSeconds parryReloadTime = new(PARRY_RELOAD_TIME);

    private Animator anim;

    // EDITABLE //
    public const float WALK_SPEED = 3.5f;
    public const float LIGHT_ATTACK_SPEED = WALK_SPEED / 2;
    public const float HEAVY_ATTACK_SPEED = WALK_SPEED / 4;
    public const float PARRY_OR_BLOCK_SPEED = WALK_SPEED / 2;
    private const float LIGHT_ATTACK_RANGE = 0.8f;
    private const float HEAVY_ATTACK_RANGE = 1.25f;
    private const float LIGHT_ATTACK_DAMAGE = 0.1f;
    private const float HEAVY_ATTACK_DAMAGE = 0.3f;
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
        uiManager = GameObject.Find("UI").GetComponent<UIManager>();
        playerMovement = gameObject.GetComponent<PlayerMovement>();

        opponentPlayerCombat = opponentPlayer.GetComponent<PlayerCombat>();

        lightAttackAction = InputSystem.actions.FindAction("LightAttack");
        heavyAttackAction = InputSystem.actions.FindAction("HeavyAttack");
        parryAndBlockAction = InputSystem.actions.FindAction("Parry/Block");

        anim = gameObject.GetComponent<Animator>();
    }

    // Checks whether the player has pressed an input, then do the thing
    private void Update()
    {
        // TEMPORARY (fix the InputSystem for both players first)
        if (playerNumber == 1 && Time.timeScale != 0)
        {
            bool isDoingNothing = !playerMovement.isRolling && !isLightAttacking && !isHeavyAttacking && !isParrying && !isBlocking;
            bool isGrounded = playerMovement.isGrounded;

            if (lightAttackAction.WasPressedThisFrame() && isDoingNothing)
            {
                StartCoroutine(LightAttack());
            }
            else if (heavyAttackAction.WasPressedThisFrame() && isDoingNothing)
            {
                StartCoroutine(HeavyAttack());
            }
            else if (parryAndBlockAction.WasPressedThisFrame() && isDoingNothing && playerMovement.isGrounded)
            {
                StartCoroutine(ParryAndBlock());
            }
            else if (parryAndBlockAction.WasReleasedThisFrame() && (isParrying || isBlocking) || !playerMovement.isGrounded)
            {
                StartCoroutine(StopParryAndBlock());
            }
        }
    }

    // FUNCTIONS //
    private IEnumerator LightAttack()
    {
        isLightAttacking = true;

        anim.SetTrigger("LightAttack"); // 40%

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        while (!stateInfo.IsName("D_Punch"))
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        while (stateInfo.IsName("D_Punch") && stateInfo.normalizedTime < 0.4f)
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        Vector3 originPos = transform.position + new Vector3(0, 0.5f, 0);
        Vector3 forwardDirection = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(originPos, forwardDirection, LIGHT_ATTACK_RANGE, opponentMask))
        {
            if (opponentPlayerCombat.isParrying)
            {
                Debug.Log("P" + playerNumber + " " + "Light: Parried");
            }
            else if (opponentPlayerCombat.isBlocking)
            {
                Debug.Log("P" + playerNumber + " " + "Light: Blocked");
                // stamina bar logic here
            }
            else
            {
                Debug.Log("P" + playerNumber + " " + "Light: Hit");
                uiManager.TakeDamage(opponentNumber, LIGHT_ATTACK_DAMAGE);
            }
        }
        else
        {
            Debug.Log("P" + playerNumber + " " + "Light: -");
        }

        while (stateInfo.IsName("D_Punch") && stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        isLightAttacking = false;
    }
    private IEnumerator HeavyAttack()
    {
        isHeavyAttacking = true;

        anim.SetTrigger("HeavyAttack"); // 43%

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        while (!stateInfo.IsName("D_Kick"))
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        while (stateInfo.IsName("D_Kick") && stateInfo.normalizedTime < 0.4f)
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        Vector3 originPos = transform.position + new Vector3(0, 0.5f, 0);
        Vector3 forwardDirection = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(originPos, forwardDirection, HEAVY_ATTACK_RANGE, opponentMask))
        {
            if (opponentPlayerCombat.isParrying)
            {
                Debug.Log("P" + playerNumber + " " + "Light: Parried");
            }
            else if (opponentPlayerCombat.isBlocking)
            {
                Debug.Log("P" + playerNumber + " " + "Light: Blocked");
                // stamina bar logic here
            }
            else
            {
                Debug.Log("P" + playerNumber + " " + "Light: Hit");
                uiManager.TakeDamage(opponentNumber, HEAVY_ATTACK_DAMAGE);
            }
        }
        else
        {
            Debug.Log("P" + playerNumber + " " + "Light: -");
        }

        while (stateInfo.IsName("D_Kick") && stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        isHeavyAttacking = false;
    }

    private IEnumerator ParryAndBlock()
    {
        if (canParry)
        {
            anim.SetBool("IsParryingOrBlocking", true);
            Debug.Log("Parry");

            canParry = false;

            isParrying = true;
            isBlocking = false;

            yield return parryTime;
        }

        if (parryAndBlockAction.IsPressed())
        {
            anim.SetBool("IsParryingOrBlocking", true);
            Debug.Log("Block");

            isParrying = false;
            isBlocking = true;
        }
    }
    private IEnumerator StopParryAndBlock()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        while (stateInfo.normalizedTime < 0.9f)
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        anim.SetBool("IsParryingOrBlocking", false);

        while (stateInfo.normalizedTime < 0.46f)
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        isParrying = false;
        isBlocking = false;

        StartCoroutine(ReloadParry());
    }
    private IEnumerator ReloadParry()
    {
        yield return parryReloadTime;

        canParry = true;
    }

}
