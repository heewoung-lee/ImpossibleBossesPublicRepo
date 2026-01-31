using System;
using Controller;
using Stats.BaseStats;
using UnityEngine;

namespace DataType.Skill.Factory.Target.Def
{
    public enum TargetState
    {
        Die,
        Alive
    }
    
    [Serializable]
    public class ExtraTargetCondition
    {
        [SerializeField]
        TargetState targetState;

        public bool CheckValidState(BaseStats playerStat)
        {
            if (playerStat == null)
            {
                return false;
            }
            
            switch (targetState)
            {
                case TargetState.Die:
                    if (playerStat.IsDead == true)
                    {
                        return true;
                    }
                    return false;
                case TargetState.Alive:
                    if (playerStat.IsDead == false)
                    {
                        return true;
                    }
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}