using System;
using Controller;
using Skill;
using UnityEngine;

namespace DataType
{
    public interface IStrategy
    {
        public void Execute(ExecutionContext context);
    }
}