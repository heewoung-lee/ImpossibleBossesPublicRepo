using System;
using System.Collections;
using GameManagers.SoundManagement;
using GameManagers.PoolManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Util;
using Zenject;

namespace VFX
{
    public class NgoIndicatorController : NetworkBehaviourBase, IIndicatorBahaviour
    {
        public const ulong InvalidSpawnerBossNetworkObjectId = ulong.MaxValue;
        private const string IndicatorGaugeUpCueId = "IndicatorGaugeUpSFX";

        IResourcesServices _resourcesServices;
        private RelayManager _relayManager;

        [Inject]
        private void Construct(IResourcesServices resourcesServices, RelayManager relayManager)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
        }

        private const float Depth = 100f;

        enum DecalProjectors
        {
            Circle,
            CircleBorder
        }

        [Header("Editor Preview Settings")]
        [SerializeField, Range(0, 50f)] private float _editorRadius = 5f;
        [SerializeField, Range(0, 360f)] private float _editorArc = 360f;
        [SerializeField, Range(0, 360f)] private float _editorAngle = 0f;
        [SerializeField, Range(0, 1f)] private float _editorFillProgress = 0f;

        [Header("Colors")] 
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private Color _fillColor = new Color(1, 1, 1, 0.5f);
        [SerializeField] private Color _borderColor = Color.white;

        private Action _onIndicatorDone;
        public event Action OnIndicatorDone
        {
            add { UniqueEventRegister.AddSingleEvent(ref _onIndicatorDone, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _onIndicatorDone, value); }
        }

