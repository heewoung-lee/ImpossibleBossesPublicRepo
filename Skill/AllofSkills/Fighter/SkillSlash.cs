using System.Collections;
using Controller;
using Controller.ControllerStats;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using Module.PlayerModule;
using Module.PlayerModule.PlayerClassModule;
using Skill.BaseSkill;
using Stats;
using UnityEngine;
using Util;
using Zenject;

namespace Skill.AllofSkills.Fighter
{
    public class SkillSlash : SkillImmedialty
    {
        private readonly IPlayerSpawnManager _gameManagerEx;
        private readonly IResourcesServices _resourcesServices;
        private readonly IVFXManagerServices _vfxManager;
        private readonly ICoroutineRunner _coroutineRunner;
        
        [Inject]
        public SkillSlash(
            IPlayerSpawnManager gameManagerEx,
            IResourcesServices resourcesServices,
            IVFXManagerServices vfxManager,
            ICoroutineRunner coroutineRunner)
        {
            _resourcesServices = resourcesServices;
            _gameManagerEx = gameManagerEx;
            _vfxManager = vfxManager;
            _coroutineRunner = coroutineRunner;
        }
        private BaseController _playerController;
        private PlayerStats _playerStat;
        private ModuleFighterClass _fighterClass;
        private AnimationClip _slashAnimClip;


        public AnimationClip SlashAnimClip
        {
            get
            {
                if (_slashAnimClip == null)
                {
                    _slashAnimClip = _playerController.GetComponent<ModulePlayerAnimInfo>()
                        .GetAnimationClip(_fighterClass.HashSlash);
                }

                return _slashAnimClip;
            }
        }

        public float AttackDamage
        {
            get
            {
                if (_playerStat == null)
                {
                    _playerStat = _gameManagerEx.GetPlayer().GetComponent<PlayerStats>();
                }

                return _playerStat.Attack * Value;
            }
        }

        public PlayerStats PlayerStat
        {
            get
            {
                if (_playerStat == null)
                {
                    _playerStat = _gameManagerEx.GetPlayer().GetComponent<PlayerStats>();
                }

                return _playerStat;
            }
        }

        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Fighter;
        public override string SkillName => "강베기";
        public override float CoolTime => 2f;
        public override string EffectDescriptionText => $"적에게{AttackDamage}만큼 X3의 피해를 줍니다.";
        public override string ETCDescriptionText => "강하게 벤다";

        public override Sprite SkillconImage =>
            _resourcesServices.Load<Sprite>("Art/Player/SkillICon/WarriorSkill/SkillIcon/Slash");

        public override float Value => 1.5f;

        public override BaseController PlayerController
        {
            get => _playerController;
            protected set => _playerController = value;
        }

        public override ModulePlayerClass ModulePlayerClass
        {
            get => _fighterClass;
            protected set => _fighterClass = value as ModuleFighterClass;
        }

        public override IState State => _fighterClass.SlashState;
        public override void SkillAction()
        {
            _vfxManager.InstantiateParticleToChaseTarget("Prefabs/Player/SkillVFX/Fighter_Slash",_playerController.transform);
            _coroutineRunner.RunCoroutine(FrameInHit(PlayerStat, SlashAnimClip.length));
        }

        IEnumerator FrameInHit(PlayerStats stats, float animLength)
        {
            float duration = 0f;
            float[] hitFrames = new float[3] { 0.25f, 0.5f, 0.75f };
            int hitIndex = 0;
            while (duration < 1)
            {
                duration += Time.deltaTime / animLength;

                if (hitIndex < hitFrames.Length && duration > hitFrames[hitIndex])
                {
                    TargetInSight.AttackTargetInSector(stats, (int)AttackDamage);
                    hitIndex++;
                }

                yield return null;
            }

            duration = 0;
        }
    }
}