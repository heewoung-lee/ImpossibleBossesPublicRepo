using System;
using UnityEngine;

namespace DataType.Skill.Factory.Target.Def
{
    [Serializable]
    public sealed class CircleQueryDef
    {
        // 어떤 레이어의 콜라이더만 맞을지(적 레이어 등)
        public LayerMask mask;

        // 범위 반지름
        public float radius = 2.0f;

        // 캡슐의 높이
        public float height = 0f;

        // 캐스터 기준으로 앞쪽으로 얼마나 밀어서 검사할지
        public float forwardOffset = 0f;

        // 360이면 원형, 120이면 전방 부채꼴(±60도)
        [Range(0f, 360f)]
        public float angle = 360f;
    }
    
    [Serializable]
    public sealed class CircleQueryTargetingDef : ITargetingDef
    {
        //1.22일 수정 아예 쿼리 자체를 강타입으로 묶어버림.
        //제각기 다른 쿼리방식을 타겟이 유연하게 쓸 수 있는 방식이 아무리
        //생각해도 안떠오름 그래서 강타입으로 묶음
        [SerializeReference] public CircleQueryDef circleQueryDef = new CircleQueryDef();
    }
}