        private NetworkVariable<float> _radius = new NetworkVariable<float>
            (0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<float> _angle = new NetworkVariable<float>
            (0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<float> _arc = new NetworkVariable<float>
            (0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<Vector3> _currentPosition = new NetworkVariable<Vector3>
            (Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<float> _durationTime = new NetworkVariable<float>
            (0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private double _startServerTime;

        private DecalProjector _decalCircleProjector;
        private DecalProjector _decalCircleBorderProjector;

        public ulong SpawnerBossNetworkObjectId { get; private set; } = InvalidSpawnerBossNetworkObjectId;
        public bool HasValidSpawnerBossNetworkObjectId => SpawnerBossNetworkObjectId != InvalidSpawnerBossNetworkObjectId;

        public float Radius
        {
            get => _radius.Value;
            private set
            {
                if (IsHost == false) return;
                _radius.Value = Mathf.Max(value, 0f);
            }
        }
        public float Angle
        {
            get => _angle.Value;
            private set
            {
                if (IsHost == false) return;
                _angle.Value = value;
            }
        }
        public float Arc
        {
            get => _arc.Value;
            private set
            {
                if (IsHost == false) return;
                _arc.Value = Mathf.Clamp(value, 0f, 360f);
            }
        }
        public Vector3 Position
        {
            get => _currentPosition.Value;
            private set
            {
                if (IsHost == false) return;
                _currentPosition.Value = value;
            }
        }
        public float DurationTime
        {
            get => _durationTime.Value;
            set
            {
                if (IsHost == false) return;
                _durationTime.Value = value;
            }
        }

        public void SetSpawnerBossNetworkObjectId(ulong spawnerBossNetworkObjectId)
        {
            SpawnerBossNetworkObjectId = spawnerBossNetworkObjectId;
        }

        public float NormalizedProgress
        {
            get
            {
                if (_relayManager == null || DurationTime <= Mathf.Epsilon)
                {
                    return 0f;
                }

                double elapsedTime = _relayManager.NetworkManagerEx.ServerTime.Time - _startServerTime;
                return Mathf.Clamp01((float)(elapsedTime / DurationTime));
            }
        }

        private static readonly int ColorShaderID = Shader.PropertyToID("_Color");
        private static readonly int FillColorShaderID = Shader.PropertyToID("_FillColor");
        private static readonly int FillProgressShaderID = Shader.PropertyToID("_FillProgress");
        private static readonly int ArcShaderID = Shader.PropertyToID("_Arc");
        private static readonly int AngleShaderID = Shader.PropertyToID("_Angle");

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            SubscribeValueEvents();

            if (IsHost)
            {
                _currentPosition.Value = transform.position;
            }
            // 초기 동기화
            UpdateVisuals_Position(Position);
            UpdateVisuals_Size(Radius);
            UpdateVisuals_MaterialProps(Angle, Arc);
        }

        public override void OnNetworkDespawn()
        {
            StopGaugeUpSfx();
            UnsubscribeValueEvents();
        }

        protected override void OnDisableInit()
        {
            StopGaugeUpSfx();
        }

        private void SubscribeValueEvents()
        {
            _radius.OnValueChanged += OnRadiusValueChanged;
            _arc.OnValueChanged += OnArcValueChagned;
            _angle.OnValueChanged += OnAngleValueChanged;
            _currentPosition.OnValueChanged += OnCallerPositionChanged;
        }
        private void UnsubscribeValueEvents()
        {
            _radius.OnValueChanged -= OnRadiusValueChanged;
            _arc.OnValueChanged -= OnArcValueChagned;
            _angle.OnValueChanged -= OnAngleValueChanged;
            _currentPosition.OnValueChanged -= OnCallerPositionChanged;
        }
        
        
        private void OnCallerPositionChanged(Vector3 previousValue, Vector3 newValue) => UpdateVisuals_Position(newValue);
        private void OnRadiusValueChanged(float previousValue, float newValue) => UpdateVisuals_Size(newValue);
        
        private void OnArcValueChagned(float previousValue, float newValue) => UpdateVisuals_MaterialProps(Angle, newValue);
        private void OnAngleValueChanged(float previousValue, float newValue) => UpdateVisuals_MaterialProps(newValue, Arc);


        private void UpdateVisuals_Position(Vector3 pos)
        {
            transform.position = pos;
        }

        private void UpdateVisuals_Size(float r)
        {
            if (_decalCircleProjector == null || _decalCircleBorderProjector == null) return;

            Vector3 currentSize;
            currentSize.x = r * 2;
            currentSize.y = r * 2;
            currentSize.z = Depth;

            _decalCircleProjector.size = currentSize;
            _decalCircleBorderProjector.size = currentSize;
        }

        private void UpdateVisuals_MaterialProps(float angleVal, float arcVal)
        {
            if (_decalCircleProjector == null || _decalCircleBorderProjector == null) return;

            float normalizedAngle = Mathf.Repeat((angleVal - 90) % 360, 360) / 360;
            float arcAngleNormalized = 1f - arcVal / 360;

            if (_decalCircleProjector.material != null)
            {
                _decalCircleProjector.material.SetFloat(AngleShaderID, normalizedAngle);
                _decalCircleProjector.material.SetFloat(ArcShaderID, arcAngleNormalized);
                _decalCircleProjector.material.SetColor(ColorShaderID, _color);
                if (_decalCircleProjector.material.HasProperty(FillColorShaderID))
                {
                    _decalCircleProjector.material.SetColor(FillColorShaderID, _fillColor);
                }
            }

            if (_decalCircleBorderProjector.material != null)
            {
                if (_decalCircleBorderProjector.material.HasProperty(AngleShaderID))
                {
                    _decalCircleBorderProjector.material.SetFloat(AngleShaderID, normalizedAngle);
                }

                if (_decalCircleBorderProjector.material.HasProperty(ArcShaderID))
                {
                    _decalCircleBorderProjector.material.SetFloat(ArcShaderID, arcAngleNormalized);
                }

                _decalCircleBorderProjector.material.SetColor(ColorShaderID, _borderColor);
                if (_decalCircleBorderProjector.material.HasProperty(FillColorShaderID))
                {
                    _decalCircleBorderProjector.material.SetColor(FillColorShaderID, _borderColor);
                }
            }
        }

        private void UpdateDecalFillProgressProjector(float fillAmount)
        {
            if (_decalCircleProjector != null && _decalCircleProjector.material != null
                                             && _decalCircleProjector.material.HasProperty(FillProgressShaderID))
                _decalCircleProjector.material.SetFloat(FillProgressShaderID, fillAmount);
            
            if (_decalCircleBorderProjector != null && _decalCircleBorderProjector.material != null
                                                    && _decalCircleBorderProjector.material.HasProperty(FillProgressShaderID))
                _decalCircleBorderProjector.material.SetFloat(FillProgressShaderID, fillAmount);
        }


        protected override void StartInit()
        {
        }

        protected override void AwakeInit()
        {
            Bind<DecalProjector>(typeof(DecalProjectors));
            _decalCircleProjector = Get<DecalProjector>((int)DecalProjectors.Circle);
            _decalCircleBorderProjector = Get<DecalProjector>((int)DecalProjectors.CircleBorder);
            
            GetComponent<Poolable>().WorldPositionStays = false;
            
            if (TryGetComponent(out NgoPoolingInitializeBase initbase))
            {
                initbase.PoolObjectReleaseEvent += ReleseProjector;
            }
            ReassignMaterials();
        }

        // 에디터와 런타임 모두 프로젝터를 찾을 수 있도록 헬퍼 메서드 추가
        private void EnsureProjectorsRef()
        {
            if (_decalCircleProjector == null)
                _decalCircleProjector = transform.Find(DecalProjectors.Circle.ToString())?.GetComponent<DecalProjector>();
            
            if (_decalCircleBorderProjector == null)
                _decalCircleBorderProjector = transform.Find(DecalProjectors.CircleBorder.ToString())?.GetComponent<DecalProjector>();
        }

        private void ReassignMaterials()
        {
            if (!Application.isPlaying) return;

            if (_decalCircleProjector != null)
                _decalCircleProjector.material = new Material(_decalCircleProjector.material);

            if (_decalCircleBorderProjector != null)
                _decalCircleBorderProjector.material = new Material(_decalCircleBorderProjector.material);
        }

        public void SetValue(float radius, float arc, Transform targetTr, float durationTime, Action indicatorDoneEvent = null)
        {
            Radius = radius;
            Arc = arc;
            Position = targetTr.position;
            Angle = targetTr.eulerAngles.y;

            _editorRadius = radius;
            _editorArc = arc;
            _editorAngle = Angle;

            OnIndicatorDone += indicatorDoneEvent;
            float clampDuration = Mathf.Max(durationTime, 0.1f);
            DurationTime = clampDuration;
            StartProjectorCoroutineRpc(clampDuration, _relayManager.NetworkManagerEx.ServerTime.Time);
        }

        public void SetValue(float radius, float arc, Vector3 targetPos, float durationTime, Action indicatorDoneEvent = null)
        {
            Radius = radius;
            Arc = arc;
            Position = targetPos;
            
            _editorRadius = radius;
            _editorArc = arc;

            OnIndicatorDone += indicatorDoneEvent;
            
            float clampDuration = Mathf.Max(durationTime, 0.1f);
            DurationTime = clampDuration;
            StartProjectorCoroutineRpc(clampDuration, _relayManager.NetworkManagerEx.ServerTime.Time);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void StartProjectorCoroutineRpc(float durationTime, double startServerTime)
        {
            _startServerTime = startServerTime;
            PlayGaugeUpSfx(durationTime);
            StartCoroutine(Play_Indicator(durationTime));
        }

        private void PlayGaugeUpSfx(float durationTime)
        {
            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder) == false)
            {
                return;
            }

            if (soundPlayerBinder.TryGetClip(IndicatorGaugeUpCueId, out AudioClip clip) == false)
            {
                return;
            }

            float pitch = clip.length / Mathf.Max(durationTime, 0.01f);
            soundPlayerBinder.StopLoop(IndicatorGaugeUpCueId);
            soundPlayerBinder.PlayLoop(IndicatorGaugeUpCueId, pitch);
        }

        private void StopGaugeUpSfx()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder))
            {
                soundPlayerBinder.StopLoop(IndicatorGaugeUpCueId);
            }
        }

