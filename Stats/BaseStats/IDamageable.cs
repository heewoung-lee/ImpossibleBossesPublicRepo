namespace Stats.BaseStats
{
    public interface IDamageable
    {
        public float LastDamagedTime { get;}
        public void OnAttacked(IAttackRange attacker,int damage = -1);
    }
}