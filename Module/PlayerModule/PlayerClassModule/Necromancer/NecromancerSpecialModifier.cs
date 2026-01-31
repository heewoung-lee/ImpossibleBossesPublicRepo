using System;
using System.Collections.Generic;
using GameManagers.ResourcesEx;
using Stats.BaseStats;
using Test.TestScripts;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule.Necromancer
{
    public class NecromancerSpecialModifier : NetworkBehaviour, ISpecialModifier,ITargetable
    {
        private IResourcesServices _resourceService;
        private Material _changeMaterial;
        private Dictionary<Renderer, Material[]> _originalMaterialCache = new Dictionary<Renderer, Material[]>();
        private Renderer[] _cachedRenderers;
        private const string StealthMaterialPath = "Prefabs/Player/VFX/NecromancerSkillPrefab/StealthMaterial";
        
        [Inject]
        public void Construct(IResourcesServices resourceService)
        {
            _resourceService = resourceService;
        }
        
        private NetworkVariable<bool> _isCharacterStealth = new NetworkVariable<bool>
            (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _changeMaterial =_resourceService.Load<Material>(StealthMaterialPath);
            Debug.Assert(_changeMaterial != null, $"Check : {StealthMaterialPath}");
            
            //루트에서 부터 안가도 될듯 어차피 이 스크립트는 최상단에 있는 스크립트라.
            _cachedRenderers = GetComponentsInChildren<Renderer>();
            
            _isCharacterStealth.OnValueChanged += OnStealthStateChanged;
            UpdateStealthVisual(_isCharacterStealth.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _isCharacterStealth.OnValueChanged -= OnStealthStateChanged;
        }

        
        //값을 가져올때 0을 초과하는 수를 가져오면
        //버퍼 컴포넌트도 같은 값을 가지고 있을테니
        //양수나 음수냐를 가지고 스위치로 썼음.
        [Rpc(SendTo.Server)]
        private void PlayerApplyModifiedRpc(float value)
        {
            if (value < 0 && _isCharacterStealth.Value == true)
            {
                _isCharacterStealth.Value = false;
            }
            else if (value > 0 && _isCharacterStealth.Value == false)
            {
                _isCharacterStealth.Value = true;
            }
        }
        // BufferManager가 호출
        public void ApplyModified(float value)
        {
            PlayerApplyModifiedRpc(value);
        }

        private void OnStealthStateChanged(bool previousValue, bool newValue)
        {
            UpdateStealthVisual(newValue);
        }

        // 실제 비주얼 처리
        private void UpdateStealthVisual(bool isStealth)
        {
            Debug.Assert(_changeMaterial != null, $"CachedRender is null" );
            
            if (isStealth == true)//은신이면
            {
                foreach (Renderer render in _cachedRenderers)
                {
                    if (_originalMaterialCache.ContainsKey(render) == false)
                    {
                        _originalMaterialCache.Add(render, render.sharedMaterials);
                    }//등록되지 않았다면 등록.

                    Material[] newMats = new Material[render.sharedMaterials.Length];
                    for (int i = 0; i < newMats.Length; i++)
                    {
                        newMats[i] = _changeMaterial;
                    }
                    render.materials = newMats;
                }
            }
            else
            {
                foreach (KeyValuePair<Renderer, Material[]> originalRenderDict in _originalMaterialCache)
                {
                    Renderer render = originalRenderDict.Key;
                    Material[] originalMats = originalRenderDict.Value;

                    if (render != null)
                    {
                        render.materials = originalMats;
                    }
                }
            }
            
        }

        //숨기가 false일때 타겟이 가능하고
        //숨기가 true일떄 타겟이 불가능함
        public bool IsTargetable => !_isCharacterStealth.Value;
    }

}