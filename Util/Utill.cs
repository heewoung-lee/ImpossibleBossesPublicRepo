using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataType;
using DataType.Skill;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Effect.Def;
using Stats;
using Stats.BaseStats;
using UnityEngine;

namespace Util
{
    public class Utill
    {
        public static Color GetItemGradeColor(ItemGradeType grade)
        {
            switch (grade)
            {
                case ItemGradeType.Normal:
                    return Color.white;
                case ItemGradeType.Magic:
                    return Color.green;
                case ItemGradeType.Rare:
                    return new Color(150 / 255f, 200 / 255f, 255 / 255f); //파란색;
                case ItemGradeType.Unique:
                    return Color.red;
                case ItemGradeType.Epic:
                    return Color.yellow;
            }

            return Color.white;
        }

        public static T FindChild<T>(GameObject go, string name = null, bool recursive = false)
            where T : UnityEngine.Object
        {
            if (go == null)
                return null;

            if (recursive == false)
            {
                //제일 밑에 있는 자식을 탐색해서 component를 돌려주면 됨.
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    Transform child = go.transform.GetChild(i);
                    if (string.IsNullOrEmpty(child.name) || child.name == name)
                    {
                        T component = child.GetComponent<T>();
                        return component;
                    }
                }
            }
            else
            {
                foreach (T component in go.GetComponentsInChildren<T>())
                {
                    if (string.IsNullOrEmpty(name) || component.name == name)
                        return component;
                }
            }

            return null;
        }


        public static T[] FindChildAll<T>(GameObject go, string name = null, bool recursive = false)
            where T : UnityEngine.Object
        {
            List<T> list = new List<T>();
            if (recursive == false)
            {
                //제일 밑에 있는 자식을 탐색해서 component를 돌려주면 됨.
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    Transform child = go.transform.GetChild(i);
                    if (string.IsNullOrEmpty(child.name) || child.name == name)
                    {
                        list.Add(child.GetComponent<T>());
                    }
                }
            }
            else
            {
                foreach (T component in go.GetComponentsInChildren<T>())
                {
                    if (string.IsNullOrEmpty(name) || component.name == name)
                        list.Add(component);
                }
            }

