using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataType.Skill.Factory.Effect.Def
{
    [Serializable]
    public sealed class TelePortDef : IEffectDef
    {
        [SerializeField]
        [MinValue(1f)]                
        [LabelText("teleport Distance")]   
        public float jumpDistance = 1f;
        public LayerMask obstacleLayer;
        
        // 벽에 닿았을 때 얼마나 뒤로 물러나서 멈출지
        public float stopOffset = 0.5f;
        
        // 레이 시작 위치
        public bool useColliderCenterAsRayOrigin = true;
        public float rayUpOffset = 1.0f;
        
    }
}