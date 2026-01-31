using System;
using Sirenix.OdinInspector;
using Stats.BaseStats;
using UnityEngine;

namespace DataType.Skill.Factory.Effect.Def
{
   [Serializable]
    public class BufferEffectDef: IEffectDef
    {
        [SerializeField]
        [MinValue(0.1f)]
        private int valueInt = 1;

        public float Value => valueInt;
        public DurationRefDef skillduration = new DurationRefDef();
        public string buffIconPath;
        public StatType buffType;
    }
}