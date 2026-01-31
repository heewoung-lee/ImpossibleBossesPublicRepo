using System;
using DataType.Skill.ShareDataDef;
using Sirenix.OdinInspector;

namespace DataType.Skill.Factory.Decorator.Def.Start
{
    [Serializable]
    public class AreaVfxDecoratorDef : INormalDecoratorDef
    {
        [FolderPath(ParentFolder = "Assets/Resources")]
        public string castVfxPath;
        public DurationRefDef castVfxDuration = new DurationRefDef();
        public ScaleRefDef scaleRef;
    }
}