        private IEnumerator Play_Indicator(float duration)
        {
            float elapsed = 0f;
            double nowTime = _relayManager.NetworkManagerEx.ServerTime.Time;
            while (elapsed < duration)
            {
                double currentNetTime = _relayManager.NetworkManagerEx.ServerTime.Time;
                double deltaTime = currentNetTime - nowTime;
                nowTime = currentNetTime;

                elapsed += (float)deltaTime;
                
                float fillAmount = Mathf.Clamp01(elapsed / duration);
                
                _editorFillProgress = fillAmount;
                UpdateDecalFillProgressProjector(fillAmount);
                
                yield return null;
            }
            _onIndicatorDone?.Invoke();
            StopGaugeUpSfx();
            UpdateDecalFillProgressProjector(0f); 
            _editorFillProgress = 0f;
            
            _resourcesServices.DestroyObject(gameObject);
        }

        private void ReleseProjector()
        {
            StopGaugeUpSfx();
            Position = Vector3.zero;
            Radius = 0f;
            Arc = 0f;
            Angle = 0f;
            SpawnerBossNetworkObjectId = InvalidSpawnerBossNetworkObjectId;
            
            UpdateDecalFillProgressProjector(0f);
            transform.position = Vector3.zero;
            _startServerTime = 0d;
            _onIndicatorDone = null;
        }

      
#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureProjectorsRef();

            if (_decalCircleProjector == null || _decalCircleBorderProjector == null) return;

            //값 갱신
            UpdateVisuals_Size(_editorRadius);
            UpdateVisuals_MaterialProps(_editorAngle, _editorArc);
            UpdateDecalFillProgressProjector(_editorFillProgress);
        }
#endif
    }
}
