using System.Collections.Generic;
using Controller.CrowdControl;
using GameManagers.ResourcesExManagement;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule.Necromancer
{
    public class NecromancerSpecialModifier : PlayerCrowdControlNetworkReceiver, ITargetable
    {
        private const int StealthSpecialCode = 2;
        private const string StealthMaterialPath = "Prefabs/Player/VFX/NecromancerSkillPrefab/StealthMaterial";

        private IResourcesServices _resourceService;
        private Material _changeMaterial;
        private readonly Dictionary<Renderer, Material[]> _originalMaterialCache =
            new Dictionary<Renderer, Material[]>();
        private Renderer[] _cachedRenderers;

        private readonly NetworkVariable<bool> _isCharacterStealth = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        [Inject]
        public void ConstructNecromancer(IResourcesServices resourceService)
        {
            _resourceService = resourceService;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _changeMaterial = _resourceService.Load<Material>(StealthMaterialPath);
            Debug.Assert(_changeMaterial != null, $"Check : {StealthMaterialPath}");

            _cachedRenderers = GetComponentsInChildren<Renderer>();

            _isCharacterStealth.OnValueChanged += OnStealthStateChanged;
            UpdateStealthVisual(_isCharacterStealth.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _isCharacterStealth.OnValueChanged -= OnStealthStateChanged;
        }

        protected override void ApplyClassSpecial(int specialCode, bool isApply)
        {
            if (specialCode != StealthSpecialCode)
            {
                base.ApplyClassSpecial(specialCode, isApply);
                return;
            }

            SetStealthStateRpc(isApply);
        }

        [Rpc(SendTo.Server)]
        private void SetStealthStateRpc(bool isStealth)
        {
            _isCharacterStealth.Value = isStealth;
        }

        private void OnStealthStateChanged(bool previousValue, bool newValue)
        {
            UpdateStealthVisual(newValue);
        }

        private void UpdateStealthVisual(bool isStealth)
        {
            Debug.Assert(_changeMaterial != null, "CachedRender is null");

            if (isStealth)
            {
                foreach (Renderer render in _cachedRenderers)
                {
                    if (_originalMaterialCache.ContainsKey(render) == false)
                    {
                        _originalMaterialCache.Add(render, render.sharedMaterials);
                    }

                    Material[] newMats = new Material[render.sharedMaterials.Length];
                    for (int i = 0; i < newMats.Length; i++)
                    {
                        newMats[i] = _changeMaterial;
                    }

                    render.materials = newMats;
                }

                return;
            }

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

        public bool IsTargetable => !_isCharacterStealth.Value;
    }
}
