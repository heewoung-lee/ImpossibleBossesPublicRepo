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
    public class SkillBuffDetermination : SkillDuration
    {
        private readonly IResourcesServices _resourcesServices;
        private readonly IBufferManager _bufferManager;
        private readonly IDetectObject _detectObject;
        private readonly RelayManager _relayManager;
        private readonly IVFXManagerServices _vfxManager;

        [Inject]
        public SkillBuffDetermination(
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
            _determination = new BufferDetermination(_buffIconImage);
        }


        //TODO: 버프 아이콘 이미지 바꿀것
        private Sprite _buffIconImage;
        private BufferDetermination _determination;
        private Collider[] _players = null;
        private BaseController _playerController;
        private ModuleFighterClass _fighterClass;

        public sealed override string BuffIconImagePath =>
            "Art/Player/SkillICon/WarriorSkill/BuffSkillIcon/IconSet_Equip_Helmet";

        public override float SkillDurationTime => 10f;
        public override Sprite BuffIconImage => _buffIconImage;
        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Fighter;
        public override string SkillName => "결의";
        public override float CoolTime => 5f;
        public override string EffectDescriptionText => $"파티원들에게 10의 방어력을 부여합니다";
        public override string ETCDescriptionText => "서로간의 결의";

        public override Sprite SkillconImage =>
            _resourcesServices.Load<Sprite>("Art/Player/SkillICon/WarriorSkill/SkillIcon/Determination");

        public override float Value => 10f;
        public override BuffModifier BuffModifier => _determination;

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

        public override IState State => _fighterClass.DeterminationState;


        public override void InvokeSkill()
        {
            base.InvokeSkill();
        }

        public override void RemoveStats()
        {
            //해제되었을때 추가 로직
        }

        public override void SkillAction()
        {
            _players = _detectObject.DetectedPlayers();

            _bufferManager.ALL_Character_ApplyBuffAndCreateParticle(_players,
                (playerNgo) =>
                {
                    _vfxManager.InstantiateParticleToChaseTarget("Prefabs/Player/SkillVFX/Shield_Determination",
                        playerNgo.transform, SkillDurationTime);
                }
                ,
                () =>
                {
                    StatEffect effect = new StatEffect(_determination.StatType, Value, _determination.Buffname);
                    _relayManager.NgoRPCCaller.Call_InitBuffer_ServerRpc(effect, BuffIconImagePath,
                        SkillDurationTime);
                });
        }
    }
}