using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class CheckDistanceToPlayer : Conditional
    {
        [SerializeField] private float rangeToStartChaseX;
        [SerializeField] private float rangeToStartChaseY;
        private Transform player;

        public override void OnStart()
        {
            player = PlayerController.instance.transform;
        }

        public override TaskStatus OnUpdate()
        {
            if((player.position.x >= (transform.position.x - rangeToStartChaseX) && player.position.x <= (transform.position.x + rangeToStartChaseX)) && (player.position.y >= (transform.position.y - rangeToStartChaseY) && player.position.y <= (transform.position.y + rangeToStartChaseY)))
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }
}