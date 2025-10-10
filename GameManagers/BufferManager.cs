using System;
using System.Collections.Generic;
using Buffer;
using GameManagers.Interface;
using GameManagers.Interface.BufferManager;
using GameManagers.Interface.DataManager;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIManager;
using Stats.BaseStats;
using UI.Scene.SceneUI;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers
{

    
    
    
    public class BufferManager: IInitializable,IBufferManager,IDetectObject
    {
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourcesServices;
        private readonly IRequestDataType _requestDataType;
        private readonly IBufferTypeCreator _bufferTypeCreator;

        private readonly Dictionary<string, BuffModifier> _allBuffModifierDict;
        
        
        [Inject]
        public  BufferManager(
            IUIManagerServices uiManagerServices,
            IResourcesServices resourcesServices,
            IRequestDataType requestDataType, IBufferTypeCreator bufferTypeCreator) 
        {
            _uiManagerServices = uiManagerServices;
            _resourcesServices = resourcesServices;
            _requestDataType = requestDataType;
            _bufferTypeCreator = bufferTypeCreator;
            _allBuffModifierDict = new Dictionary<string, BuffModifier>();
        }
        
        private IList<Type> _requestType = new List<Type>();
        private UIBufferBar _uiBufferBar;
        
        
        private UIBufferBar UIBufferBar
        {
            get
            {
                if (_uiBufferBar == null)
                    _uiBufferBar = _uiManagerServices.Get_Scene_UI<UIBufferBar>();

                return _uiBufferBar;
            }
        }
        public BuffModifier GetModifier(StatEffect efftect)
        {
            return _allBuffModifierDict[efftect.buffname];
        }
        public BufferComponent InitBuff(BaseStats targetStat, float duration,StatEffect effect)
        {
            GameObject bufferGo = _resourcesServices.InstantiateByKey("Prefabs/Buffer/Buffer", UIBufferBar.BufferContext);
            BufferComponent buffer = _resourcesServices.GetOrAddComponent<BufferComponent>(bufferGo);
            buffer.InitAndStartBuff(targetStat, duration, effect);
            return buffer;
        }

        public BufferComponent InitBuff(BaseStats targetStat, float duration, BuffModifier bufferModifier,float value)
        {
            GameObject bufferGo = _resourcesServices.InstantiateByKey("Prefabs/Buffer/Buffer", UIBufferBar.BufferContext);
            BufferComponent buffer = _resourcesServices.GetOrAddComponent<BufferComponent>(bufferGo);
            buffer.InitAndStartBuff(targetStat, duration, bufferModifier, value);
            return buffer;
        }
        public void RemoveBuffer(BufferComponent buffer)
        {
            DurationBuff durationBuff = buffer.Modifier as DurationBuff;
            durationBuff.RemoveStats(buffer.TarGetStat, buffer.Value);
            _resourcesServices.DestroyObject(buffer.gameObject);
        }

        public void ImmediatelyBuffStart(BufferComponent buffer)
        {
            buffer.Modifier.ApplyStats(buffer.TarGetStat,buffer.Value);
            _resourcesServices.DestroyObject(buffer.gameObject);
        }

        public void Initialize()
        {
            _requestType = _requestDataType.LoadSerializableTypesFromFolder("Assets/Scripts/Buffer/Buffer_Type", GetBuffModifierType);
            foreach(Type type in _requestType)
            {
                // Activator.CreateInstance로 인스턴스 생성 _requestType은 메타데이터 이므로 인스턴스가 아님
                //따라서 Type 메타정보를 바탕으로 인스턴스를 생성해줘야함
                //TODO: 이쪽에 BufferModifier가 동적으로 생성되다보니. Container주입이 안됨.
                BuffModifier modifierInstance = _bufferTypeCreator.CreateBufferType(type);
                _allBuffModifierDict.Add(modifierInstance.Buffname, modifierInstance);
            }
        }
        private void GetBuffModifierType(Type type, List<Type> typeList)
        {
            if (typeof(BuffModifier).IsAssignableFrom(type))
            {
                typeList.Add(type);
            }
        }
        public void ALL_Character_ApplyBuffAndCreateParticle(Collider[] targets,Action<NetworkObject> createPaticle,Action invokeBuff)
        {
            foreach (Collider playersCollider in targets)
            {
                if (playersCollider.TryGetComponent(out NetworkObject playerNgo))
                {
                    createPaticle.Invoke(playerNgo);
                    if (playerNgo.IsOwner)
                    {
                        invokeBuff.Invoke();
                    }

                }
            }
        }
        
        public Collider[] DetectedPlayers()
        {
            LayerMask playerLayerMask = LayerMask.GetMask("Player") | LayerMask.GetMask("AnotherPlayer");
            float skillRadius = float.MaxValue;
            return  Physics.OverlapSphere(Vector3.zero,skillRadius,playerLayerMask);
        }
        public Collider[] DetectedOther(params string[] layerName)
        {
            LayerMask detectTargetMask = LayerMask.GetMask(layerName);
            float skillRadius = float.MaxValue;
            return  Physics.OverlapSphere(Vector3.zero,skillRadius,detectTargetMask);
        }

        
    }
}