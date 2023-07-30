using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Pathfinding;
using System.Numerics;
using UnityEngine.UIElements;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class GroundPatrol : Action
    {
        [Header("Target Info")]
        [SerializeField] private UnityEngine.Transform[] targetedPoints;
        private UnityEngine.Transform targetedPoint;
        [SerializeField] private bool shouldChooseRandomly = false;
        private int currentTargetIndex = 0;
        [SerializeField] private bool shouldChasePlayer = false;

        [Header("Patrol Info")]
        [SerializeField] private float speed = 3f;
        [SerializeField] private bool finishOnHit = false;
        private bool finished = false;
        [SerializeField] private bool finishOnDuration = false;
        [SerializeField] private float finishOnDurationTime = 0f;
        private float finishOnDurationTimer;
        [SerializeField] private UnityEngine.Transform checkFront, checkGround, centerPos;
        [SerializeField] private float checkRadius = 0.5f;
        [SerializeField] private LayerMask frontJump, whatIsGround;
        [SerializeField] private float nextWaypointDistance = 0.1f;

        [Header("Jump")]
        [SerializeField] private bool jumpEnabled = true;
        [SerializeField] private float playerDistanceToJump = 0f;
        [SerializeField] private float jumpMaxHeightActivation = 0.5f;
        [SerializeField] private float jumpForce = 15f;
        private bool isGrounded = false;

        private Path path;
        private int currentWaypoint = 0;

        private float updateDelay = 0.5f;
        private float updateTimer;

        private Rigidbody2D rb;
        private Seeker seeker;
        private PlayerController player;

        #region Basics

        public override void OnStart()          
        {
            rb = GetComponent<Rigidbody2D>();
            seeker = GetComponent<Seeker>();
            player = PlayerController.instance;

            if (finishOnDuration)
                finishOnDurationTimer = finishOnDurationTime;

            if (shouldChasePlayer)
            {
                targetedPoint = player.gameObject.transform;
            }
            else
            {
                FindNewTargetPoint();
            }

            updateTimer = updateDelay;
            UpdatePath();
        }

        public override TaskStatus OnUpdate()
        {
            updateTimer -= Time.deltaTime;

            bool shouldUpdate = (finishOnHit && shouldChasePlayer) || (!finishOnHit);
            if (updateTimer <= 0 && shouldUpdate)
            {
                UpdatePath();
                updateTimer = updateDelay;
            }

            if(path == null)
                return TaskStatus.Running;

            if (currentWaypoint >= path.vectorPath.Count) // If we are above the number of waypoints of the path
            {
                if (finishOnHit)
                    finished = true;

                if (!shouldChasePlayer)
                    FindNewTargetPoint();

                currentWaypoint = 0;
            }

            if (shouldChasePlayer)
                targetedPoint = player.gameObject.transform;

            if (finishOnDuration)
                finishOnDurationTimer -= Time.deltaTime;
            if (finishOnDurationTimer < 0f)
                finished = true;

            isGrounded = Physics2D.OverlapCircle(checkGround.position, checkRadius, whatIsGround);

            // Direction Calculation
            UnityEngine.Vector2 direction = ((UnityEngine.Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

            rb.velocity = new UnityEngine.Vector2(direction.x * speed, rb.velocity.y);

            // Jump
            if (jumpEnabled && isGrounded)
            {
                if ((direction.y > jumpMaxHeightActivation && Mathf.Abs(centerPos.transform.position.x - player.transform.position.x) <= playerDistanceToJump) || Physics2D.OverlapCircle(checkFront.position, checkRadius, frontJump))
                {
                    rb.velocity = new UnityEngine.Vector2(rb.velocity.x, jumpForce);
                    gameObject.GetComponentInChildren<Animator>().SetTrigger("Jump");
                }
            }

            // Direction Graphics Handling
            if (rb.velocity.x > 0.1f)
            {
                transform.localScale = new UnityEngine.Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (rb.velocity.x < -0.1f)
            {
                transform.localScale = new UnityEngine.Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            float distance = UnityEngine.Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
            if (distance <= nextWaypointDistance)
                currentWaypoint++;

            gameObject.GetComponentInChildren<Animator>().SetFloat("Speed", Mathf.Abs(rb.velocity.x));

            if (finished)
            {
                finished = false;
                return TaskStatus.Success;
            }
            else
                return TaskStatus.Running;
        }

        #endregion

        #region Extra Functions

        private void UpdatePath()
        {
            if (seeker.IsDone()) // Check if the seeker isn't currently making a path
                seeker.StartPath(rb.position, targetedPoint.position, OnPathComplete);
        }

        private void OnPathComplete(Path p)
        {
            if (!p.error) // after calculating the path, make the enemy use it
            {
                path = p;
                currentWaypoint = 0;
            }
        }

        private void FindNewTargetPoint()
        {
            if (shouldChooseRandomly)
            {
                Transform newTargetedPoint = targetedPoints[Random.Range(0, targetedPoints.Length)]; // Random int from 0 to (tagretedPoints-1)

                if (newTargetedPoint != targetedPoint)
                    targetedPoint = newTargetedPoint;
                else
                    FindNewTargetPoint();

                float distance = UnityEngine.Vector2.Distance(rb.position, targetedPoint.position);
                if (distance <= nextWaypointDistance)
                    FindNewTargetPoint();
            }
            else
            {
                if (currentTargetIndex >= targetedPoints.Length - 1)
                    currentTargetIndex = 0;
                else
                    currentTargetIndex += 1;

                targetedPoint = targetedPoints[currentTargetIndex];
            }
        }

        #endregion

    }
}