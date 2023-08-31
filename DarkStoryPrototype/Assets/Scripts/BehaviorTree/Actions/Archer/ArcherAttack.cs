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
    public class ArcherAttack : Action
    {
        #region Variables

        [Header("Attack infos")]
        [SerializeField] private bool isSpecialAttack = false;
        [SerializeField] private Transform normalAttackPos;
        [SerializeField] private UnityEngine.Vector2 normalAttackSize;
        [SerializeField] private Transform specialAttackPos;
        [SerializeField] private UnityEngine.Vector2 specialAttackSize;
        [SerializeField] private Transform specialAttackBackCheckBox;
        [SerializeField] private UnityEngine.Vector2 specialAttackBackCheckSize;
        [SerializeField] private LayerMask cancelSpecialAttackLayers;
        [SerializeField] private int normalAttackDamages = 1;
        [SerializeField] private int specialAttackDamages = 2;
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
            finished = false;
            hasHitPlayer = false;

            rb = GetComponent<Rigidbody2D>();
            initialLocalSpritePos = sr.gameObject.transform.localPosition;
            initialLocalCollisionOffset = GetComponent<CapsuleCollider2D>().offset; // change the x offset of the collision when the enemy move during the anim

            if (!GetComponent<Enemy>().isKnockbacking)
            {
                if (!isSpecialAttack)
                    StartCoroutine(NormalAttack());
                else if (!Physics2D.OverlapBox(specialAttackBackCheckBox.position, specialAttackBackCheckSize, 0, cancelSpecialAttackLayers))
                    StartCoroutine(SpecialAttack());
                else
                    finished = true;
            }
            else
                finished = true;
        }

        public override TaskStatus OnUpdate()
        {
            if (GetComponent<Enemy>().isKnockbacking && Random.Range(0, 2) == 0)
            {
                sr.gameObject.transform.localPosition = initialLocalSpritePos;
                GetComponent<CapsuleCollider2D>().offset = initialLocalCollisionOffset;
                StopAllCoroutines();
                finished = false;
                return TaskStatus.Success;
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
            sr.gameObject.transform.localPosition = new UnityEngine.Vector3(sr.gameObject.transform.localPosition.x + 0.4f, sr.gameObject.transform.localPosition.y, sr.gameObject.transform.localPosition.z);
            
            yield return new WaitForSeconds(0.4f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.025f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.025f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.025f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.025f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.18f / 0.6f);

            anim.SetTrigger("ReturnToIdle");
            sr.gameObject.transform.localPosition = initialLocalSpritePos;
            finished = true;
        }
        private IEnumerator SpecialAttack()
        {
            hasHitPlayer = false;

            rb.velocity = new UnityEngine.Vector2(0f, rb.velocity.y); // vanish
            anim.SetTrigger("SpecialAttackVanish");

            yield return new WaitForSeconds(0.18f / 0.6f);

            gameObject.layer = LayerMask.NameToLayer("EnemyNoColWithPlayer");

            yield return new WaitForSeconds(0.24f / 0.6f);

            gameObject.layer = LayerMask.NameToLayer("Enemy"); // Shoot
            gameObject.transform.position = new UnityEngine.Vector3(transform.position.x + (-transform.localScale.x * 1.5f), transform.position.y, transform.position.z);
            sr.gameObject.transform.localPosition = new UnityEngine.Vector3(sr.gameObject.transform.localPosition.x + 2.3f, sr.gameObject.transform.localPosition.y, sr.gameObject.transform.localPosition.z);
            anim.SetTrigger("SpecialAttackShoot");

            yield return new WaitForSeconds(0.15f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.025f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.025f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.025f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.025f / 0.6f);
            DealDamage();
            GetComponent<CapsuleCollider2D>().offset -= new UnityEngine.Vector2(0.3f, 0f);

            yield return new WaitForSeconds(0.52f / 0.6f);

            anim.SetTrigger("ReturnToIdle");
            sr.gameObject.transform.localPosition = initialLocalSpritePos;
            GetComponent<CapsuleCollider2D>().offset = initialLocalCollisionOffset;
            finished = true;
        }

        private void DealDamage()
        {
            if (!isSpecialAttack)
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
            else
            {
                var collisionDetected = Physics2D.OverlapBoxAll(specialAttackPos.position, specialAttackSize, 0);
                foreach (Collider2D attackable in collisionDetected)
                {
                    if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                    {
                        GetComponent<Enemy>().DealDamageToPlayer(specialAttackDamages);
                        hasHitPlayer = true;
                    }
                }
            }
        }

        #endregion
    }
}