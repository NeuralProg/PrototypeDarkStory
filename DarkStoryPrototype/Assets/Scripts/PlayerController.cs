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
        }
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

    private void MoveHorizontaly()
    {
        rb.velocity = new Vector2(inputDirection.x * moveSpeed, rb.velocity.y);


    }

    #endregion
}
