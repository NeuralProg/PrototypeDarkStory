using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Pathfinding;
using System.Numerics;
using UnityEngine.UIElements;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class StopMovement : Action
    {
        #region Basics

        public override TaskStatus OnUpdate()
        {
            GetComponent<Rigidbody2D>().velocity = new UnityEngine.Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
            return TaskStatus.Success;
        }

        #endregion
    }
}