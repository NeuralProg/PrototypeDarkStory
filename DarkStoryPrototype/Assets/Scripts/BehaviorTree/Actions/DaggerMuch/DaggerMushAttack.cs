using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Pathfinding;
using System.Numerics;
using UnityEngine.UIElements;
using Unity.VisualScripting;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class DaggerMushAttack : Action
    {
        #region Variables

        [Header("Attack infos")]
        [SerializeField] private int attackType = 1; // 1 = normal, 2 = combo, 3 = air attack
        [SerializeField] private Transform normalAttackPos;
        [SerializeField] private UnityEngine.Vector2 normalAttackSize;

        [SerializeField] private Transform comboAttackPos1;
        [SerializeField] private UnityEngine.Vector2 comboAttackSize1;

        [SerializeField] private Transform comboAttackPos2;
        [SerializeField] private UnityEngine.Vector2 comboAttackSize2;

        [SerializeField] private Transform canComboAttackPos;
        [SerializeField] private UnityEngine.Vector2 canComboAttackSize;

        [SerializeField] private Transform airAttackPos;
        [SerializeField] private UnityEngine.Vector2 airAttackSize;

        [SerializeField] private Transform canAirAttackPos;
        [SerializeField] private UnityEngine.Vector2 canAirAttackSize;

        [SerializeField] private int normalAttackDamages = 1;
        [SerializeField] private int comboAttackDamages = 1;
        [SerializeField] private int airAttackDamages = 1;
        [SerializeField] private LayerMask cancelSpecialAttacksLayers;
        private bool hasBeenHit = false;
        private bool hasHitPlayer;
        private bool finished;

        [Header("References")]
        [SerializeField] private Animator anim;
        [SerializeField] private SpriteRenderer sr;
        private Rigidbody2D rb;
        private UnityEngine.Vector3 initialLocalSpritePos;
        private UnityEngine.Vector2 initialLocalCollisionOffset;

        #endregion


        #region Basics

        public override void OnStart()
        {
            hasBeenHit = false;
            finished = false;
            hasHitPlayer = false;

            rb = GetComponent<Rigidbody2D>();
            initialLocalSpritePos = sr.gameObject.transform.localPosition;
            initialLocalCollisionOffset = GetComponent<CapsuleCollider2D>().offset; // change the x offset of the collision when the enemy move during the anim

            if (!GetComponent<Enemy>().isKnockbacking)
            {
                if (attackType == 1)
                    StartCoroutine(NormalAttack());
                else if (!Physics2D.OverlapBox(canComboAttackPos.position, canComboAttackSize, 0, cancelSpecialAttacksLayers) && attackType == 2)
                    StartCoroutine(ComboAttack());
                else if (!Physics2D.OverlapBox(canAirAttackPos.position, canAirAttackSize, 0, cancelSpecialAttacksLayers) && attackType == 3)
                    StartCoroutine(AirAttack());
                else
                    finished = true;
            }
            else
                finished = true;
        }

        public override TaskStatus OnUpdate()
        {
            bool cancelChance = Random.Range(0, 6) == 0;
            if (GetComponent<Enemy>().isKnockbacking)
            {
                if (cancelChance && !hasBeenHit)
                {
                    Debug.Log("canceled");
                    anim.SetTrigger("ReturnToIdle");
                    sr.gameObject.transform.localPosition = initialLocalSpritePos;
                    GetComponent<CapsuleCollider2D>().offset = initialLocalCollisionOffset;
                    StopAllCoroutines();
                    finished = false;
                    return TaskStatus.Success;
                }
                hasBeenHit = true;
            }

            if (finished)
            {
                sr.gameObject.transform.localPosition = initialLocalSpritePos;
                GetComponent<CapsuleCollider2D>().offset = initialLocalCollisionOffset;
                finished = false;
                return TaskStatus.Success;
            }
            else
                return TaskStatus.Running;
        }

        #endregion


        #region Functions

        private IEnumerator NormalAttack()
        {
            hasHitPlayer = false;
            rb.velocity = new UnityEngine.Vector2(0f, rb.velocity.y);
            anim.SetTrigger("Attack");
            
            yield return new WaitForSeconds(0.25f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.06f / 0.6f);


            anim.SetTrigger("ReturnToIdle");
            sr.gameObject.transform.localPosition = initialLocalSpritePos;
            finished = true;
        }

        private IEnumerator ComboAttack()
        {
            hasHitPlayer = false;

            rb.velocity = new UnityEngine.Vector2(0f, rb.velocity.y); 
            anim.SetTrigger("AttackCombo");

            yield return new WaitForSeconds(0.11f / 0.6f);

            hasHitPlayer = false;
            gameObject.layer = LayerMask.NameToLayer("EnemyNoColWithPlayer");
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();

            gameObject.layer = LayerMask.NameToLayer("Enemy");
            GetComponent<CapsuleCollider2D>().offset = new UnityEngine.Vector2(1.72f, 0f);

            yield return new WaitForSeconds(0.14f / 0.6f);

            hasHitPlayer = false;
            gameObject.layer = LayerMask.NameToLayer("EnemyNoColWithPlayer");
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage(false);
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage(false);
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage(false);
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage(false);
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage(false);

            gameObject.layer = LayerMask.NameToLayer("Enemy");
            GetComponent<CapsuleCollider2D>().offset = new UnityEngine.Vector2(2.25f, 0f);

            yield return new WaitForSeconds(0.05f / 0.6f);

            anim.SetTrigger("ReturnToIdle");
            gameObject.transform.position = new UnityEngine.Vector3(transform.position.x + (transform.localScale.x * 2.26f), transform.position.y, transform.position.z);
            GetComponent<CapsuleCollider2D>().offset = initialLocalCollisionOffset;
            finished = true;
        }

        private IEnumerator AirAttack()
        {
            hasHitPlayer = false;

            rb.velocity = new UnityEngine.Vector2(0f, rb.velocity.y);
            anim.SetTrigger("AirAttack");

            yield return new WaitForSeconds(0.14f / 0.6f);

            gameObject.layer = LayerMask.NameToLayer("EnemyNoColWithPlayer");

            yield return new WaitForSeconds(0.35f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.01f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.07f / 0.6f);

            gameObject.layer = LayerMask.NameToLayer("Enemy");
            anim.SetTrigger("ReturnToIdle");
            GetComponent<CapsuleCollider2D>().offset = initialLocalCollisionOffset;
            finished = true;
        }

        private void DealDamage(bool comboFirstAttack = true)
        {
            if (attackType == 1)
            {
                var collisionDetected = Physics2D.OverlapBoxAll(normalAttackPos.position, normalAttackSize, 0);
                foreach (Collider2D attackable in collisionDetected)
                {
                    if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                    {
                        GetComponent<Enemy>().DealDamageToPlayer(normalAttackDamages);
                        hasHitPlayer = true;
                    }
                }
            }
            else if (attackType == 2)
            {
                if (comboFirstAttack)
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(comboAttackPos1.position, comboAttackSize1, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(comboAttackDamages);
                            hasHitPlayer = true;
                        }
                    }
                }
                else
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(comboAttackPos2.position, comboAttackSize2, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(comboAttackDamages);
                            hasHitPlayer = true;
                        }
                    }
                }
            }
            else if (attackType == 3)
            {
                var collisionDetected = Physics2D.OverlapBoxAll(airAttackPos.position, airAttackSize, 0);
                foreach (Collider2D attackable in collisionDetected)
                {
                    if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                    {
                        GetComponent<Enemy>().DealDamageToPlayer(airAttackDamages);
                        hasHitPlayer = true;
                    }
                }
            }
        }

        #endregion
    }
}