            if (list.Count < 0)
                return null;
            else
                return list.ToArray();
        }


        public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
        {
            Transform transform = FindChild<Transform>(go, name, recursive);

            if (transform == null)
                return null;

            return transform.gameObject;
        }


        public static float GetAnimationLength(string animationName, Animator anim)
        {
            RuntimeAnimatorController ac = anim.runtimeAnimatorController;
            
            Debug.Assert(ac != null, $"anim is null");
            
            for (int i = 0; i < ac.animationClips.Length; i++)
            {
                AnimationClip clip = ac.animationClips[i];
                
                if (clip.name == animationName) // 완전 일치하게 적을 경우 리턴
                {
                    return clip.length;
                }
                
                if (clip.name.EndsWith("@" + animationName)) // 보통은 Fighter@Jump 이
                {
                    return clip.length;
                }
            }
            Debug.LogWarning($"[AttackSkillStrategy] Could not find AnimationClip named '{animationName}'. Defaulting to 1.0f.");
            return 1.0f;
        }

        public static string ItemGradeConvertToKorean(ItemGradeType itemGrade)
        {
            switch (itemGrade)
            {
                case ItemGradeType.Normal:
                    return "노멀";
                case ItemGradeType.Magic:
                    return "매직";
                case ItemGradeType.Rare:
                    return "레어";
                case ItemGradeType.Unique:
                    return "유니크";
                case ItemGradeType.Epic:
                    return "에픽";
            }

            return "Unknown";
        }

        public static string StatTypeConvertToKorean(StatType statType)
        {
            switch (statType)
            {
                case StatType.MaxHP:
                    return "최대체력";
                case StatType.CurrentHp:
                    return "체력";
                case StatType.Attack:
                    return "공격력";
                case StatType.Defence:
                    return "방어력";
                case StatType.MoveSpeed:
                    return "이동속도";
            }

            return "Unknown";
        }

        public static bool IsAlphanumeric(string input)
        {
            // 영문과 숫자만 포함된 문자열인지 확인
            return Regex.IsMatch(input, "^[A-Za-z0-9]+$");
        }

        //public static async Task<T> RateLimited<T>(Func<Task<T>> action, int millisecondsDelay = 1000)
        //{
        //    Debug.LogWarning($"Rate limit exceeded. Retrying in {millisecondsDelay / 1000} seconds...");
        //    await Task.Delay(millisecondsDelay); // 대기
        //    return await action.Invoke(); // 전달받은 작업 실행 및 결과 반환
        //}
        //public static async Task RateLimited(Func<Task> action, int millisecondsDelay = 1000)
        //{
        //    Debug.LogWarning($"Rate limit exceeded. Retrying in {millisecondsDelay / 1000} seconds...");
        //    await Task.Delay(millisecondsDelay); // 대기
        //    await action.Invoke(); // 전달받은 작업 실행 및 결과 반환
        //}

        private static CancellationTokenSource _retryCts;
        private static CancellationTokenSource _retryCtsVoid;

        public static async UniTask<T> RateLimited<T>(Func<UniTask<T>> action, int delayMs = 1_000)
        {
            // 1) 먼저 새 CTS를 만든다.
            var newCts = new CancellationTokenSource();

            // 2) 이전 CTS를 원자적으로 취소·폐기하고
            var prevCts = Interlocked.Exchange(ref _retryCts, newCts);
            prevCts?.Cancel();
            prevCts?.Dispose();

            try
            {
                Debug.LogWarning($"Rate limit exceeded. Retrying in {delayMs / 1000f} seconds…");
                await UniTask.Delay(delayMs, ignoreTimeScale: true, cancellationToken: newCts.Token); 
                return await action();
            }
            catch (OperationCanceledException) // [수정] UniTask 취소 표준 예외
            {
                Debug.Log("RateLimited<T>: 이전 예약이 취소되어 실행하지 않습니다.");
                return default;
            }
            finally
            {
                Interlocked.CompareExchange(ref _retryCts, null, newCts);
                newCts.Dispose();
            }
        }

        public static async UniTask RateLimited(Func<UniTask> action, int delayMs = 1_000)
        {
            // 1) 먼저 새 CTS를 만든다.
            var newCts = new CancellationTokenSource();

            // 2) 이전 CTS를 원자적으로 취소·폐기하고
            var prevCts = Interlocked.Exchange(ref _retryCtsVoid, newCts);
            prevCts?.Cancel();
            prevCts?.Dispose();

            try
            {
                Debug.LogWarning($"Rate limit exceeded. Retrying in {delayMs / 1000f} seconds…");
                await UniTask.Delay(delayMs, ignoreTimeScale: true, cancellationToken: newCts.Token); 
                await action();
            }
            catch (OperationCanceledException) // [수정] UniTask 취소 표준 예외
            {
                Debug.Log("RateLimited<T>: 이전 예약이 취소되어 실행하지 않습니다.");
            }
            finally
            {
                Interlocked.CompareExchange(ref _retryCtsVoid, null, newCts);
                newCts.Dispose();
            }
        }

        public static string GetLayerID(Enum enumvalue)
        {
            return enumvalue.ToString();
        }
        
        
        
        public static string GetFinalDescription(BaseDataSO data, PlayerStats stats)
        {
            if (data == null || string.IsNullOrEmpty(data.description)) return "";
        
            string rawText = data.description;
        
            // 정규식으로 {Keyword} 찾기
            return Regex.Replace(rawText, @"\{(.*?)\}", match =>
            {
                string keyword = match.Groups[1].Value;
                return ResolveKeyword(keyword, data, stats);
            });
        }

        private static string ResolveKeyword(string keyword, BaseDataSO data, PlayerStats stats)
        {
            if (data is SkillDataSO skilldata)
            {
                // {AttackDamage} 키워드를 발견했을 때
                if (keyword == "AttackDamage")
                {
                    if (skilldata.effect is AttackEffectDef attackEffect)
                    {
                        float finalDamage = stats.Attack * attackEffect.multiplier + attackEffect.additional;
                        return $"<color=red>{finalDamage:F0}</color>"; 
                    }
                }
            }
            return keyword;
        }
        

    }
}