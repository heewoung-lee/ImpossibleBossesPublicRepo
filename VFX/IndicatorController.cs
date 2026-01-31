using System;
using System.Collections;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace VFX
{
    public class IndicatorController : MonoBehaviour, IIndicatorBahaviour
    {
        private IResourcesServices _resourcesServices;
        
        [Inject] 
        public void Construct(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }

        private const float Depth = 100f;
        //public int ID = 0; 1.4일 제거 내가 이걸 왜 넣엇는지 몰것음

        enum DecalProjectors
        {
            Circle,
            CircleBorder
        }

        // --- Editor Control Fields ---
        [Header("Editor Preview Settings")]
        [SerializeField, Range(0, 50f)] private float _editorRadius = 5f;
        [SerializeField, Range(0, 360f)] private float _editorArc = 360f;
        [SerializeField, Range(0, 360f)] private float _editorAngle = 0f;
        [SerializeField, Range(0, 1f)] private float _editorFillProgress = 0f;
        
        [Header("Colors")]
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private Color _fillColor = new Color(1, 1, 1, 0.5f);
        // -----------------------------

        private float _radius;
        private float _angle;
        private float _arc;
        private Vector3 _callerPosition;

        private DecalProjector _decalCircleProjector;
        private DecalProjector _decalCircleBorderProjector;

        private Action _doneIndicatorEvent;

        // Shader Property IDs
        private static readonly int ColorShaderID = Shader.PropertyToID("_Color");
        private static readonly int FillColorShaderID = Shader.PropertyToID("_FillColor");
        private static readonly int FillProgressShaderID = Shader.PropertyToID("_FillProgress");
        private static readonly int ArcShaderID = Shader.PropertyToID("_Arc");
        private static readonly int AngleShaderID = Shader.PropertyToID("_Angle");

        public float Radius
        {
            get => _radius;
            private set
            {
                _radius = Mathf.Max(value, 0f);
                UpdateDecalSize();
            }
        }

        public float Angle
        {
            get => _angle;
            private set
            {
                _angle = value;
                UpdateDecalMaterials();
            }
        }

        public float Arc
        {
            get => _arc;
            private set
            {
                _arc = Mathf.Clamp(value, 0f, 360f);
                UpdateDecalMaterials();
            }
        }

        public Vector3 CallerPosition
        {
            get => _callerPosition;
            private set
            {
                _callerPosition = value;
                transform.position = _callerPosition;
            }
        }

        protected void Awake()
        {
            InitProjectors();
            ReassignMaterials();
        }

        private void InitProjectors()
        {
            if (_decalCircleProjector == null)
                _decalCircleProjector = transform.Find(DecalProjectors.Circle.ToString())?.GetComponent<DecalProjector>();
            
            if (_decalCircleBorderProjector == null)
                _decalCircleBorderProjector = transform.Find(DecalProjectors.CircleBorder.ToString())?.GetComponent<DecalProjector>();
        }

        private void ReassignMaterials()
        {
            // 런타임에만 복제본(Instance)을 생성하여 사용
            if (_decalCircleProjector != null)
                _decalCircleProjector.material = new Material(_decalCircleProjector.material);

            if (_decalCircleBorderProjector != null)
                _decalCircleBorderProjector.material = new Material(_decalCircleBorderProjector.material);
        }

        // 사이즈 조절 로직 분리
        private void UpdateDecalSize()
        {
            if (_decalCircleProjector == null || _decalCircleBorderProjector == null) return;

            Vector3 currentSize;
            currentSize.x = _radius * 2;
            currentSize.y = _radius * 2;
            currentSize.z = Depth;

            _decalCircleProjector.size = currentSize;
            _decalCircleBorderProjector.size = currentSize;
        }

        // 머티리얼 속성 조절 로직 분리
        private void UpdateDecalMaterials()
        {
            if (_decalCircleProjector == null || _decalCircleBorderProjector == null) return;

            // Angle
            float normalizedAngle = Mathf.Repeat((_angle - 90) % 360, 360) / 360;
            
            // Arc
            float arcAngleNormalized = 1f - _arc / 360;

            ApplyMaterialFloat(AngleShaderID, normalizedAngle);
            ApplyMaterialFloat(ArcShaderID, arcAngleNormalized);
            
            // Editor나 초기화 시 색상 적용을 위해 추가
            ApplyMaterialColor(ColorShaderID, _color);
            ApplyMaterialColor(FillColorShaderID, _fillColor);
        }
        public void SetTargetingPreview(float radius, float arc = 360f)
        {
            StopAllCoroutines();      // 혹시 기존 코루틴 돌고 있으면 끊기
            _doneIndicatorEvent = null;

            Radius = radius;          // 여기서 UpdateDecalSize() 호출됨
            Arc = arc;                // 머티리얼 업데이트도 같이 됨

            UpdateDecalFillProgressProjector(1f); // 항상 꽉 찬 표시(원하면 0으로)
        }

        public void SetTargetingPosition(Vector3 pos)
        {
            CallerPosition = pos;     // transform.position 반영됨
        }
        private void ApplyMaterialFloat(int propID, float value)
        {
            if (_decalCircleProjector.material != null) _decalCircleProjector.material.SetFloat(propID, value);
            if (_decalCircleBorderProjector.material != null) _decalCircleBorderProjector.material.SetFloat(propID, value);
        }

        private void ApplyMaterialColor(int propID, Color color)
        {
            if (_decalCircleProjector.material != null) _decalCircleProjector.material.SetColor(propID, color);
            if (_decalCircleBorderProjector.material != null) _decalCircleBorderProjector.material.SetColor(propID, color);
        }

        private void UpdateDecalFillProgressProjector(float fillAmount)
        {
            ApplyMaterialFloat(FillProgressShaderID, fillAmount);
        }

        public void SetValue(float radius, float arc, Transform targetTr, float duration, Action indicatorDoneEvent = null)
        {
            // 실제 데이터 설정
            Radius = radius;
            Arc = arc;
            CallerPosition = targetTr.position;
            Angle = targetTr.eulerAngles.y;
            
            // 에디터 변수 동기화 (인스펙터에서도 보이게)
            _editorRadius = radius;
            _editorArc = arc;
            _editorAngle = Angle;

            _doneIndicatorEvent += indicatorDoneEvent;
            StartCoroutine(Play_Indicator(duration));
        }

        public void SetValue(float radius, float arc, Vector3 targetPos, float duration, Action indicatorDoneEvent = null)
        {
            Radius = radius;
            Arc = arc;
            CallerPosition = targetPos;

            _editorRadius = radius;
            _editorArc = arc;
            
            _doneIndicatorEvent += indicatorDoneEvent;
            StartCoroutine(Play_Indicator(duration));
        }

        private IEnumerator Play_Indicator(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float fillAmount = Mathf.Clamp01(elapsed / duration);
                
                // 에디터 변수도 업데이트 (디버깅용)
                _editorFillProgress = fillAmount; 
                UpdateDecalFillProgressProjector(fillAmount);
                
                yield return null;
            }
            _doneIndicatorEvent?.Invoke();
            _doneIndicatorEvent = null;
            
            UpdateDecalFillProgressProjector(0f);
            _editorFillProgress = 0f;
            
            _resourcesServices.DestroyObject(gameObject);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            //컴포넌트 찾기 (Awake가 안 불린 상태일 수 있음)
            InitProjectors();

            if (_decalCircleProjector == null || _decalCircleBorderProjector == null) return;

            _radius = Mathf.Max(_editorRadius, 0f);
            _angle = _editorAngle;
            _arc = Mathf.Clamp(_editorArc, 0f, 360f);

            //비주얼 업데이트 호출
            UpdateDecalSize();
            UpdateDecalMaterials();
            UpdateDecalFillProgressProjector(_editorFillProgress);
        }
#endif
    }
}