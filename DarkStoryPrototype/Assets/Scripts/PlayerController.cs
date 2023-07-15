using System.Collections;
using System.Collections.Generic;
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
    [HideInInspector] public float moveSpeed = 6f;

    [Header("Checks")]
    [SerializeField] private Transform groundPos;
    [SerializeField] private LayerMask groundMask;
    private bool isGrounded;
    private bool isFalling;

    // Jump
    private bool isJumping = false;
    private float jumpHeight = 18f;
    private float jumpCoyoteTime = 0.1f;
    private float jumpCoyoteTimer;

    // Dash
    private float dashDuration = 0.1f;
    private float dashSpeedMultiplier = 3f;
    private float dashCooldown = 0.5f;
    private bool canDash;
    private bool inDashCooldown = false;
    private bool isDashing = false;

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
        }
        else
        {
            jumpCoyoteTimer -= Time.deltaTime;
        }
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

        if(rb.velocity.x < -0.1f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if(rb.velocity.x > 0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void Jump()
    {
        bool canJump = (jumpCoyoteTimer >= 0) && !isJumping;

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
        bool canDash = !isDashing && !inDashCooldown;

        if (controls.Player.Dash.WasPressedThisFrame() && canDash)
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

    #endregion
}
