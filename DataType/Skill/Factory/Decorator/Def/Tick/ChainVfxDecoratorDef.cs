using System;
using DataType.Skill.ShareDataDef;

namespace DataType.Skill.Factory.Decorator.Def
{
    [Serializable]
    public sealed class ChainVfxDecoratorDef : ITickDecoratorDef
    {
        // Resources.Load 경로 (확장자/Resources 생략)
        public VFXPathRefDef hitvfxPath = new VFXPathRefDef();
        // 한 번의 hop(링크) 빔 수명
        public DurationRefDef hitVfxDuration = new DurationRefDef();

        // 시작/끝 위치 보정 (보통 가슴/중심 높이)
        public float startYOffset = 1.0f;
        public float endYOffset = 1.0f;

    }
}