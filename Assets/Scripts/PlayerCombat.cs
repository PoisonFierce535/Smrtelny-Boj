using System.Collections;
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

    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;
    private float lightAttackValue;
    private float heavyAttackValue;

    private Rigidbody rigidBody;

    private readonly static WaitForSeconds attackDelay = new(0.25f);

    [SerializeField] private LayerMask layerMask;

    private const float WALK_SPEED = 3.5f;
    private const float LIGHT_ATTACK_SPEED = WALK_SPEED / 1.2f;
    private const float LIGHT_ATTACK_RANGE = 1.4f;
    private const float HEAVY_ATTACK_RANGE = 1.4f;


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
        lightAttackAction = InputSystem.actions.FindAction("LightAttack");
        heavyAttackAction = InputSystem.actions.FindAction("HeavyAttack");

        rigidBody = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (lightAttackAction.WasPressedThisFrame())
        {
            StartCoroutine(LightAttack());
        }
        else if (heavyAttackAction.WasPressedThisFrame())
        {
            StartCoroutine(HeavyAttack());
        }
    }

    IEnumerator LightAttack()
    {
        yield return attackDelay;

        Vector3 originPos = transform.position + new Vector3(0, 0.5f, 0);
        Vector3 forwardDirection = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(originPos, forwardDirection, out RaycastHit hitInfo, LIGHT_ATTACK_RANGE, layerMask))
        {
            Debug.Log("Light: hit");

        }
        else
        {
            Debug.Log("Light: -");

        }
    }
    IEnumerator HeavyAttack()
    {
        yield return attackDelay;

        Vector3 originPos = transform.position + new Vector3(0, 0.5f, 0);
        Vector3 forwardDirection = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(originPos, forwardDirection, out RaycastHit hitInfo, HEAVY_ATTACK_RANGE, layerMask) /* && notBlockingEnemy */)
        {
            Debug.Log("Heavy: hit");
        }
        else
        {
            Debug.Log("Heavy: -");
        }
    }
}
