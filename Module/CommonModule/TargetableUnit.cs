using System.Collections.Generic;
using GameManagers.Target;
using Scene.CommonInstaller.InGameInstaller;
using UnityEngine;

namespace Module.CommonModule
{
    public class TargetableUnit : MonoBehaviour, ITargetInteractable
    {
        // 렌더러와 원래 머티리얼을 저장할 데이터
        private class RendererInfo
        {
            public Renderer renderer;
            public Material[] originalMaterials;
        }

        private List<RendererInfo> _renderers = new List<RendererInfo>();
        private bool _isHighlighted = false;

        private void Awake()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

            foreach (Renderer render in renderers)
            {
                // 파티클은 렌더러에서 제외
                if (render is ParticleSystemRenderer) continue;
                // 추후 UI나 스프라이트 등도 제외하고 싶다면 여기서 필터링

                _renderers.Add(new RendererInfo
                {
                    renderer = render,
                    originalMaterials = render.sharedMaterials // 원래 때깔 저장
                });
            }
        }

        public void SetHighlight(Material mat)
        {
            if (_isHighlighted) return;
            _isHighlighted = true;

            foreach (var info in _renderers)
            {
                if (info.renderer == null) continue;

                // 해당 렌더러의 모든 머티리얼 슬롯을 타겟매니저가 준 머테리얼로 교체
                Material[] newMats = new Material[info.originalMaterials.Length];
                for (int i = 0; i < newMats.Length; i++)
                {
                    newMats[i] = mat;
                }
                info.renderer.materials = newMats;
            }
        }

        public void RemoveHighlight()
        {
            if (!_isHighlighted) return;
            _isHighlighted = false;

            foreach (var info in _renderers)
            {
                if (info.renderer == null) continue;
                
                // 원래 머티리얼로 복구
                info.renderer.materials = info.originalMaterials;
            }
        }

        public GameObject GetGameObject() => this.gameObject;
    }
}