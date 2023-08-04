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
        #region Variables

        [Header("Patrol infos")]
        [SerializeField] private Transform[] waypoints;
        private int currentWaypointIndex = 0;
        private UnityEngine.Vector3 currentTargetedPoint;
        [SerializeField] private float waypointDistanceDetection = 0.1f;
        [SerializeField] private bool shouldPatrolRandomly = false;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private bool shouldChasePlayer = false;
        [SerializeField] private bool shouldFinishOnArrived = false;
        [SerializeField] private bool shouldFinishOnDuration = false;
        [SerializeField] private float finishOnDurationTime = 0f;
        private bool finished = false;

        [Header("References")]
        private PlayerController player;
        private Rigidbody2D rb;

        #endregion


        #region Basics

        public override void OnStart()
        {
            currentWaypointIndex = -1;
            FindNewTargetPoint();
            finished = false;

            player = PlayerController.instance;
            rb = GetComponent<Rigidbody2D>();

            if (shouldFinishOnDuration)
            {
                StartCoroutine(FinishOnDurationTimer());
            }
        }

        public override TaskStatus OnUpdate()
        {
            DefineTargetedPoint();

            Move();
            
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

        private void Move()
        {
            float direction = (currentTargetedPoint.x - transform.position.x) / Mathf.Abs(currentTargetedPoint.x - transform.position.x);
            rb.velocity = new UnityEngine.Vector2(moveSpeed * direction, rb.velocity.y);     // clamp Y velocity and move to target

            // Turn
            if (rb.velocity.x > 0.1f)
            {
                if(!shouldChasePlayer)
                    transform.localScale = new UnityEngine.Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                else if(currentTargetedPoint.x - transform.position.x > 0.3f || currentTargetedPoint.x - transform.position.x < -0.3f)
                    transform.localScale = new UnityEngine.Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (rb.velocity.x < -0.1f)
            {
                if (!shouldChasePlayer)
                    transform.localScale = new UnityEngine.Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                else if (currentTargetedPoint.x - transform.position.x > 0.3f || currentTargetedPoint.x - transform.position.x < -0.3f)
                    transform.localScale = new UnityEngine.Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            // Check if arrived to waypoint
            if (UnityEngine.Vector3.Distance(currentTargetedPoint, transform.position) <= waypointDistanceDetection)
            {
                FindNewTargetPoint();
            }
        }

        private void DefineTargetedPoint()
        {
            if (shouldChasePlayer)
            {
                player = PlayerController.instance;
                currentTargetedPoint = player.transform.position;
            }
            else
                currentTargetedPoint = waypoints[currentWaypointIndex].position;
        }

        private void FindNewTargetPoint()
        {
            if (shouldFinishOnArrived)
            {
                finished = true;
                return;
            }

            if (shouldPatrolRandomly)
            {
                int newTargetedPointIndex = Random.Range(0, waypoints.Length);   // Random int from 0 to (tagretedPoints-1)

                if (newTargetedPointIndex != currentWaypointIndex)
                    currentWaypointIndex = newTargetedPointIndex;
                else
                    FindNewTargetPoint();

                DefineTargetedPoint();
            }
            else
            {
                if (currentWaypointIndex >= waypoints.Length - 1)
                    currentWaypointIndex = 0;
                else
                    currentWaypointIndex += 1;

                DefineTargetedPoint();
            }
        }

        private IEnumerator FinishOnDurationTimer()
        {
            yield return new WaitForSeconds(finishOnDurationTime);
            finished = true;
        }

        #endregion
    }
}