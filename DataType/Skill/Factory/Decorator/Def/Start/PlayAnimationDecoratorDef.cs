using System;
using DataType.Skill.ShareDataDef;

namespace DataType.Skill.Factory.Decorator.Def.Start
{
    [Serializable]
    public sealed class PlayAnimationDecoratorDef : INormalDecoratorDef 
    {
        public AnimNameRefDef animNameRef = new AnimNameRefDef();
    }
}