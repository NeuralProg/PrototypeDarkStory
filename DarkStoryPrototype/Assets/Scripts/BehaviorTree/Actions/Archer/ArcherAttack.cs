using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Pathfinding;
using System.Numerics;
using UnityEngine.UIElements;

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
        [SerializeField] private int normalAttackDamages = 1;
        [SerializeField] private int specialAttackDamages = 2;
        private bool finished;

        [Header("References")]
        [SerializeField] private Animator anim;
        [SerializeField] private SpriteRenderer sr;
        private Rigidbody2D rb;
        private UnityEngine.Vector3 initialLocalSpritePos;

        #endregion


        #region Basics

        public override void OnStart()
        {
            finished = false;

            rb = GetComponent<Rigidbody2D>();
            initialLocalSpritePos = sr.gameObject.transform.localPosition;

            if (!GetComponent<Enemy>().isKnockbacking)
            {
                if (!isSpecialAttack)
                    StartCoroutine(NormalAttack());
                //else
            }
            else
                finished = true;

        }

        public override TaskStatus OnUpdate()
        {
            if (GetComponent<Enemy>().isKnockbacking && Random.Range(0, 2) == 0)
            {
                sr.gameObject.transform.localPosition = initialLocalSpritePos;
                StopAllCoroutines();
                finished = false;
                return TaskStatus.Success;
            }

            if (finished)
            {
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
            rb.velocity = new UnityEngine.Vector2(0f, rb.velocity.y);
            anim.SetTrigger("Attack");
            sr.gameObject.transform.localPosition = new UnityEngine.Vector3(sr.gameObject.transform.localPosition.x + 0.4f, sr.gameObject.transform.localPosition.y, sr.gameObject.transform.localPosition.z);
            yield return new WaitForSeconds(0.25f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.35f / 0.6f);
            sr.gameObject.transform.localPosition = initialLocalSpritePos;
            finished = true;
        }
        private IEnumerator SpecialAttack()
        {
            rb.velocity = new UnityEngine.Vector2(0f, rb.velocity.y); // vanish, disable collision (layer mask), appear, attack
            anim.SetTrigger("Attack");
            yield return new WaitForSeconds(0.06f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.36f / 0.6f);
            finished = true;
        }

        private void DealDamage()
        {
            if (!isSpecialAttack)
            {
                var collisionDetected = Physics2D.OverlapBoxAll(normalAttackPos.position, normalAttackSize, 0);
                foreach (Collider2D attackable in collisionDetected)
                {
                    if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player")
                        GetComponent<Enemy>().DealDamageToPlayer(normalAttackDamages);
                }
            }
            else
            {
                var collisionDetected = Physics2D.OverlapBoxAll(specialAttackPos.position, specialAttackSize, 0);
                foreach (Collider2D attackable in collisionDetected)
                {
                    if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player")
                        GetComponent<Enemy>().DealDamageToPlayer(specialAttackDamages);
                }
            }
        }

        #endregion
    }
}