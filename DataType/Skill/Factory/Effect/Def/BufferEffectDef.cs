using System;
using Sirenix.OdinInspector;
using Stats.BaseStats;
using UnityEngine;

namespace DataType.Skill.Factory.Effect.Def
{
   [Serializable]
    public class BufferEffectDef: IEffectDef
    {
        //절대 MinValue 잡지 말것 디버프 용으로도 쓸 수 있으니깐
        //음수가 뜨는것도 정상임
        [SerializeField]
        private int valueInt = 1;

        public float Value => valueInt;
        public DurationRefDef skillduration = new DurationRefDef();
        public string buffIconPath;
        public StatType buffType;
    }
}