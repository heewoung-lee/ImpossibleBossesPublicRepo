using Buffer;
using Buffer.SkillBuffer;
using Controller;
using Controller.ControllerStats;
using GameManagers;
using GameManagers.Interface.BufferManager;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using Module.PlayerModule.PlayerClassModule;
using Skill.BaseSkill;
using UnityEngine;
using Util;
using Zenject;

namespace Skill.AllofSkills.Fighter
{
    public class SkillBuffRoar : SkillDuration
    {
        private readonly IResourcesServices _resourcesServices;
        private readonly IBufferManager _bufferManager;
        private readonly IDetectObject _detectObject;
        private readonly RelayManager _relayManager; 
        private readonly IVFXManagerServices _vfxManager;

        [Inject]
        public SkillBuffRoar(
            IResourcesServices resourcesServices,
            IBufferManager bufferManager,
            IDetectObject detectObject,
            RelayManager relayManager,
            IVFXManagerServices vfxManager
        ) 
        {
            _resourcesServices = resourcesServices;
            _bufferManager = bufferManager;
            _detectObject = detectObject;
            _relayManager = relayManager;
            _vfxManager = vfxManager;
            _buffIconImage = _resourcesServices.Load<Sprite>(BuffIconImagePath);
            _roarModifier = new BufferRoarModifier(_buffIconImage);
        }
        
        private BufferRoarModifier _roarModifier;
        private Collider[] _players = null;
        private BaseController _playerController;
        private ModuleFighterClass _fighterClass;
        private Sprite _buffIconImage;

        public sealed override string BuffIconImagePath =>
            "Art/Player/SkillICon/WarriorSkill/BuffSkillIcon/Icon_Booster_Power";

        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Fighter;
        public override Sprite BuffIconImage => _buffIconImage;
        public override float CoolTime => 5f;
        public override string EffectDescriptionText => $"파티원들에게 10의 공격력을 부여합니다";

        public override Sprite SkillconImage =>
            _resourcesServices.Load<Sprite>("Art/Player/SkillICon/WarriorSkill/SkillIcon/Roar");

        public override float SkillDurationTime => 10f; //지속시간
        public override string SkillName => "분노";
        public override string ETCDescriptionText => "화가난다!";
        public override float Value => 10f;
        public override BuffModifier BuffModifier => _roarModifier;
        public override IState State => _fighterClass.RoarState;

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


        public override void InvokeSkill()
        {
            base.InvokeSkill();
        }

        public override void RemoveStats()
        {
            if (_players != null)
            {
            }
        }

        public override void SkillAction()
        {
            _players = _detectObject.DetectedPlayers();

            _bufferManager.ALL_Character_ApplyBuffAndCreateParticle(_players,
                (playerNgo) =>
                {
                    _vfxManager.InstantiateParticleToChaseTarget("Prefabs/Player/SkillVFX/Aura_Roar", playerNgo.transform,
                        SkillDurationTime);
                }
                ,
                () =>
                {
                    StatEffect effect = new StatEffect(_roarModifier.StatType, Value, _roarModifier.Buffname);
                    _relayManager.NgoRPCCaller.Call_InitBuffer_ServerRpc(effect, BuffIconImagePath,
                        SkillDurationTime);
                });
        }
    }
}