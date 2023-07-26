using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Variables

    [Header("Health")]
    [SerializeField] private int maxHealth = 4;
    private int currentHealth;
    private bool dead;

    [Header("Damages")]
    [SerializeField] private int damagesOnPlayerTouch = 1;

    // Knockback
    private float knockbackForce = 4f;
    private float knockbackAttackedDuration = 0.2f;
    private bool isKnockbacking;
    private Vector2 knockbackDirection;

    [Header("Effects")]
    [SerializeField] private GameObject damagedParticles;
    [SerializeField] private GameObject deathParticles;
    [SerializeField] private float deathAnimTime;

    [Header("Components refs")]
    public Transform centerPoint;
    private Rigidbody2D rb;
    private Animator anim;

    #endregion


    #region Basics

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
    }

    void Update()
    {
        HandleKnockback();

        if (currentHealth <= 0 && !dead)
        {
            StartCoroutine(Die());
        }

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -20, 20));    // Limit the max Y speed

        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        if(!isKnockbacking)
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    #endregion

    #region Functions

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0 && !dead)
        {
            StartCoroutine(Die());
        }
        else 
        {
            if (damagedParticles != null && !dead)
                Instantiate(damagedParticles, transform.position, transform.rotation);

            anim.SetTrigger("Damage");
        }

        // Apply knockback
        knockbackDirection = new Vector2(centerPoint.position.x - PlayerController.instance.centerPoint.position.x, centerPoint.position.y - PlayerController.instance.centerPoint.position.y).normalized;
        StartCoroutine(KnockbackTime());
    }

    private IEnumerator Die()
    {
        anim.SetTrigger("Dead");
        dead = true;
        gameObject.layer = LayerMask.NameToLayer("NoColPlayerEnemy");

        yield return new WaitForSeconds(deathAnimTime);

        if(deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(LayerMask.LayerToName(collision.gameObject.layer) == "Player")
            PlayerController.instance.TakeDamage(centerPoint.position, damagesOnPlayerTouch);
    }

    private void HandleKnockback()
    {
        if (isKnockbacking)
        {
            rb.velocity = knockbackDirection * knockbackForce;
        }
    }
    private IEnumerator KnockbackTime()
    {
        isKnockbacking = true;
        rb.gravityScale = 0f;

        yield return new WaitForSeconds(knockbackAttackedDuration);

        rb.gravityScale = 5f;
        isKnockbacking = false;
    }

    #endregion
}
