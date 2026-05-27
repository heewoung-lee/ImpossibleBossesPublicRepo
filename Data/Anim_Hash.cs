using UnityEngine;

namespace Data
{
    public class PlayerAnimHash
    {

        public static int Idle { get; private set; } = Animator.StringToHash("Idle");
        public static int Run { get; private set; } = Animator.StringToHash("Run");
        public static int Die { get; private set; } = Animator.StringToHash("Die");
        public static int Attack { get; private set; } = Animator.StringToHash("Attack");
    }
    public class BossGolemAnimHash
    {
        public static int GolemWait { get; private set; } = Animator.StringToHash("Golem_Wait");
        public static int GolemIdle { get; private set; } = Animator.StringToHash("Golem_Idle");
        public static int GolemRise { get; private set; } = Animator.StringToHash("Golem_Rise");
        public static int GolemWalk { get; private set; } = Animator.StringToHash("Golem_Walk");
        public static int GolemAttack1 { get; private set; } = Animator.StringToHash("Golem_Attack1");
        public static int GolemAttack2 { get; private set; } = Animator.StringToHash("Golem_Attack2");
        public static int GolemSkill { get; private set; } = Animator.StringToHash("Golem_Skill");
        public static int GolemAttacked { get; private set; } = Animator.StringToHash("Golem_Attacked");
        public static int GolemSpawnRock { get; private set; } = Animator.StringToHash("SpawnRock");
        public static int GolemDead { get; private set; } = Animator.StringToHash("Golem_Dead");

    }
    
    
    
    public class BossDarkWizardAnimHash
    {
        public static int DarkWizardIdle { get; private set; } = Animator.StringToHash("DarkWizard_Idle");
        public static int DarkWizardDashAttack { get; private set; } = Animator.StringToHash("DarkWizard_DashAttack");
        public static int DarkWizardCastSpell { get; private set; } = Animator.StringToHash("DarkWizard_CastSpell");
        public static int DarkWizardFlyForward { get; private set; } = Animator.StringToHash("DarkWizard_FlyForward");
        public static int DarkWizardDie { get; private set; } = Animator.StringToHash("DarkWizard_Death");
        public static int DarkWizardSpawn { get; private set; } = Animator.StringToHash("DarkWizard_Spawn");
        public static int DarkWizardProjectileAttack { get; private set; } = Animator.StringToHash("DarkWizard_ProjectileAttack");
        public static int DarkWizardSlashAttack { get; private set; } = Animator.StringToHash("DarkWizard_SlashAttack");
        public static int DarkWizardTakeDamage { get; private set; } = Animator.StringToHash("DarkWizard_TakeDamage");

    }
    
    
    public class BossRedDragonAnimHash
    {
        public static int RedDragonIdle { get; private set; } = Animator.StringToHash("RedDragonIdle");
        public static int RedDragonDie { get; private set; } = Animator.StringToHash("RedDragonDeath");
        public static int RedDragonMove { get; private set; } = Animator.StringToHash("RedDragonWalk");
        public static int RedDragonAttack { get; private set; } = Animator.StringToHash("RedDragonGroundAttack");
        public static int RedDragonTailAttack { get; private set; } = Animator.StringToHash("RedDragonTailAttack");
        public static int RedDragonCast { get; private set; } = Animator.StringToHash("RedDragonCast");
        public static int RedDragonBreath { get; private set; } = Animator.StringToHash("RedDragonBreath");
        public static int RedDragonBreathStart { get; private set; } = Animator.StringToHash("RedDragonBreathStart");
        public static int RedDragonBreathLoop { get; private set; } = Animator.StringToHash("RedDragonBreathLoop");
        public static int RedDragonBreathEnd { get; private set; } = Animator.StringToHash("RedDragonBreathEnd");
        public static int RedDragonSpawnMinion { get; private set; } = Animator.StringToHash("RedDragonSpawnMinion");
        public static int RedDragonProjectileAttack { get; private set; } = Animator.StringToHash("RedDragonProjectileAttack");
        public static int RedDragonFlyMove { get; private set; } = Animator.StringToHash("FlyMove");
        public static int RedDragonJump { get; private set; } = Animator.StringToHash("RedDragonJump");
        public static int RedDragonLanding { get; private set; } = Animator.StringToHash("RedDragonLanding");
        
        
    }
    
}
