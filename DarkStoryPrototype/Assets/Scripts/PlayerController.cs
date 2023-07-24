using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
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
    private float jumpHeight = 17f;
    private float jumpCoyoteTime = 0.1f;
    private float jumpCoyoteTimer;
    private bool shouldPlayLandEffect = false;
    private float jumpBufferingTime = 0.05f;
    private float jumpBufferingTimer;

    [Header("CornerCorrection")]
    [SerializeField] private float topRaycastLength;
    [SerializeField] private Vector3 edgeRaycast0ffset;
    [SerializeField] private Vector3 innerRaycastOffset;
    private bool canCornerCorrect;

    // Dash
    private float dashDuration = 0.2f;
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
    [SerializeField] private Transform pogoPoint;
    [SerializeField] private GameObject pogoSlashEffect;
    private GameObject activePogoSlashEffect;
    private float comboTimeAuthorized = 0.5f;
    private float comboTimer = -1f;
    private int comboState = 0;
    private bool isAttacking = false;
    private bool inAttackCooldown = false;
    private bool isInPogo = false;
    private float pogoHeight = 6f;
    private float pogoDuration = 0.15f;
    private float slowOnAttackTime = 0.1f;
    private float slowOnAttackTimer;
    private bool launchedAttack = false;

    [Header("LedgeInteraction")]
    [SerializeField] private Transform ledgeDetectionInTop;
    [SerializeField] private Transform ledgeDetectionInBot;
    [SerializeField] private Vector3 ledgeClimbErrorOffset;
    [SerializeField] private Transform ledgeDetectionOut;
    [SerializeField] private LayerMask ledgeLayers;
    [SerializeField] private Transform ledgeClimbingPoint;
    private bool isOnLedge;
    private bool isLedgeClimbing;
    private float ledgeGrabCooldownTime = 0.2f;
    private float ledgeGrabCooldownTimer;

    [Header("WallInteraction")]
    [SerializeField] private Transform wallCheckFront;
    [SerializeField] private Transform wallCheckBack;
    [SerializeField] private LayerMask wallLayer;
    private bool isWalled;
    private bool hasWallBehind;
    private bool wallJumping;
    private float wallJumpDelayTime = 0.2f;
    private float wallJumpDelayTimer;
    private float wallJumpDuration = 0.1f;
    private float wallJumpForce = 7f;

    // Parry
    private bool isParrying = false;
    private float parryDuration = 0.15f / 0.6f;
    private float parryCooldown = 0.5f;
    private bool inParryCooldown = false;

    [Header("ComponentsReference")]
    [SerializeField] private GameObject defaultSprite;
    [SerializeField] private GameObject ledgeSprite;
    private Vector3 defaultLedgeSpritePosition;
    private Rigidbody2D rb;
    private Animator anim;

    #endregion


    #region Basics

    private void Awake()
    {
        //Time.timeScale = 1f; // localScale also affects the IEnumerators

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

        defaultLedgeSpritePosition = ledgeSprite.transform.localPosition;
    }

    void Update()
    {
        if (canMove)
        {
            if (!isLedgeClimbing)
            {
                MoveHorizontaly();
                Jump();
                Dash();
                HandleAttacking();
                Pogo();
                HandleWallInteraction();
                HandleParry();
            }
            HandleLedgeInteraction();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        Checks();

        if(activePogoSlashEffect != null)
        {
            activePogoSlashEffect.transform.position = pogoPoint.position;
            activePogoSlashEffect.transform.localScale = transform.localScale;
        }

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -20, 20));    // Limit the max Y speed
        anim = GetComponentInChildren<Animator>();

        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetBool("Grounded", isGrounded);
        anim.SetBool("Falling", isFalling);
        anim.SetBool("WallSliding", isWalled);
        anim.SetBool("Jumping", (isJumping || wallJumping));
    }

    private void FixedUpdate()
    {
        if (canCornerCorrect)
            CornerCorrect(rb.velocity.y);

        canCornerCorrect = (Physics2D.Raycast(transform.position + edgeRaycast0ffset, Vector2.up, topRaycastLength, groundMask) &&
                           !Physics2D.Raycast(transform.position + innerRaycastOffset, Vector2.up, topRaycastLength, groundMask) ||
                           Physics2D.Raycast(transform.position - edgeRaycast0ffset, Vector2.up, topRaycastLength, groundMask) &&
                           !Physics2D.Raycast(transform.position - innerRaycastOffset, Vector2.up, topRaycastLength, groundMask)) && (isJumping || wallJumping);
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
        isGrounded = Physics2D.OverlapBox(groundPos.position, new Vector2(0.4f, 0.25f), 0f, groundMask);
        isFalling = rb.velocity.y < 0 && !isGrounded;

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
            inDashCooldown = false;
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
        if (!isDashing && !wallJumping && !isParrying)
        {
            if(inputDirection.x != 0f)
                rb.velocity = new Vector2((inputDirection.x / Mathf.Abs(inputDirection.x)) * moveSpeed, rb.velocity.y);
            else
                rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        if(rb.velocity.x < -0.1f && !isAttacking && !isInPogo)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if(rb.velocity.x > 0.1f && !isAttacking && !isInPogo)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void Jump()
    {
        bool canJump = (jumpCoyoteTimer >= 0) && !isJumping && !isParrying;

        if (isFalling && rb.velocity.y < -27f)
            shouldPlayLandEffect = true;

        if (isFalling)
            isJumping = false;

        if (isJumping && controls.Player.Jump.WasReleasedThisFrame())
        {
            isJumping = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 3f);
        }

        if (controls.Player.Jump.WasPressedThisFrame() && isFalling && !isWalled)
        {
            jumpBufferingTimer = jumpBufferingTime;
        }
        if (jumpBufferingTimer >= 0)
        {
            jumpBufferingTimer -= Time.deltaTime;
        }

        if (controls.Player.Jump.WasPressedThisFrame() && !wallJumping && isWalled)
        {
            StartCoroutine(WallJump(true));
        }
        else if(controls.Player.Jump.WasPressedThisFrame() && !wallJumping && hasWallBehind && wallJumpDelayTimer > 0)
        {
            StartCoroutine(WallJump(false));
        }
        else 
        {
            if (canJump && (controls.Player.Jump.WasPressedThisFrame() || jumpBufferingTimer > 0))
            {
                jumpBufferingTimer = 0;
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
                isJumping = true;
            }
        }
    }
    private void CornerCorrect(float yVelocity)
    {
        // Push to the right
        RaycastHit2D hit = Physics2D.Raycast(transform.position - innerRaycastOffset + Vector3.up * topRaycastLength, Vector3.left, topRaycastLength, groundMask);
        if(hit.collider != null)
        {
            float newPos = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * topRaycastLength, transform.position - edgeRaycast0ffset + Vector3.up * topRaycastLength);
            transform.position = new Vector3(transform.position.x + newPos, transform.position.y, transform.position.z);
            rb.velocity = new Vector2(rb.velocity.x, yVelocity);
            return;
        }

        // Push to the left
        hit = Physics2D.Raycast(transform.position + innerRaycastOffset + Vector3.up * topRaycastLength, Vector3.right, topRaycastLength, groundMask);
        if (hit.collider != null)
        {
            float newPos = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * topRaycastLength, transform.position + edgeRaycast0ffset + Vector3.up * topRaycastLength);
            transform.position = new Vector3(transform.position.x - newPos, transform.position.y, transform.position.z);
            rb.velocity = new Vector2(rb.velocity.x, yVelocity);
            return;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position + edgeRaycast0ffset, transform.position + edgeRaycast0ffset + Vector3.up * topRaycastLength);
        Gizmos.DrawLine(transform.position - edgeRaycast0ffset, transform.position - edgeRaycast0ffset + Vector3.up * topRaycastLength);
        Gizmos.DrawLine(transform.position + innerRaycastOffset, transform.position + innerRaycastOffset + Vector3.up * topRaycastLength);
        Gizmos.DrawLine(transform.position - innerRaycastOffset, transform.position - innerRaycastOffset + Vector3.up * topRaycastLength);

        Gizmos.DrawLine(transform.position - innerRaycastOffset + Vector3.up * topRaycastLength, transform.position - innerRaycastOffset + Vector3.up * topRaycastLength + Vector3.left * topRaycastLength);
        Gizmos.DrawLine(transform.position + innerRaycastOffset + Vector3.up * topRaycastLength, transform.position + innerRaycastOffset + Vector3.up * topRaycastLength + Vector3.right * topRaycastLength);
    }

    private void Dash()
    {
        bool dashCondition = !isDashing && !inDashCooldown && !isAttacking && !isOnLedge && !isWalled && !isParrying;

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
        if (launchedAttack && isGrounded)      // Slow the character whenever he attacks
        {
            moveSpeed = 1f;
            slowOnAttackTimer = slowOnAttackTime;
        }
        else
        {
            if (slowOnAttackTimer <= 0 || !isGrounded)
            {
                moveSpeed = 4f;
                slowOnAttackTimer = 0;
                launchedAttack = false;
            }
            else
                slowOnAttackTimer -= Time.deltaTime;
        }

        if (comboTimer > -1f)
        {
            comboTimer -= Time.deltaTime;
        }
        if(!isAttacking && comboTimer <= 0)
        {
            comboState = 0;
        }

        bool attackAvailable = !isAttacking && !inAttackCooldown && !isDashing && !isOnLedge && !isWalled && !isParrying;

        if (attackAvailable && controls.Player.Attack.WasPressedThisFrame())
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
                            Slash1();
                        else if (comboState == 2)
                            SlamAttack();
                        else if (comboState == 3)
                        {
                            SpinAttack();
                            comboState = 0;
                        }
                    }
                    else
                    {
                        Slash2();
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
            StartCoroutine(PerformAttack(slash1Pos, slash1Size, 0.25f / 0.6f, 0, true, comboTimeAuthorized));
        else
            StartCoroutine(PerformAttack(slash1Pos, slash1Size, 0.25f / 0.6f, 0.1f));
    }
    private void Slash2(bool combo = true)
    {
        anim.SetTrigger("Slash2");
        if (combo)
            StartCoroutine(PerformAttack(slash2Pos, slash2Size, 0.19f / 0.6f, 0, true, comboTimeAuthorized));
        else
            StartCoroutine(PerformAttack(slash2Pos, slash2Size, 0.18f / 0.6f, 0.1f));
    }
    private void SpinAttack(bool combo = true)
    {
        anim.SetTrigger("SpinAttack");
        if (combo)
            StartCoroutine(PerformAttack(spinAttackPos, spinAttackSize, 0.22f / 0.6f, 0.5f, true, comboTimeAuthorized));
        else
            StartCoroutine(PerformAttack(spinAttackPos, spinAttackSize, 0.22f / 0.6f, 0.1f));
    }
    private void SlamAttack(bool combo = true)
    {
        anim.SetTrigger("SlamAttack");
        if (combo)
            StartCoroutine(PerformAttack(slamAttackPos, slamAttackSize, 0.27f / 0.6f, 0, true, comboTimeAuthorized));
        else
            StartCoroutine(PerformAttack(slamAttackPos, slamAttackSize, 0.27f / 0.6f, 0.3f));
    }
    private void FallAttack()
    {
        anim.SetTrigger("FallAttack");

        if(pogoSlashEffect != null) 
        {
            activePogoSlashEffect = Instantiate(pogoSlashEffect, pogoPoint.position, pogoPoint.rotation);
            activePogoSlashEffect.GetComponentInChildren<Animator>().SetTrigger("Appear");
            Destroy(activePogoSlashEffect, 0.15f/0.6f);
        }

        StartCoroutine(PerformAttack(fallAttackPos, fallAttackSize, 0.20f / 0.6f, 0f));
    }
    private IEnumerator PerformAttack(Transform attackCollisionPosition, Vector2 attackCollisionSize, float attackDuration, float attackCooldown, bool shouldTriggerCombo = false, float comboTime = 0f)
    {
        launchedAttack = true;
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
        launchedAttack = false;

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

    private void HandleLedgeInteraction()
    {
        if(ledgeGrabCooldownTimer >= 0)
            ledgeGrabCooldownTimer -= Time.deltaTime;

        bool ledgeInCollision = Physics2D.OverlapBox(ledgeDetectionInBot.position, new Vector2(0.08f, 0.15f), 0, ledgeLayers) || Physics2D.OverlapBox(ledgeDetectionInTop.position, new Vector2(0.08f, 0.15f), 0, ledgeLayers);
        bool ledgeOutNotCollision = !Physics2D.OverlapBox(ledgeDetectionOut.position, new Vector2(0.08f, 0.2f), 0, ledgeLayers);

        if(Physics2D.OverlapBox(ledgeDetectionInBot.position, new Vector2(0.08f, 0.15f), 0, ledgeLayers) && Physics2D.OverlapBox(ledgeDetectionInTop.position, new Vector2(0.08f, 0.15f), 0, ledgeLayers))
        {
            ledgeSprite.transform.localPosition = defaultLedgeSpritePosition + ledgeClimbErrorOffset;
        }
        if(!isOnLedge && !isLedgeClimbing)
        {
            ledgeSprite.transform.localPosition = defaultLedgeSpritePosition;
        }

        if (ledgeGrabCooldownTimer < 0 && ledgeInCollision && ledgeOutNotCollision && (isFalling || isOnLedge))
        {
            isOnLedge = true;
        }
        else
        {
            isOnLedge = false;
        }

        if (isOnLedge)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.gravityScale = 0f;
            ledgeSprite.SetActive(true);
            defaultSprite.SetActive(false);
        }
        else
        {
            if (!isDashing && !isLedgeClimbing)
                rb.gravityScale = 5f;

            ledgeSprite.SetActive(false);
            defaultSprite.SetActive(true);
        }

        if (isOnLedge && !isLedgeClimbing && (inputDirection.y >= 0.2f || controls.Player.Jump.WasPressedThisFrame()))
        {
            StartCoroutine(LedgeClimb());
        }
        else if (inputDirection.y <= -0.9f && isOnLedge && !isLedgeClimbing)
        {
            isOnLedge = false;
            ledgeGrabCooldownTimer = ledgeGrabCooldownTime;
        }

        anim.SetBool("OnLedge", isOnLedge);
    }

    private IEnumerator LedgeClimb()
    {
        ledgeSprite.SetActive(true);
        defaultSprite.SetActive(false);
        isLedgeClimbing = true;
        anim = GetComponentInChildren<Animator>();
        anim.SetTrigger("LedgeClimb");

        yield return new WaitForSeconds(0.5f);

        ledgeSprite.SetActive(false);
        defaultSprite.SetActive(true);
        isLedgeClimbing = false;
        transform.position = ledgeClimbingPoint.position;
    }

    private void HandleWallInteraction()
    {
        bool isGoingToWall = (transform.localScale.x < 0 && inputDirection.x < -0.2f) || (transform.localScale.x > 0 && inputDirection.x > 0.2f);
        isWalled = Physics2D.OverlapBox(wallCheckFront.position, new Vector2(0.25f, 0.5f), 0, wallLayer) && !isGrounded && (isGoingToWall || isWalled) && !isOnLedge && !isLedgeClimbing;
        hasWallBehind = Physics2D.OverlapBox(wallCheckBack.position, new Vector2(0.5f, 0.5f), 0, wallLayer) && !isGrounded;

        if (wallJumpDelayTimer >= 0)
            wallJumpDelayTimer -= Time.deltaTime;

        if (isWalled)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -2, 20));
            wallJumpDelayTimer = wallJumpDelayTime;
            canDash = true;
            shouldPlayLandEffect = false;
        }

        if(wallJumping && !controls.Player.Jump.IsPressed())
        {
            rb.velocity = new Vector2(wallJumpForce * transform.localScale.x, rb.velocity.y / 2);
            wallJumping = false;
        }
        else if (wallJumping)
        {
            rb.velocity = new Vector2(wallJumpForce * transform.localScale.x, wallJumpForce * 1.5f);
        }
    }
    private IEnumerator WallJump(bool shouldFlip = false)
    {
        wallJumping = true;

        if (shouldFlip)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        yield return new WaitForSeconds(wallJumpDuration);

        wallJumping = false;
    }

    private void HandleParry()
    {
        bool canParry = !isParrying && !inParryCooldown && !isWalled && !isOnLedge && !isDashing && !isAttacking && !wallJumping && controls.Player.Parry.WasPressedThisFrame();

        if (canParry)
        {
            StartCoroutine(ParryTimer());
        }

        if((isOnLedge || isLedgeClimbing))
        {
            isParrying = false;
        }
    }
    private IEnumerator ParryTimer()
    {
        anim.SetTrigger("Parry");
        isParrying = true;
        inParryCooldown = false;

        yield return new WaitForSeconds(parryDuration);

        isParrying = false;
        inParryCooldown = true;
        StartCoroutine(SucessfullParry());

        yield return new WaitForSeconds(parryCooldown);

        inParryCooldown = false;
    }
    private IEnumerator SucessfullParry()
    {
        anim.SetTrigger("SuccessfullParry");
        isParrying = true;
        inParryCooldown = false;
        rb.velocity = Vector2.zero;
        Time.timeScale = 0.5f;

        yield return new WaitForSeconds((0.15f / 0.6f) / 2);

        Time.timeScale = 1f;

        yield return new WaitForSeconds(0.22f / 0.6f);

        if(!isOnLedge && !isLedgeClimbing)
            SpinAttack(false);

        isParrying = false;
        inParryCooldown = true;

        yield return new WaitForSeconds(parryCooldown);

        inParryCooldown = false;
    }

    #endregion
}
