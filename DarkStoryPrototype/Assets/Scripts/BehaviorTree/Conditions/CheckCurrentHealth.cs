using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class CheckCurrentHealth : Conditional
    {
        [SerializeField] bool healthUnder = false;
        [SerializeField] bool healthEqual = false;
        [SerializeField] bool healthAbove = false;
        [SerializeField] int healthToCheck = 0;
        private Enemy enemy;

        public override void OnStart()
        {
            enemy = GetComponent<Enemy>();
        }

        public override TaskStatus OnUpdate()
        {
            if ((healthUnder && enemy.currentHealth < healthToCheck) || (healthEqual && enemy.currentHealth == healthToCheck) || (healthAbove && enemy.currentHealth > healthToCheck))
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }
}