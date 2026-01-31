using Controller;
using Skill;
using UnityEngine;

namespace DataType
{
    public abstract class ExecutionContext
    {
        public BaseController Caster { get; }
        public BaseDataSO Data { get; }
        protected ExecutionContext(BaseController caster, BaseDataSO data)
        {
            Caster = caster;
            Data = data;
        }
    }
}