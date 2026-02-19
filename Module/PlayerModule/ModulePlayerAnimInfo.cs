using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Util;

namespace Module.PlayerModule
{
    public class ModulePlayerAnimInfo : MonoBehaviour
    {
        private Dictionary<int, AnimatorStateInfo> _playerAnimaInfoDict;
        private Animator _animator;
        public Animator Animator => _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _playerAnimaInfoDict = new Dictionary<int, AnimatorStateInfo>();
        }

        public async UniTask<AnimatorStateInfo> GetStateInfo(int animHashCode, CancellationToken token = default)
        {
            if (_playerAnimaInfoDict.TryGetValue(animHashCode, out AnimatorStateInfo animInfo) == false)
            {
                AnimatorStateInfo animatorState = await WaitUntilState(animHashCode, token);
                _playerAnimaInfoDict.Add(animHashCode, animatorState);
                return animatorState;
            }

            return animInfo;
        }

        /// <summary>
        /// 2.9일 추가 런타임중에 애니메이션 클립의 정보를 가져올 수 있는 함수.
        /// </summary>
        /// <param name="stateHash"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask<AnimatorStateInfo> WaitUntilState(int stateHash, CancellationToken token)
        {
            //기존 토큰에 타임아웃을 걸어둠 타임아웃은 1초후에도 애니메이션을 못찾으면 잘못적은것이라 판단하기 위함.
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(1)); // 1초 뒤 자동 취소 설정
       
                while (timeoutCts.IsCancellationRequested == false)
                {
                    AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
                    if (info.shortNameHash == stateHash || info.fullPathHash == stateHash)
                        return info;

                    if (_animator.IsInTransition(0))
                    {
                        AnimatorStateInfo next = _animator.GetNextAnimatorStateInfo(0);
                        if (next.shortNameHash == stateHash || next.fullPathHash == stateHash)
                            return next;
                    }

                    //취소여부를 여기서 받고 취소가 되면 예외 출력
                    bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, timeoutCts.Token)
                        .SuppressCancellationThrow();
                    
                    if (canceled)
                        break;
                }
            
            if (token.IsCancellationRequested)
            {
                UtilDebug.Log($"[WaitUntilState] 외부 요청에 의해 애니메이션 대기 취소됨. (Hash: {stateHash})");
            }
            else if (timeoutCts.IsCancellationRequested)
            {
                string currentStateName = _animator.GetCurrentAnimatorStateInfo(0).fullPathHash.ToString();
                UtilDebug.LogError($"[WaitUntilState] 타임아웃 에러! 1초 동안 상태를 찾지 못함. " +
                                   $"찾으려던 Hash: {stateHash}, 현재 실제 Hash: {currentStateName}");
            }
            return default;
        }
    }
}