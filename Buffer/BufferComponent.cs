using System;
using System.Collections;
using System.Collections.Generic;
using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface.BufferManager;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using GameManagers.ResourcesEx;
using Stats.BaseStats;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Buffer
{
    public class BufferComponent : MonoBehaviour
    {
        [Inject] private IResourcesServices _resourcesServices;
        [Inject] private IBufferManager _bufferManager;
        [Inject] private IVFXManagerServices _vfxManager;

        private BaseStats _targetStat;

        public BaseStats TarGetStat
        {
            get => _targetStat;
        }

        private StatType _statType;
        public StatType StatType => _statType;

        private float _value;

        public float Value
        {
            get => _value;
        }

        private float _duration;
        private Image _image;
        private string _buffID;
        private string _vfxPrefabPath;

        public string BuffID
        {
            get => _buffID;
        }

        private Coroutine _bufferCoroutine;
        private LayoutElement _layoutElement;


        private void Awake()
        {
            _layoutElement = GetComponent<LayoutElement>();
        }

        public void InitAndStartBuff(BaseStats targetStat, float duration, StatType statType,
            float value, Sprite icon, string buffID)
        {
            _targetStat = targetStat;
            _duration = duration;
            _statType = statType;
            _value = value;
            _buffID = buffID;
            _image = GetComponentInChildren<Image>();
            if (icon == null)
            {
                _image.sprite = null;
                _image.color = Color.clear;
            }
            else
            {
                _image.sprite = icon;
                _image.color = Color.white;
            }

            StartBuff();
        }

        private void StartBuff()
        {
            if (RemoveSameTypeBuff())
            {
                return;
            }
            

            _bufferManager.ModifyStat(_targetStat, _statType, _value);
            _bufferCoroutine = StartBuffer();
        }

        private bool RemoveSameTypeBuff()
        {
            foreach (BufferComponent buffer in transform.parent.GetComponentsInChildren<BufferComponent>())
            {
                if (_targetStat != buffer._targetStat)
                    continue;

                // [변경] Modifier 비교 -> StatType 비교
                if (_statType != buffer.StatType)
                {
                    continue; // 다른 종류면 패스
                }

                if (buffer == this)
                {
                    continue; // 나 자신이면 패스
                }

                if (_buffID != buffer.BuffID)
                {
                    continue;
                }


                // 같은 종류가 이미 있다면?
                buffer.BufferReStart(_duration);
                _resourcesServices.DestroyObject(gameObject);

                return true;
            }

            return false;
        }

        private IEnumerator StartBuffFlicker()
        {
            //아이콘이 없으면 상대방 디버프일 수 있으니깐 그때는 깜박임 없이 지속시간만 지나면 해제
            //1.29일 수정 버프를 걸 때 내 소유인 객체들에게 버프를 걸 때, UI가 중복해서 나오는 문제가 있었음.
            //UI는 오직 내 캐릭터가 버프/디버프를 받았을때만 나와야 하기 때문에.
            //아래 조건으로 버프/디버프 능력치는 유지한채, UI만 내 캐릭터에게 나오도록 수정함.
            //하드 코딩이긴 한데 나중에 문제 생기면 바꿀것
            if (_image.sprite == null || _targetStat.gameObject.layer != LayerMask.NameToLayer("Player"))
            {
                if (_image != null)
                    _image.enabled = false;
                if (_layoutElement != null)
                    _layoutElement.ignoreLayout = true;
                yield return new WaitForSeconds(_duration);
                _bufferManager.RemoveBuffer(this);
                yield break; // 코루틴 종료
            }

            if (_image != null)
            {
                float elapsedTime = _duration;
                float minAlpha = 0.3f; // 최소 알파값 (조절 가능)
                float maxAlpha = 1f; // 최대 알파값 (조절 가능)
                float remainingTime = 5f;
                float timeDeal = 0f;
                // 처음에는 이미지를 완전한 알파값으로 세팅
                Color color = _image.color;
                color.a = maxAlpha;
                _image.color = color;

                while (elapsedTime > 0)
                {
                    elapsedTime -= Time.deltaTime;
                    if (elapsedTime < remainingTime)
                    {
                        float timeRatio = 1f - (elapsedTime / remainingTime);
                        float flickerSpeed = Mathf.Lerp(3f, 10f, timeRatio);
                        timeDeal += Time.deltaTime * flickerSpeed; //값의 증가량을 일정하게 높여야하므로 Time을 더함
                        float t = Mathf.PingPong(timeDeal, 1f);
                        color.a = Mathf.Lerp(minAlpha, maxAlpha, t);
                        _image.color = color;
                    }
                    else
                    {
                        // 5초 이상 남았을 땐 깜빡이지 않고 알파값을 고정
                        color = _image.color;
                        color.a = maxAlpha;
                        _image.color = color;
                    }

                    yield return null;
                }
            }

            _bufferManager.RemoveBuffer(this);
        }


        private Coroutine StartBuffer()
        {
            return StartCoroutine(StartBuffFlicker());
        }


        public void BufferReStart(float newDuration)
        {
            // 1. 기존 코루틴 정지
            if (_bufferCoroutine != null)
            {
                StopCoroutine(_bufferCoroutine);
            }

            _duration = newDuration;

            if (_image != null && _image.sprite != null)
            {
                Color color = _image.color;
                color.a = 1f; // 투명도 원상복구
                _image.color = color;
            }

            _bufferCoroutine = StartCoroutine(StartBuffFlicker());
        }
    }
}