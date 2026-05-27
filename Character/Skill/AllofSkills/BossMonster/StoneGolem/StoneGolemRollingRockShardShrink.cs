using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTreeNode.BossGolem.Task
{
    public class StoneGolemRollingRockShardShrink : MonoBehaviour
    {
        private const float ShrinkDelay = 1.5f;
        private const float ShrinkDuration = 0.6f;

        private readonly List<Transform> _shardTransforms = new List<Transform>();
        private readonly List<Vector3> _originScales = new List<Vector3>();

        private bool _isPlaying;
        private float _elapsedTime;

        private void Awake()
        {
            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>(true);
            Rigidbody rootRigidbody = GetComponent<Rigidbody>();

            for (int i = 0; i < rigidbodies.Length; i++)
            {
                Rigidbody shardRigidbody = rigidbodies[i];
                if (shardRigidbody == rootRigidbody)
                {
                    continue;
                }

                _shardTransforms.Add(shardRigidbody.transform);
                _originScales.Add(shardRigidbody.transform.localScale);
            }
        }

        private void Update()
        {
            if (_isPlaying == false)
            {
                return;
            }

            _elapsedTime += Time.deltaTime;
            if (_elapsedTime < ShrinkDelay)
            {
                return;
            }

            float normalizedTime = Mathf.Clamp01((_elapsedTime - ShrinkDelay) / ShrinkDuration);
            float scaleMultiplier = 1f - normalizedTime;

            for (int i = 0; i < _shardTransforms.Count; i++)
            {
                _shardTransforms[i].localScale = _originScales[i] * scaleMultiplier;
            }

            if (normalizedTime >= 1f)
            {
                _isPlaying = false;
            }
        }

        public void Play()
        {
            _elapsedTime = 0f;
            _isPlaying = true;

            for (int i = 0; i < _shardTransforms.Count; i++)
            {
                _shardTransforms[i].localScale = _originScales[i];
            }
        }

        private void OnDisable()
        {
            _isPlaying = false;
            _elapsedTime = 0f;

            for (int i = 0; i < _shardTransforms.Count; i++)
            {
                _shardTransforms[i].localScale = _originScales[i];
            }
        }
    }
}
