using System;
using System.Collections.Generic;
using DataType.Skill.Factory.Decorator;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Effect;
using DataType.Skill.Factory.Sequence;
using DataType.Skill.Factory.Target;
using DataType.Skill.Factory.Trigger;
using DataType.Skill.ShareDataDef;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Util;

namespace DataType.Skill
{
    [CreateAssetMenu(fileName = "SkillSO", menuName = "DataSO/Skill/SkillSO")]
    public class SkillDataSO : BaseDataSO
    {
        [Title("Basic Information")] 
        public Define.PlayerClass playerClass; // 사용 클래스
        
        [TextArea]
        public string etcDescription; // 스킬 스토리 설명용 보조창
        
        [Title("Numerical Data")]
        [Min(0.1f)]
        public float cooldown; // 예: 2f
        
        [Title("Animation Info")] 
        [Tooltip("애니메이션 전환 시간 (기본 0.1~0.3)")] 
        public string animationStateName; // Animator State 이름
        [MinValue(0f)]
        public float transitionDuration = 0.1f;
        public bool isAnimationLocked = true;
        
       
        [Title("Visual & Animation")] 
        public string vfxPrefabPath; // Resources path 등
  
        
        [Title("Variable References")]
        [Tooltip("스킬에 쓸 변수들을 여기에 추가하면 된다.")] 
        [SerializeReference]
        [ListDrawerSettings(ShowFoldout = true)]
        public List<ISkillSharedDef> sharedDefs = new List<ISkillSharedDef>();
        private int _curCount = 0;
        
        
        
        // -------------------------
        //Pipeline recipe
        // -------------------------
        [Title("Pipeline Defs")] 
        [SerializeReference] public ITriggerDef trigger;
        [SerializeReference] public ITargetingDef targeting;
        [SerializeReference] public ISequenceDef sequence;
        public DecoratorStackDef decorator = new DecoratorStackDef();
        [SerializeReference] public IEffectDef effect;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (trigger == null) Debug.LogError($"[{name}] Trigger is null");
            if (targeting == null) Debug.LogError($"[{name}] Targeting is null");
            if (sequence == null) Debug.LogError($"[{name}] Sequence is null");
            if (decorator == null) Debug.LogError($"[{name}] Decorator is null");
            if (effect == null) Debug.LogError($"[{name}] Effect is null");

            SetPrimeShareValue();
        }


        /// <summary>
        /// 공유변수는 반드시 하나만 가져야하고 중복되는 값이 있으면 추가안되게 막아놓음
        /// </summary>
        private void SetPrimeShareValue()
        {
            if (_curCount == sharedDefs.Count)
                return;

            if (_curCount < sharedDefs.Count && sharedDefs.Count > 0 )
            {
                ISkillSharedDef currentSharedDef = sharedDefs[sharedDefs.Count-1];

                for (int i = 0; i < sharedDefs.Count - 1; i++)
                {
                    if (currentSharedDef.GetType() == sharedDefs[i].GetType())
                    {
                        sharedDefs.RemoveAt(sharedDefs.Count-1);
                        Debug.Assert(false,"you already have the same type");
                        return;
                    }
                }
            }
            _curCount = sharedDefs.Count;
        }
#endif
    }
}
