using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Pathfinding;
using System.Numerics;
using UnityEngine.UIElements;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class TurnToPlayer : Action
    {
        #region Basics

        public override TaskStatus OnUpdate()
        {
            transform.localScale = new UnityEngine.Vector3((PlayerController.instance.gameObject.transform.position.x - transform.position.x) / Mathf.Abs(PlayerController.instance.gameObject.transform.position.x - transform.position.x), 1, 1);
            return TaskStatus.Success;
        }

        #endregion
    }
}