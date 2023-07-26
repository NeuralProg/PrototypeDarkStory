using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    #region Variables
    [Header("Health")]
    [SerializeField] private int maxHealth = 4;
    public int currentHealth;
    private bool dead;

    [Header("Damages")]
    [SerializeField] private int damagesOnPlayerTouch = 1;

    [Header("Effects")]
    [SerializeField] private GameObject damagedParticles;
    [SerializeField] private GameObject deathParticles;
    [SerializeField] private float deathAnimTime;

    // Components refs
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
        if (currentHealth <= 0 && !dead)
        {
            StartCoroutine(Die());
        }

        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
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
        else if(damagedParticles != null && !dead)
        {
            Instantiate(damagedParticles, transform.position, transform.rotation);
        }
    }

    private IEnumerator Die()
    {
        anim.SetTrigger("Dead");
        dead = true;

        yield return new WaitForSeconds(deathAnimTime);

        if(deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    #endregion
}
