using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class CheckPlayerDetected : Conditional
    {
        [SerializeField] private float rangeToStartChaseX;
        [SerializeField] private float rangeToStartChaseY;
        [SerializeField] private bool shouldStillTrue = false;
        [SerializeField] private bool shouldOnlyCheckDistance = false;
        private Transform player;

        public override void OnStart()
        {
            player = PlayerController.instance.transform;
        }

        public override TaskStatus OnUpdate()
        {
            if (shouldStillTrue)
            {
                if (((player.position.x >= (transform.position.x - rangeToStartChaseX) && player.position.x <= (transform.position.x + rangeToStartChaseX)) && (player.position.y >= (transform.position.y - rangeToStartChaseY) && player.position.y <= (transform.position.y + rangeToStartChaseY))) || GetComponent<Enemy>().playerDetected)
                {
                    if(!GetComponent<Enemy>().playerDetected)
                        GetComponent<Enemy>().DetectPlayer();
                    return TaskStatus.Success;
                }
                else
                {
                    return TaskStatus.Failure;
                }
            }
            else
            {
                if ((player.position.x >= (transform.position.x - rangeToStartChaseX) && player.position.x <= (transform.position.x + rangeToStartChaseX)) && (player.position.y >= (transform.position.y - rangeToStartChaseY) && player.position.y <= (transform.position.y + rangeToStartChaseY)))
                {
                    if (!GetComponent<Enemy>().playerDetected && !shouldOnlyCheckDistance)
                        GetComponent<Enemy>().DetectPlayer();
                    return TaskStatus.Success;
                }
                else
                {
                    return TaskStatus.Failure;
                }
            }
        }
    }
}