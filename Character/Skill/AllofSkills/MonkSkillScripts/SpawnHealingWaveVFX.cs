using System;
using UnityEngine;
using Character.Skill.AllofSkills.MonkSkillScripts;
using DataType.Skill.Factory.Decorator.Strategy;

[RequireComponent(typeof(LineRenderer))]
public class SpawnHealingWaveVFX : MonoBehaviour, IGetChainVfxLineRenderer
{
    // 내부 VFX 참조
    private GameObject _startVFXObject;
    private GameObject _endVFXObject;
    private LineRenderer _lineRenderer;

    [Header("Visual Settings")]
    [SerializeField] private float textureScrollSpeed = 5f; // 텍스처 흐름 속도
    [SerializeField] private float textureLengthScale = 3f; // 텍스처 반복 비율
    [SerializeField] private float pulseSpeed = 10f;        // 두께 깜빡임 속도
    [SerializeField] private float minWidth = 0.5f;         // 최소 두께
    [SerializeField] private float maxWidth = 1.0f;         // 최대 두께

    public GameObject VFXStartObject { get; set; }
    public GameObject VFXEndObject { get; set; }
    public Vector3 VFXStartOffSetPosition { get; set; }
    public Vector3 VFXEndOffsetPosition { get; set; }

    void Awake()
    {
        var startComp = GetComponentInChildren<HealingWaveStart>();
        var endComp = GetComponentInChildren<HealingWaveEnd>();

        if (startComp) _startVFXObject = startComp.gameObject;
        if (endComp) _endVFXObject = endComp.gameObject;
        
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = true; 
    }

    private void OnEnable()
    {
        _lineRenderer.positionCount = 2;
    }

    void Update()
    {
        Vector3 startPos = VFXStartObject.transform.position + VFXStartOffSetPosition;
        Vector3 endPos =VFXEndObject.transform.position + VFXEndOffsetPosition;

        if (_startVFXObject != null) _startVFXObject.transform.position = startPos;
        if (_endVFXObject != null) _endVFXObject.transform.position = endPos;

        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);

        UpdateVisuals(startPos, endPos);
    }

    private void UpdateVisuals(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);

        _lineRenderer.material.mainTextureScale = new Vector2(distance / textureLengthScale, 1);

        _lineRenderer.material.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float currentWidth = Mathf.Lerp(minWidth, maxWidth, t);

        _lineRenderer.startWidth = currentWidth;
        _lineRenderer.endWidth = currentWidth;
    }

    public void Clear()
    {
        _lineRenderer.positionCount = 0;
        VFXStartObject = null;
        VFXEndObject = null;
        VFXStartOffSetPosition = Vector3.zero;
        VFXEndOffsetPosition = Vector3.zero;
    }
}