using System;
using System.Collections;
using System.Collections.Generic;
using GameManagers.PoolManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Util;
using Zenject;
using Vector2 = UnityEngine.Vector2;

namespace VFX
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class NgoArrowIndicatorController : NetworkBehaviourBase, IIndicatorBahaviour
    {
        public const ulong InvalidSpawnerBossNetworkObjectId = ulong.MaxValue;
        private const string IndicatorGaugeUpCueId = "IndicatorGaugeUpSFX";

        private sealed class LineVisual
        {
            public Transform Root;
            public DecalProjector HeadProjector;
            public DecalProjector BodyProjector;
        }

        private const string TemplateBodyName = "Body";
        private const string TemplateHeadName = "Head";
        private const string GeneratedRootName = "__GeneratedLines";
        private const string LineRootPrefix = "ArrowLine_";
        private const float ArrowZDisplacement = 2.9835f;
        private const float YPosition = 0.15f;
        private const float MinDuration = 0.1f;
        private const int MinLineCount = 1;

        private static readonly int ColorShaderID = Shader.PropertyToID("_Color");
        private static readonly int FillColorShaderID = Shader.PropertyToID("_FillColor");
        private static readonly int FillProgressShaderID = Shader.PropertyToID("_FillProgress");
        private static readonly int FadeAmountShaderID = Shader.PropertyToID("_FadeAmount");

        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;

        [Header("Editor Preview Settings")]
        [SerializeField, Min(MinLineCount)] private int _editorLineCount = 6;
        [SerializeField, Min(0f)] private float _editorRadius = 3f;
        [SerializeField, Range(0f, 360f)] private float _editorArc = 45f;
        [SerializeField, Range(0f, 1f)] private float _editorFillProgress;
        [SerializeField, Range(0f, 1f)] private float _editorFadeAmount = 0.33f;
        [SerializeField, Min(0f)] private float _editorWidth = 0.46f;
        [SerializeField, Min(0f)] private float _editorBodyLength = 3f;
        [SerializeField, Min(0f)] private float _editorDepth = 10f;
        [SerializeField, Range(0f, 360f)] private float _editorAngle;

        [Header("Colors")]
        [SerializeField] private Color _color = new Color(1f, 0.90588236f, 0f, 1f);
        [SerializeField] private Color _fillColor = new Color(1f, 0.043137256f, 0f, 1f);

        [Header("Template Materials")]
        [SerializeField] private Material _headMaterial;
        [SerializeField] private Material _bodyMaterial;

        [Header("Visual Scale")]
        [SerializeField, Min(0.01f)] private float _headWidthScale = 1.25f;
        [SerializeField, Min(0.01f)] private float _headLengthScale = 1.1f;

        [Header("Visual Alignment")]
        [SerializeField] private float _bodyLocalZOffset;
        [SerializeField] private float _headLocalZOffset = -0.12f;

        private readonly List<LineVisual> _lineVisuals = new List<LineVisual>();
        private Transform _templateBody;
        private Transform _templateHead;
        private Transform _generatedRoot;
        private Action _onIndicatorDone;
        private bool _editorPreviewDirty = true;
        private Material _appliedHeadMaterial;
        private Material _appliedBodyMaterial;
        private Vector3 _templateHeadProjectorSize = Vector3.one;
        private Vector3 _templateBodyProjectorSize = Vector3.one;
        private float _templateHeadLocalZ = 0f;
        private float _templateBodyLocalZ = 0f;

        private readonly NetworkVariable<float> _radius = new(
            3f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _angle = new(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _arc = new(
            45f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _width = new(
            0.46f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _bodyLength = new(
            3f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _depth = new(
            10f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _fadeAmount = new(
            0.33f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _lineCount = new(
            6,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<Vector3> _position = new(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<Color> _networkColor = new(
            Color.white,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<Color> _networkFillColor = new(
            Color.red,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public ulong SpawnerBossNetworkObjectId { get; private set; } = InvalidSpawnerBossNetworkObjectId;
        public bool HasValidSpawnerBossNetworkObjectId => SpawnerBossNetworkObjectId != InvalidSpawnerBossNetworkObjectId;

        public event Action OnIndicatorDone
        {
            add { UniqueEventRegister.AddSingleEvent(ref _onIndicatorDone, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _onIndicatorDone, value); }
        }

        public float Radius
        {
            get => _radius.Value;
            private set
            {
                if (IsHost == false)
                {
                    return;
                }

                _radius.Value = Mathf.Max(value, 0f);
            }
        }

        public float Angle
        {
            get => _angle.Value;
            private set
            {
                if (IsHost == false)
                {
                    return;
                }

                _angle.Value = value;
            }
        }

        public float Arc
        {
            get => _arc.Value;
            private set
            {
                if (IsHost == false)
                {
                    return;
                }

                _arc.Value = Mathf.Clamp(value, 0f, 360f);
            }
        }

        public Vector3 Position
        {
            get => _position.Value;
            private set
            {
                if (IsHost == false)
                {
                    return;
                }

                _position.Value = value;
            }
        }

        public float Width
        {
            get => _width.Value;
            private set
            {
                if (IsHost == false)
                {
                    return;
                }

                _width.Value = Mathf.Max(value, 0f);
            }
        }

        public float FadeAmount
        {
            get => _fadeAmount.Value;
            private set
            {
                if (IsHost == false)
                {
                    return;
                }

                _fadeAmount.Value = Mathf.Clamp01(value);
            }
        }

        public float BodyLength
        {
            get => _bodyLength.Value;
            private set
            {
                if (IsHost == false)
                {
                    return;
                }

                _bodyLength.Value = Mathf.Max(value, 0f);
            }
        }

        public float Depth
        {
            get => _depth.Value;
            private set
            {
                if (IsHost == false)
                {
                    return;
                }

                _depth.Value = Mathf.Max(value, 0f);
            }
        }

        public int LineCount
        {
            get => _lineCount.Value;
            private set
            {
                if (IsHost == false)
                {
                    return;
                }

                _lineCount.Value = Mathf.Max(value, MinLineCount);
            }
        }

        [Inject]
        private void Construct(IResourcesServices resourcesServices, RelayManager relayManager)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
        }

        public void SetSpawnerBossNetworkObjectId(ulong spawnerBossNetworkObjectId)
        {
            SpawnerBossNetworkObjectId = spawnerBossNetworkObjectId;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SubscribeValueEvents();
            EnsureTemplateReferences();
            EnsureGeneratedRoot();
            DisableTemplateObjects();
            DisableLegacyGeneratedChildren();
            EnsureLineVisualCapacity(LineCount);

            if (IsHost)
            {
                _radius.Value = Mathf.Max(_editorRadius, 0f);
                _angle.Value = _editorAngle;
                _arc.Value = Mathf.Clamp(_editorArc, 0f, 360f);
                _width.Value = Mathf.Max(_editorWidth, 0f);
                _bodyLength.Value = Mathf.Max(_editorBodyLength, 0f);
                _depth.Value = Mathf.Max(_editorDepth, 0f);
                _fadeAmount.Value = Mathf.Clamp01(_editorFadeAmount);
                _lineCount.Value = Mathf.Max(_editorLineCount, MinLineCount);
                _position.Value = transform.position;
                _networkColor.Value = _color;
                _networkFillColor.Value = _fillColor;
            }

            ApplyCurrentState(0f);
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

        public void SetColor(Color color, Color fillColor)
        {
            if (IsHost == false)
            {
                return;
            }

            _color = color;
            _fillColor = fillColor;
            _networkColor.Value = color;
            _networkFillColor.Value = fillColor;
            _editorPreviewDirty = true;
        }

        public void SetWidth(float width)
        {
            Width = width;
            if (Application.isPlaying == false)
            {
                _editorWidth = width;
            }

            _editorPreviewDirty = true;
        }

        public void SetFadeAmount(float fadeAmount)
        {
            FadeAmount = fadeAmount;
            if (Application.isPlaying == false)
            {
                _editorFadeAmount = fadeAmount;
            }

            _editorPreviewDirty = true;
        }

        public void SetBodyLength(float bodyLength)
        {
            BodyLength = bodyLength;
            if (Application.isPlaying == false)
            {
                _editorBodyLength = bodyLength;
            }

            _editorPreviewDirty = true;
        }

        public void SetDepth(float depth)
        {
            Depth = depth;
            if (Application.isPlaying == false)
            {
                _editorDepth = depth;
            }

            _editorPreviewDirty = true;
        }

        public void SetLineCount(int count)
        {
            LineCount = count;
            if (Application.isPlaying == false)
            {
                _editorLineCount = count;
            }

            _editorPreviewDirty = true;
        }

        public void SetDirection(float angle)
        {
            Angle = angle;
            if (Application.isPlaying == false)
            {
                _editorAngle = angle;
            }

            _editorPreviewDirty = true;
        }

        public void SetValue(float radius, float arc, Transform targetTr, float duration, Action indicatorDoneEvent = null)
        {
            if (targetTr == null)
            {
                UtilDebug.LogError($"[{nameof(NgoArrowIndicatorController)}] targetTr is null.");
                return;
            }

            SetValue(radius, arc, targetTr.position, duration, indicatorDoneEvent);
            SetDirection(targetTr.eulerAngles.y);
        }

        public void SetValue(float radius, float arc, Vector3 targetPos, float duration, Action indicatorDoneEvent = null)
        {
            Radius = radius;
            Arc = arc;
            Position = targetPos;
            BodyLength = radius;

            if (Application.isPlaying == false)
            {
                _editorRadius = radius;
                _editorArc = arc;
                _editorBodyLength = radius;
            }

            _editorPreviewDirty = true;

            OnIndicatorDone += indicatorDoneEvent;
            StartProjectorCoroutineRpc(Mathf.Max(duration, MinDuration));
        }

        public void PlayWithCurrentShape(Transform targetTr, float duration, Action indicatorDoneEvent = null)
        {
            if (targetTr == null)
            {
                UtilDebug.LogError($"[{nameof(NgoArrowIndicatorController)}] targetTr is null.");
                return;
            }

            SetDirection(targetTr.eulerAngles.y);
            PlayWithCurrentShape(targetTr.position, duration, indicatorDoneEvent);
        }

        public void PlayWithCurrentShape(Vector3 targetPos, float duration, Action indicatorDoneEvent = null)
        {
            Position = targetPos;
            _editorPreviewDirty = true;

            OnIndicatorDone += indicatorDoneEvent;
            StartProjectorCoroutineRpc(Mathf.Max(duration, MinDuration));
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void StartProjectorCoroutineRpc(float duration)
        {
            StopAllCoroutines();
            PlayGaugeUpSfx(duration);
            StartCoroutine(PlayIndicator(duration));
        }

        private void PlayGaugeUpSfx(float duration)
        {
            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder) == false)
            {
                return;
            }

            if (soundPlayerBinder.TryGetClip(IndicatorGaugeUpCueId, out AudioClip clip) == false)
            {
                return;
            }

            float pitch = clip.length / Mathf.Max(duration, 0.01f);
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

        protected override void AwakeInit()
        {
            if (TryGetComponent(out Poolable poolable) == true)
            {
                poolable.WorldPositionStays = false;
            }

            if (TryGetComponent(out NgoPoolingInitializeBase initBase) == true)
            {
                initBase.PoolObjectReleaseEvent += ReleaseIndicator;
            }

            EnsureTemplateReferences();
            EnsureGeneratedRoot();
            DisableTemplateObjects();
            DisableLegacyGeneratedChildren();
            EnsureLineVisualCapacity(Mathf.Max(_editorLineCount, MinLineCount));
            ApplyCurrentState(_editorFillProgress);
        }

        protected override void StartInit()
        {
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_editorPreviewDirty == false && EditorPreviewNeedsRefresh() == false)
            {
                return;
            }

            RebuildEditorPreview();
        }

        private void SubscribeValueEvents()
        {
            _radius.OnValueChanged += OnShapeChanged;
            _arc.OnValueChanged += OnShapeChanged;
            _width.OnValueChanged += OnShapeChanged;
            _bodyLength.OnValueChanged += OnShapeChanged;
            _depth.OnValueChanged += OnShapeChanged;
            _fadeAmount.OnValueChanged += OnShapeChanged;
            _lineCount.OnValueChanged += OnLineCountChanged;
            _position.OnValueChanged += OnPositionChanged;
            _angle.OnValueChanged += OnAngleChanged;
            _networkColor.OnValueChanged += OnColorChanged;
            _networkFillColor.OnValueChanged += OnColorChanged;
        }

        private void UnsubscribeValueEvents()
        {
            _radius.OnValueChanged -= OnShapeChanged;
            _arc.OnValueChanged -= OnShapeChanged;
            _width.OnValueChanged -= OnShapeChanged;
            _bodyLength.OnValueChanged -= OnShapeChanged;
            _depth.OnValueChanged -= OnShapeChanged;
            _fadeAmount.OnValueChanged -= OnShapeChanged;
            _lineCount.OnValueChanged -= OnLineCountChanged;
            _position.OnValueChanged -= OnPositionChanged;
            _angle.OnValueChanged -= OnAngleChanged;
            _networkColor.OnValueChanged -= OnColorChanged;
            _networkFillColor.OnValueChanged -= OnColorChanged;
        }

        private void OnShapeChanged<T>(T previousValue, T newValue)
        {
            _editorPreviewDirty = true;
            ApplyCurrentState(0f);
        }

        private void OnLineCountChanged(int previousValue, int newValue)
        {
            _editorPreviewDirty = true;
            EnsureLineVisualCapacity(newValue);
            ApplyCurrentState(0f);
        }

        private void OnPositionChanged(Vector3 previousValue, Vector3 newValue)
        {
            _editorPreviewDirty = true;
            UpdateTransformPositionAndRotation();
        }

        private void OnAngleChanged(float previousValue, float newValue)
        {
            _editorPreviewDirty = true;
            UpdateTransformPositionAndRotation();
        }

        private void OnColorChanged<T>(T previousValue, T newValue)
        {
            _editorPreviewDirty = true;
            ApplyCurrentState(0f);
        }

        private void ApplyCurrentState(float fillProgress)
        {
            UpdateTransformPositionAndRotation();
            EnsureLineVisualCapacity(Application.isPlaying ? LineCount : Mathf.Max(_editorLineCount, MinLineCount));
            UpdateLineVisuals(fillProgress >= 0f
                ? fillProgress
                : Application.isPlaying ? 0f : _editorFillProgress);
            _editorPreviewDirty = false;
        }

        private void EnsureTemplateReferences()
        {
            if (_templateBody == null)
            {
                _templateBody = transform.Find(TemplateBodyName);
            }

            if (_templateHead == null)
            {
                _templateHead = transform.Find(TemplateHeadName);
            }

            CacheTemplateGeometry();
            CacheTemplateMaterialsIfNeeded();
            ApplyTemplateMaterials();
        }

        private void EnsureGeneratedRoot()
        {
            if (_generatedRoot != null)
            {
                _generatedRoot.gameObject.SetActive(true);
                return;
            }

            Transform found = transform.Find(GeneratedRootName);
            if (found != null)
            {
                _generatedRoot = found;
                _generatedRoot.gameObject.SetActive(true);
                return;
            }

            GameObject generatedRoot = new GameObject(GeneratedRootName);
            generatedRoot.transform.SetParent(transform, false);
            generatedRoot.SetActive(true);
            _generatedRoot = generatedRoot.transform;
        }

        private bool EditorPreviewNeedsRefresh()
        {
            if (_generatedRoot == null)
            {
                return true;
            }

            if (_generatedRoot.gameObject.activeSelf == false)
            {
                return true;
            }

            if (_generatedRoot.childCount != Mathf.Max(_editorLineCount, MinLineCount))
            {
                return true;
            }

            return false;
        }

        private void RebuildEditorPreview()
        {
            EnsureTemplateReferences();
            EnsureGeneratedRoot();
            DisableTemplateObjects();
            DisableLegacyGeneratedChildren();
            EnsureLineVisualCapacity(Mathf.Max(_editorLineCount, MinLineCount));
            RefreshGeneratedLineMaterials();
            ApplyCurrentState(_editorFillProgress);
        }

        private void EnsureLineVisualCapacity(int desiredCount)
        {
            desiredCount = Mathf.Max(desiredCount, MinLineCount);

            if (_generatedRoot != null && _generatedRoot.gameObject.activeSelf == false)
            {
                _generatedRoot.gameObject.SetActive(true);
            }

            RebuildExistingLineVisuals();

            while (_lineVisuals.Count < desiredCount)
            {
                LineVisual visual = CreateLineVisual(_lineVisuals.Count);
                if (visual == null)
                {
                    return;
                }

                _lineVisuals.Add(visual);
            }

            for (int i = 0; i < _lineVisuals.Count; i++)
            {
                if (_lineVisuals[i] == null || _lineVisuals[i].Root == null)
                {
                    continue;
                }

                _lineVisuals[i].Root.gameObject.SetActive(i < desiredCount);
                _lineVisuals[i].Root.name = $"{LineRootPrefix}{i}";
            }
        }

        private void RebuildExistingLineVisuals()
        {
            if (_generatedRoot == null)
            {
                return;
            }

            _lineVisuals.Clear();

            for (int i = 0; i < _generatedRoot.childCount; i++)
            {
                Transform child = _generatedRoot.GetChild(i);
                Transform headTr = child.Find(TemplateHeadName);
                Transform bodyTr = child.Find(TemplateBodyName);

                if (headTr == null || bodyTr == null)
                {
                    continue;
                }

                if (headTr.TryGetComponent(out DecalProjector headProjector) == false)
                {
                    continue;
                }

                if (bodyTr.TryGetComponent(out DecalProjector bodyProjector) == false)
                {
                    continue;
                }

                _lineVisuals.Add(new LineVisual
                {
                    Root = child,
                    HeadProjector = headProjector,
                    BodyProjector = bodyProjector
                });

                SyncProjectorMaterials(headProjector, bodyProjector);
            }
        }

        private LineVisual CreateLineVisual(int index)
        {
            if (_templateBody == null || _templateHead == null || _generatedRoot == null)
            {
                UtilDebug.LogError($"[{nameof(NgoArrowIndicatorController)}] Arrow template references are missing.");
                return null;
            }

            GameObject root = new GameObject($"{LineRootPrefix}{index}");
            root.transform.SetParent(_generatedRoot, false);

            GameObject headInstance = Instantiate(_templateHead.gameObject, root.transform, false);
            headInstance.name = TemplateHeadName;
            headInstance.SetActive(true);

            GameObject bodyInstance = Instantiate(_templateBody.gameObject, root.transform, false);
            bodyInstance.name = TemplateBodyName;
            bodyInstance.SetActive(true);

            DecalProjector headProjector = headInstance.GetComponent<DecalProjector>();
            DecalProjector bodyProjector = bodyInstance.GetComponent<DecalProjector>();

            if (headProjector == null || bodyProjector == null)
            {
                UtilDebug.LogError($"[{nameof(NgoArrowIndicatorController)}] Template Body/Head must have DecalProjector.");
                return null;
            }

            SyncProjectorMaterials(headProjector, bodyProjector, true);

            return new LineVisual
            {
                Root = root.transform,
                HeadProjector = headProjector,
                BodyProjector = bodyProjector
            };
        }

        private static Material CloneMaterial(Material source)
        {
            if (source == null)
            {
                return null;
            }

            Material material = new Material(source);
            if (Application.isPlaying == false)
            {
                material.hideFlags = HideFlags.HideAndDontSave;
            }

            return material;
        }

        private void SyncProjectorMaterials(
            DecalProjector headProjector,
            DecalProjector bodyProjector,
            bool forceReplace = false)
        {
            EnsureProjectorMaterial(headProjector, GetHeadTemplateMaterial(), forceReplace);
            EnsureProjectorMaterial(bodyProjector, GetBodyTemplateMaterial(), forceReplace);
        }

        private static void EnsureProjectorMaterial(
            DecalProjector projector,
            Material templateMaterial,
            bool forceReplace)
        {
            if (projector == null || templateMaterial == null)
            {
                return;
            }

            if (forceReplace == false && NeedsMaterialRepair(projector.material) == false)
            {
                return;
            }

            projector.material = CloneMaterial(templateMaterial);
        }

        private static bool NeedsMaterialRepair(Material material)
        {
            if (material == null)
            {
                return true;
            }

            return material.name == "Decal" || material.name.StartsWith("Decal ");
        }

        private void CacheTemplateMaterialsIfNeeded()
        {
            if (_headMaterial == null &&
                _templateHead != null &&
                _templateHead.TryGetComponent(out DecalProjector headProjector))
            {
                _headMaterial = headProjector.material;
            }

            if (_bodyMaterial == null &&
                _templateBody != null &&
                _templateBody.TryGetComponent(out DecalProjector bodyProjector))
            {
                _bodyMaterial = bodyProjector.material;
            }
        }

        private void ApplyTemplateMaterials()
        {
            if (_templateHead != null &&
                _headMaterial != null &&
                _templateHead.TryGetComponent(out DecalProjector headProjector))
            {
                headProjector.material = _headMaterial;
            }

            if (_templateBody != null &&
                _bodyMaterial != null &&
                _templateBody.TryGetComponent(out DecalProjector bodyProjector))
            {
                bodyProjector.material = _bodyMaterial;
            }
        }

        private void RefreshGeneratedLineMaterials()
        {
            RebuildExistingLineVisuals();

            foreach (LineVisual lineVisual in _lineVisuals)
            {
                if (lineVisual == null)
                {
                    continue;
                }

                SyncProjectorMaterials(lineVisual.HeadProjector, lineVisual.BodyProjector, true);
            }
        }

        private bool HaveTemplateMaterialsChanged()
        {
            return _appliedHeadMaterial != _headMaterial || _appliedBodyMaterial != _bodyMaterial;
        }

        private void CacheAppliedTemplateMaterials()
        {
            _appliedHeadMaterial = _headMaterial;
            _appliedBodyMaterial = _bodyMaterial;
        }

        private Material GetHeadTemplateMaterial()
        {
            if (_headMaterial != null)
            {
                return _headMaterial;
            }

            if (_templateHead != null && _templateHead.TryGetComponent(out DecalProjector headProjector))
            {
                return headProjector.material;
            }

            return null;
        }

        private Material GetBodyTemplateMaterial()
        {
            if (_bodyMaterial != null)
            {
                return _bodyMaterial;
            }

            if (_templateBody != null && _templateBody.TryGetComponent(out DecalProjector bodyProjector))
            {
                return bodyProjector.material;
            }

            return null;
        }

        private void DisableTemplateObjects()
        {
            if (_templateBody != null)
            {
                _templateBody.gameObject.SetActive(false);
            }

            if (_templateHead != null)
            {
                _templateHead.gameObject.SetActive(false);
            }
        }

        private void DisableLegacyGeneratedChildren()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child == _templateBody || child == _templateHead || child == _generatedRoot)
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        private void UpdateTransformPositionAndRotation()
        {
            if (Application.isPlaying)
            {
                transform.position = Position;
                transform.rotation = Quaternion.Euler(0f, Angle, 0f);
                return;
            }

            transform.rotation = Quaternion.Euler(0f, _editorAngle, 0f);
        }

        private void UpdateLineVisuals(float fillProgress)
        {
            float radius = Application.isPlaying ? Radius : _editorRadius;
            float arc = Application.isPlaying ? Arc : _editorArc;
            float width = Application.isPlaying ? Width : _editorWidth;
            float bodyLength = Application.isPlaying ? BodyLength : _editorBodyLength;
            float depth = Application.isPlaying ? Depth : _editorDepth;
            float fadeAmount = Application.isPlaying ? FadeAmount : _editorFadeAmount;
            Color lineColor = Application.isPlaying ? _networkColor.Value : _color;
            Color fillColor = Application.isPlaying ? _networkFillColor.Value : _fillColor;

            if (bodyLength <= 0f)
            {
                bodyLength = radius;
            }

            int activeCount = 0;
            for (int i = 0; i < _lineVisuals.Count; i++)
            {
                if (_lineVisuals[i] != null && _lineVisuals[i].Root != null && _lineVisuals[i].Root.gameObject.activeSelf)
                {
                    activeCount++;
                }
            }

            int activeIndex = 0;
            foreach (LineVisual line in _lineVisuals)
            {
                if (line == null || line.Root == null || line.Root.gameObject.activeSelf == false)
                {
                    continue;
                }

                float lineAngle = activeCount > 1
                    ? arc / (activeCount - 1) * activeIndex - arc * 0.5f
                    : 0f;

                line.Root.localPosition = Vector3.zero;
                line.Root.localRotation = Quaternion.Euler(0f, lineAngle, 0f);

                UpdateBodyProjector(line.BodyProjector, bodyLength, width, depth, fillProgress, fadeAmount, lineColor, fillColor);
                UpdateHeadProjector(line.HeadProjector, bodyLength, width, depth, fillProgress, fadeAmount, lineColor, fillColor);
                activeIndex++;
            }
        }

        public List<Vector3> GenerateLineSpawnPositions(float spacing)
        {
            List<Vector3> positions = new List<Vector3>();
            float safeSpacing = Mathf.Max(spacing, 0.01f);
            float totalLength = GetIndicatorTotalLength();
            float arc = Application.isPlaying ? Arc : _editorArc;
            int activeCount = Application.isPlaying ? Mathf.Max(LineCount, MinLineCount) : Mathf.Max(_editorLineCount, MinLineCount);

            for (int i = 0; i < activeCount; i++)
            {
                float lineAngle = activeCount > 1
                    ? arc / (activeCount - 1) * i - arc * 0.5f
                    : 0f;

                Vector3 direction = Quaternion.Euler(0f, Angle + lineAngle, 0f) * Vector3.forward;

                for (float distance = 0f; distance < totalLength; distance += safeSpacing)
                {
                    TryAddUniquePosition(positions, Position + direction * distance);
                }

                TryAddUniquePosition(positions, Position + direction * totalLength);
            }

            return positions;
        }

        public float GetIndicatorTotalLength()
        {
            float bodyLength = Mathf.Max(Application.isPlaying ? BodyLength : _editorBodyLength, 0.01f);
            return bodyLength + GetHeadVisualLength();
        }

        public float GetIndicatorLineHalfWidth()
        {
            float widthMultiplier = Mathf.Max(Application.isPlaying ? Width : _editorWidth, 0.01f);
            return Mathf.Max(_templateBodyProjectorSize.x * widthMultiplier * 0.5f, 0.01f);
        }

        public bool IsPointInsideIndicator(Vector3 worldPosition, float padding = 0f)
        {
            float totalLength = GetIndicatorTotalLength();
            float halfWidth = Mathf.Max(GetIndicatorLineHalfWidth() + padding, 0.01f);
            int activeCount = Mathf.Max(LineCount, MinLineCount);
            Vector2 targetPoint = new Vector2(worldPosition.x, worldPosition.z);
            Vector2 startPoint = new Vector2(Position.x, Position.z);

            for (int i = 0; i < activeCount; i++)
            {
                float lineAngle = activeCount > 1
                    ? Arc / (activeCount - 1) * i - Arc * 0.5f
                    : 0f;

                Vector3 direction3 = Quaternion.Euler(0f, Angle + lineAngle, 0f) * Vector3.forward;
                Vector2 direction = new Vector2(direction3.x, direction3.z).normalized;
                Vector2 endPoint = startPoint + direction * totalLength;

                if (DistanceToSegment(targetPoint, startPoint, endPoint) <= halfWidth)
                {
                    return true;
                }
            }

            return false;
        }

        private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 segment = end - start;
            float segmentLengthSq = segment.sqrMagnitude;
            if (segmentLengthSq <= Mathf.Epsilon)
            {
                return Vector2.Distance(point, start);
            }

            float projection = Vector2.Dot(point - start, segment) / segmentLengthSq;
            projection = Mathf.Clamp01(projection);
            Vector2 closestPoint = start + segment * projection;
            return Vector2.Distance(point, closestPoint);
        }

        private static void TryAddUniquePosition(List<Vector3> positions, Vector3 candidate)
        {
            const float DuplicateThreshold = 0.0001f;

            for (int i = 0; i < positions.Count; i++)
            {
                if ((positions[i] - candidate).sqrMagnitude <= DuplicateThreshold)
                {
                    return;
                }
            }

            positions.Add(candidate);
        }

        private void UpdateBodyProjector(
            DecalProjector projector,
            float bodyLength,
            float width,
            float depth,
            float fillProgress,
            float fadeAmount,
            Color color,
            Color fillColor)
        {
            if (projector == null)
            {
                return;
            }

            Vector3 size = projector.size;
            size.x = Mathf.Max(_templateBodyProjectorSize.x * Mathf.Max(width, 0.01f), 0.01f);
            size.y = Mathf.Max(bodyLength, 0.01f);
            size.z = depth;

            projector.pivot = new Vector3(0f, 0f, depth * 0.5f);
            projector.size = size;
            float extraBodyLength = size.y - _templateBodyProjectorSize.y;
            projector.transform.localPosition = new Vector3(
                0f,
                YPosition,
                _templateBodyLocalZ + extraBodyLength * 0.5f + _bodyLocalZOffset);

            if (projector.material == null)
            {
                return;
            }

            projector.material.SetColor(ColorShaderID, color);
            projector.material.SetColor(FillColorShaderID, fillColor);
            projector.material.SetFloat(
                FillProgressShaderID,
                CalculateBodyFillProgress(fillProgress, bodyLength));
            projector.material.SetFloat(FadeAmountShaderID, fadeAmount * 2f);
        }

        private void UpdateHeadProjector(
            DecalProjector projector,
            float bodyLength,
            float width,
            float depth,
            float fillProgress,
            float fadeAmount,
            Color color,
            Color fillColor)
        {
            if (projector == null)
            {
                return;
            }

            Vector3 size = projector.size;
            size.x = Mathf.Max(
                _templateHeadProjectorSize.x * Mathf.Max(width, 0.01f) * Mathf.Max(_headWidthScale, 0.01f),
                0.01f);
            size.y = Mathf.Max(_templateHeadProjectorSize.y * Mathf.Max(_headLengthScale, 0.01f), 0.01f);
            size.z = depth;

            projector.pivot = new Vector3(0f, 0f, depth * 0.5f);
            projector.size = size;
            float extraBodyLength = Mathf.Max(bodyLength, 0.01f) - _templateBodyProjectorSize.y;
            float extraHeadLength = size.y - _templateHeadProjectorSize.y;
            projector.transform.localPosition = new Vector3(
                0f,
                YPosition,
                _templateHeadLocalZ + extraBodyLength + extraHeadLength * 0.5f + _headLocalZOffset);

            if (projector.material == null)
            {
                return;
            }

            projector.material.SetColor(ColorShaderID, color);
            projector.material.SetColor(FillColorShaderID, fillColor);
            projector.material.SetFloat(
                FillProgressShaderID,
                CalculateHeadFillProgress(fillProgress, bodyLength));
            projector.material.SetFloat(FadeAmountShaderID, fadeAmount * 2f - 0.5f);
        }

        private float CalculateBodyFillProgress(float fillProgress, float bodyLength)
        {
            float safeBodyLength = Mathf.Max(bodyLength, 0.01f);
            float totalLength = safeBodyLength + GetHeadVisualLength();
            float filledLength = Mathf.Clamp01(fillProgress) * totalLength;
            return Mathf.Clamp01(filledLength / safeBodyLength);
        }

        private float CalculateHeadFillProgress(float fillProgress, float bodyLength)
        {
            float safeBodyLength = Mathf.Max(bodyLength, 0.01f);
            float headLength = GetHeadVisualLength();
            float totalLength = safeBodyLength + headLength;
            float filledLength = Mathf.Clamp01(fillProgress) * totalLength;
            return Mathf.Clamp01((filledLength - safeBodyLength) / headLength);
        }

        private float GetHeadVisualLength()
        {
            return Mathf.Max(_templateHeadProjectorSize.y * Mathf.Max(_headLengthScale, 0.01f), 0.01f);
        }

        private void CacheTemplateGeometry()
        {
            if (_templateHead != null && _templateHead.TryGetComponent(out DecalProjector headProjector))
            {
                _templateHeadProjectorSize = headProjector.size;
                _templateHeadLocalZ = _templateHead.localPosition.z;
            }

            if (_templateBody != null && _templateBody.TryGetComponent(out DecalProjector bodyProjector))
            {
                _templateBodyProjectorSize = bodyProjector.size;
                _templateBodyLocalZ = _templateBody.localPosition.z;
            }
        }

        private IEnumerator PlayIndicator(float duration)
        {
            float elapsed = 0f;
            double previousTime = _relayManager.NetworkManagerEx.ServerTime.Time;

            while (elapsed < duration)
            {
                double currentTime = _relayManager.NetworkManagerEx.ServerTime.Time;
                double deltaTime = currentTime - previousTime;
                previousTime = currentTime;

                elapsed += (float)deltaTime;

                float fillProgress = Mathf.Clamp01(elapsed / duration);
                _editorFillProgress = fillProgress;
                UpdateLineVisuals(fillProgress);
                yield return null;
            }

            _onIndicatorDone?.Invoke();
            StopGaugeUpSfx();
            _editorFillProgress = 0f;
            UpdateLineVisuals(0f);
            _resourcesServices.DestroyObject(gameObject);
        }

        private void ReleaseIndicator()
        {
            StopGaugeUpSfx();
            if (IsHost)
            {
                Radius = 0f;
                Arc = 0f;
                Angle = 0f;
                BodyLength = 0f;
                Position = Vector3.zero;
            }

            SpawnerBossNetworkObjectId = InvalidSpawnerBossNetworkObjectId;
            _editorFillProgress = 0f;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            UpdateLineVisuals(0f);
            _onIndicatorDone = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureTemplateReferences();
            if (HaveTemplateMaterialsChanged())
            {
                ApplyTemplateMaterials();
                RefreshGeneratedLineMaterials();
                CacheAppliedTemplateMaterials();
            }
            _editorPreviewDirty = true;
        }
#endif
    }
}
