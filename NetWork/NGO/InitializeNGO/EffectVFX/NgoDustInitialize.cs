using NetWork.BaseNGO;

namespace NetWork.NGO.InitializeNGO.EffectVFX
{
    public class NgoDustInitialize : NgoPoolingInitializeBase
    {
        public override string PoolingNgoPath => "Prefabs/Particle/AttackEffect/Dust_Particle";

        public override int PoolingCapacity => 100;

    }
}
