using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Variables

    // Instance
    public static PlayerController instance;

    // Inputs
    private InputManager controls = null;
    [HideInInspector] Vector2 inputDirection;

    // Move
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public float moveSpeed = 4f;

    [Header("Checks")]
    [SerializeField] private Transform groundPos;
    [SerializeField] private LayerMask groundMask;
    private bool isGrounded;
    private bool isFalling;

    // Jump
    private bool isJumping = false;
    private float jumpHeight = 15f;
    private float jumpCoyoteTime = 0.1f;
    private float jumpCoyoteTimer;
    private bool shouldPlayLandEffect = false;

    // Dash
    private float dashDuration = 0.1f;
    private float dashSpeedMultiplier = 3f;
    private float dashCooldown = 0.5f;
    private bool canDash;
    private bool inDashCooldown = false;
    private bool isDashing = false;

    [Header("Attacks")]
    [SerializeField] private LayerMask attackables;
    [SerializeField] private Transform slash1Pos;
    [SerializeField] private Vector2 slash1Size;
    [SerializeField] private Transform slash2Pos;
    [SerializeField] private Vector2 slash2Size;
    [SerializeField] private Transform spinAttackPos;
    [SerializeField] private Vector2 spinAttackSize;
    [SerializeField] private Transform slamAttackPos;
    [SerializeField] private Vector2 slamAttackSize;
    [SerializeField] private Transform fallAttackPos;
    [SerializeField] private Vector2 fallAttackSize;
    private float comboTimeAuthorized = 0.3f;
    private float comboTimer = -1f;
    private int comboState = 0;
    private bool isAttacking = false;
    private bool inAttackCooldown = false;
    private bool isInPogo = false;
    private float pogoHeight = 8f;
    private float pogoDuration = 0.1f;

    // Components reference
    private Rigidbody2D rb;
    private Animator anim;

    #endregion


    #region Basics

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        controls = new InputManager();
        controls.Player.Direction.performed += ctx => inputDirection = ctx.ReadValue<Vector2>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (canMove)
        {
            MoveHorizontaly();
            Jump();
            Dash();
            HandleAttacking();
            Pogo();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        Checks();

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -30, 30));

        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetBool("Grounded", isGrounded);
        anim.SetBool("Falling", isFalling);
        anim.SetBool("Jumping", isJumping);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    #endregion


    #region Functions

    private void Checks()
    {
        isGrounded = Physics2D.OverlapCircle(groundPos.position, 0.15f, groundMask);
        isFalling = rb.velocity.y < -0.1f;

        if (isGrounded)
        {
            jumpCoyoteTimer = jumpCoyoteTime;
            canDash = true;

            if (shouldPlayLandEffect)
            {
                shouldPlayLandEffect = false;
                anim.SetTrigger("LandAnim");
                StartCoroutine(CannotMoveCooldown(0.2f / 0.6f));
            }
        }
        else
        {
            jumpCoyoteTimer -= Time.deltaTime;
        }
    }

    private IEnumerator CannotMoveCooldown(float timeToWait)
    {
        canMove = false;
        yield return new WaitForSeconds(timeToWait);
        canMove = true;
    }

    private void MoveHorizontaly()
    {
        if (!isDashing)
        {
            if(inputDirection.x != 0f)
                rb.velocity = new Vector2((inputDirection.x / Mathf.Abs(inputDirection.x)) * moveSpeed, rb.velocity.y);
            else
                rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        if(rb.velocity.x < -0.1f && (!isAttacking || isInPogo))
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if(rb.velocity.x > 0.1f && (!isAttacking || isInPogo))
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void Jump()
    {
        bool canJump = (jumpCoyoteTimer >= 0) && !isJumping;

        if (isFalling && rb.velocity.y < -27f)
            shouldPlayLandEffect = true;

        if (canJump && controls.Player.Jump.WasPressedThisFrame())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
            isJumping = true;
        }

        if (isFalling)
            isJumping = false;

        if (isJumping && controls.Player.Jump.WasReleasedThisFrame())
        {
            isJumping = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 3f);
        }
    }

    private void Dash()
    {
        bool dashCondition = !isDashing && !inDashCooldown;

        if (controls.Player.Dash.WasPressedThisFrame() && canDash && dashCondition)
        {
            StartCoroutine(DashCooldown());
        }

        if (isDashing)
        {
            rb.velocity = new Vector2(transform.localScale.x * moveSpeed * dashSpeedMultiplier, 0);
        }
    }
    private IEnumerator DashCooldown()
    {
        anim.SetTrigger("Dash");
        canDash = false;
        isDashing = true;
        inDashCooldown = true;
        rb.gravityScale = 0f;
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        rb.gravityScale = 5f;
        yield return new WaitForSeconds(dashCooldown);
        inDashCooldown = false;
    }

    private void HandleAttacking()
    {
        bool attackAvailable = !isAttacking && !inAttackCooldown;

        if(comboTimer > -1f)
        {
            comboTimer -= Time.deltaTime;
        }

        if(!isAttacking && comboTimer <= 0)
        {
            comboState = 0;
        }

        if(attackAvailable && controls.Player.Attack.WasPressedThisFrame())
        {
            if (isGrounded)
            {
                if (inputDirection.y >= 0.2f)
                {
                    SlamAttack(false);
                }
                else
                {
                    if(comboTimer > 0)
                    {
                        if (comboState == 1)
                            Slash2();
                        else if (comboState == 2)
                            SpinAttack();
                        else if (comboState == 3)
                        {
                            SlamAttack();
                            comboState = 0;
                        }
                    }
                    else
                    {
                        Slash1();
                    }
                }
            }
            else
            {
                if (inputDirection.y <= -0.2f)
                {
                    FallAttack();
                }
                else
                {
                    SpinAttack(false);
                }
            }
        }
    }
    private void Slash1(bool combo = true)
    {
        anim.SetTrigger("Slash1");
        if(combo)
            StartCoroutine(PerformAttack(slash1Pos, slash1Size, 0.2f / 0.6f, 0, true, comboTimeAuthorized));
        else
            StartCoroutine(PerformAttack(slash1Pos, slash1Size, 0.2f / 0.6f, 0.1f));
    }
    private void Slash2(bool combo = true)
    {
        anim.SetTrigger("Slash2");
        if (combo)
            StartCoroutine(PerformAttack(slash2Pos, slash2Size, 0.15f / 0.6f, 0, true, comboTimeAuthorized));
        else
            StartCoroutine(PerformAttack(slash2Pos, slash2Size, 0.15f / 0.6f, 0.1f));
    }
    private void SpinAttack(bool combo = true)
    {
        anim.SetTrigger("SpinAttack");
        if (combo)
            StartCoroutine(PerformAttack(spinAttackPos, spinAttackSize, 0.25f / 0.6f, 0, true, comboTimeAuthorized));
        else
            StartCoroutine(PerformAttack(spinAttackPos, spinAttackSize, 0.25f / 0.6f, 0.1f));
    }
    private void SlamAttack(bool combo = true)
    {
        anim.SetTrigger("SlamAttack");
        if (combo)
            StartCoroutine(PerformAttack(slamAttackPos, slamAttackSize, 0.14f / 0.6f, 0.75f, true, comboTimeAuthorized));
        else
            StartCoroutine(PerformAttack(slamAttackPos, slamAttackSize, 0.14f / 0.6f, 0.5f));
    }
    private void FallAttack()
    {
        anim.SetTrigger("FallAttack");
        StartCoroutine(PerformAttack(fallAttackPos, fallAttackSize, 0.2f / 0.6f, 0f));
    }
    private IEnumerator PerformAttack(Transform attackCollisionPosition, Vector2 attackCollisionSize, float attackDuration, float attackCooldown, bool shouldTriggerCombo = false, float comboTime = 0f)
    {
        isAttacking = true;
        inAttackCooldown = false;

        var collisionDetected = Physics2D.OverlapBoxAll(attackCollisionPosition.position, attackCollisionSize, 0, attackables);
        foreach(Collider2D attackable in collisionDetected)
        {
            if(attackCollisionPosition == fallAttackPos && !isInPogo)
            {
                StartCoroutine(PogoTime());
            }
        }

        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        inAttackCooldown = true;

        if (shouldTriggerCombo)
        {
            comboTimer = comboTime;
            comboState += 1;
        }

        yield return new WaitForSeconds(attackCooldown);
        inAttackCooldown = false;
    }

    private void Pogo()
    {
        if (isInPogo)
            rb.velocity = new Vector2(rb.velocity.x, pogoHeight);
    }
    private IEnumerator PogoTime()
    {
        isInPogo = true;
        canDash = true;
        yield return new WaitForSeconds(pogoDuration);
        isInPogo = false;
    }

    #endregion
}
