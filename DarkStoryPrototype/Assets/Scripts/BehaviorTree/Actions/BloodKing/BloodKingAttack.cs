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
    public class BloodKingAttack : Action
    {
        #region Variables

        [Header("Attack infos")]
        [SerializeField] private int attackType = 1; // 1 = double slash, 2 = stab and spin, 3 = bottom slam, 4 = bullet summon, 5 = slam attack, 6 = dodge charge

        [SerializeField] private Transform doubleSlashPos;
        [SerializeField] private UnityEngine.Vector2 doubleSlashSize;
        [SerializeField] private Transform doubleSlashPos1;
        [SerializeField] private UnityEngine.Vector2 doubleSlashSize1;
        [SerializeField] private Transform doubleSlashPos2;
        [SerializeField] private UnityEngine.Vector2 doubleSlashSize2;

        [SerializeField] private Transform stabAndSpinPos;
        [SerializeField] private UnityEngine.Vector2 stabAndSpinSize;
        [SerializeField] private Transform stabAndSpinPos1;
        [SerializeField] private UnityEngine.Vector2 stabAndSpinSize1;
        [SerializeField] private Transform canStabAndSpinPos;
        [SerializeField] private UnityEngine.Vector2 canstabAndSpinSize;

        [SerializeField] private Transform bottomSlamPos;
        [SerializeField] private UnityEngine.Vector2 bottomSlamSize;
        [SerializeField] private Transform bottomSlamPos1;
        [SerializeField] private UnityEngine.Vector2 bottomSlamSize1;

        [SerializeField] private Transform slamPos;
        [SerializeField] private UnityEngine.Vector2 slamSize;

        [SerializeField] private Transform dodgeChargePos;
        [SerializeField] private UnityEngine.Vector2 dodgeChargeSize;
        [SerializeField] private Transform dodgeChargePos1;
        [SerializeField] private UnityEngine.Vector2 dodgeChargeSize1;
        [SerializeField] private Transform canDodgeChargePos;
        [SerializeField] private UnityEngine.Vector2 canDodgeChargeSize;

        [SerializeField] private int damages = 1;
        [SerializeField] private LayerMask cancelAttacksLayers;
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

            if (attackType == 1)
                StartCoroutine(DoubleSlash());
            else if (!Physics2D.OverlapBox(canStabAndSpinPos.position, canstabAndSpinSize, 0, cancelAttacksLayers) && attackType == 2)
                StartCoroutine(StabAndSpin());
            else if (attackType == 3)
                StartCoroutine(BottomSlam());
            else if (attackType == 4)
                StartCoroutine(BulletSummon());
            else if (attackType == 5)
                StartCoroutine(SlamAttack());
            else if (!Physics2D.OverlapBox(canDodgeChargePos.position, canDodgeChargeSize, 0, cancelAttacksLayers) && attackType == 6)
                StartCoroutine(DodgeCharge());
            else
                finished = true;
        }

        public override TaskStatus OnUpdate()
        {
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

        private IEnumerator DoubleSlash()
        {
            hasHitPlayer = false;

            rb.velocity = new UnityEngine.Vector2(0f, rb.velocity.y);
            anim.SetTrigger("DoubleSlash");

            // Modify sprite pos

            yield return new WaitForSeconds(0.14f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f); // 27/60
            DealDamage();
            yield return new WaitForSeconds(0.05f / 0.6f); // 32/60

            hasHitPlayer = false;
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();

            yield return new WaitForSeconds(0.005f / 0.6f);
            DealDamage();
            yield return new WaitForSeconds(0.005f / 0.6f); // 46/60

            yield return new WaitForSeconds(0.14f / 0.6f); // 60/60

            anim.SetTrigger("ReturnToIdle");
            sr.gameObject.transform.localPosition = initialLocalSpritePos;
            GetComponent<CapsuleCollider2D>().offset = initialLocalCollisionOffset;
            finished = true;

        }

        private IEnumerator StabAndSpin()
        {
            yield return new WaitForSeconds(0f);
        }

        private IEnumerator BottomSlam()
        {
            yield return new WaitForSeconds(0f);
        }

        private IEnumerator BulletSummon()
        {
            yield return new WaitForSeconds(0f);
        }

        private IEnumerator SlamAttack()
        {
            yield return new WaitForSeconds(0f);
        }

        private IEnumerator DodgeCharge()
        {
            yield return new WaitForSeconds(0f);
        }

        private void DealDamage(int comboFirstAttack = 0)
        {
            if (attackType == 1)
            {
                if (comboFirstAttack == 0)
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(doubleSlashPos.position, doubleSlashSize, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(damages);
                            hasHitPlayer = true;
                        }
                    }
                }
                else if (comboFirstAttack == 1)
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(doubleSlashPos1.position, doubleSlashSize1, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(damages);
                            hasHitPlayer = true;
                        }
                    }
                }
                else if (comboFirstAttack == 2)
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(doubleSlashPos2.position, doubleSlashSize2, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(damages);
                            hasHitPlayer = true;
                        }
                    }
                }
            }
            else if (attackType == 2)
            {
                if (comboFirstAttack == 0)
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(stabAndSpinPos.position, stabAndSpinSize, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(damages);
                            hasHitPlayer = true;
                        }
                    }
                }
                else
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(stabAndSpinPos1.position, stabAndSpinSize1, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(damages);
                            hasHitPlayer = true;
                        }
                    }
                }
            }
            else if (attackType == 3)
            {
                if (comboFirstAttack == 0)
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(bottomSlamPos.position, bottomSlamSize, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(damages);
                            hasHitPlayer = true;
                        }
                    }
                }
                else
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(bottomSlamPos1.position, bottomSlamSize1, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(damages);
                            hasHitPlayer = true;
                        }
                    }
                }
            }
            // attack type 4 = bullet summon (so no damages here)
            else if (attackType == 5)
            {
                var collisionDetected = Physics2D.OverlapBoxAll(slamPos.position, slamSize, 0);
                foreach (Collider2D attackable in collisionDetected)
                {
                    if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                    {
                        GetComponent<Enemy>().DealDamageToPlayer(damages);
                        hasHitPlayer = true;
                    }
                }
            }
            else if (attackType == 6)
            {
                if (comboFirstAttack == 0)
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(dodgeChargePos.position, dodgeChargeSize, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(damages);
                            hasHitPlayer = true;
                        }
                    }
                }
                else
                {
                    var collisionDetected = Physics2D.OverlapBoxAll(dodgeChargePos1.position, dodgeChargeSize1, 0);
                    foreach (Collider2D attackable in collisionDetected)
                    {
                        if (LayerMask.LayerToName(attackable.gameObject.layer) == "Player" && !hasHitPlayer)
                        {
                            GetComponent<Enemy>().DealDamageToPlayer(damages);
                            hasHitPlayer = true;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
