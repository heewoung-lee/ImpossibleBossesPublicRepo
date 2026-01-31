using System;
using UnityEngine;

namespace DataType.Skill.Factory.Decorator.Def
{
    [Serializable]
    public sealed class HitEventDef
    {
        [Range(0f, 1f)] public float normalizedTime;

        //[SerializeReference] public ITargetQueryDef query;
        //1.22일 수정 앞으로 정규화 시간이 지날때마다 쿼리는
        //이전 타겟팅모듈이 가지고 있는 query를 통해 반복할것
        
    }
}