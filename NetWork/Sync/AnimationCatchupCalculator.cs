using Unity.Netcode;
using UnityEngine;
using Util;

namespace NetWork.Sync
{
    public static class AnimationCatchUpCalculator
    {
        /// <summary>
        /// 서버 시작 시각과 현재 서버 시각의 차이를 기준으로,
        /// 늦게 받은 애니메이션을 몇 배속으로 재생해야 자연스럽게 따라잡을지 계산합니다.
        /// 이 메서드는 첫 호출 시 catch-up 상태를 초기화하고,
        /// 이후 호출마다 남은 catch-up 시간을 소비하면서 최종 배속을 반환합니다.
        /// </summary>
        /// <param name="networkManager">
        /// 서버 기준 시간을 읽기 위한 NGO NetworkManager입니다.
        /// null이면 catch-up 계산을 할 수 없으므로 기본 배속 1f를 반환합니다.
        /// </param>
        /// <param name="serverStartTime">
        /// 서버가 애니메이션 시작을 결정한 시각입니다.
        /// 클라는 이 값과 자신의 현재 ServerTime을 비교해 얼마나 늦게 받았는지 계산합니다.
        /// </param>
        /// <param name="isCatchUpInitialized">
        /// catch-up 초기화가 이미 끝났는지 여부입니다.
        /// false이면 첫 호출에서만 remainingCatchUpTime을 계산하고 true로 바꿉니다.
        /// </param>
        /// <param name="remainingCatchUpTime">
        /// 아직 따라잡아야 하는 남은 애니메이션 시간입니다.
        /// 첫 호출에서 서버 시간 차이만큼 세팅되고,
        /// 이후 호출마다 이번 프레임에 따라잡은 양만큼 감소합니다.
        /// </param>
        /// <param name="deltaTime">
        /// 이번 프레임에서 흐른 시간입니다.
        /// 보통 Update에서는 Time.deltaTime,
        /// FixedUpdate에서는 Time.fixedDeltaTime을 전달합니다.
        /// </param>
        /// <param name="catchUpDuration">
        /// 남은 지연 시간을 대략 몇 초에 걸쳐 따라잡을지 정하는 값입니다.
        /// 값이 작을수록 더 빨리 따라잡고,
        /// 값이 클수록 더 완만하게 따라잡습니다.
        /// </param>
        /// <param name="maxCatchUpMultiplier">
        /// catch-up 중 허용할 최대 배속입니다.
        /// 예를 들어 3f이면 최종 배속은 최대 3배속까지만 허용됩니다.
        /// </param>
        /// <returns>
        /// 이번 프레임에 적용해야 할 최종 애니메이션 배속입니다.
        /// catch-up이 필요 없으면 1f를 반환하고,
        /// catch-up이 필요하면 1f보다 큰 배속을 반환합니다.
        /// </returns>
        public static float ConsumeSpeedMultiplier(
            NetworkManager networkManager,
            double serverStartTime,
            ref bool isCatchUpInitialized,
            ref float remainingCatchUpTime,
            float deltaTime,
            float catchUpDuration,
            float maxCatchUpMultiplier)
        {
            if (networkManager == null)
            {
                UtilDebug.LogError("networkManager == null 네트워크 매니저를 확인하십시오");
                return 1f;
            }

            // 첫 소비 시점에만 서버 기준 경과 시간을 계산합니다.
            if (isCatchUpInitialized == false)
            {
                float elapsedTime = Mathf.Max(
                    0f,
                    (float)(networkManager.ServerTime.Time - serverStartTime));

                remainingCatchUpTime = elapsedTime;
                isCatchUpInitialized = true;
            }

            // 더 따라잡을 시간이 없으면 기본 재생속도 유지
            if (remainingCatchUpTime <= 0f)
            {
                return 1f;
            }

            float effectiveCatchUpDuration = Mathf.Max(catchUpDuration, 0.01f);

            // 남은 지연 시간을 catchUpDuration 안에 따라잡기 위해 필요한
            // 추가배속 계산
            float extraSpeedMultiplier = Mathf.Min(
                remainingCatchUpTime / effectiveCatchUpDuration,
                maxCatchUpMultiplier - 1f);

            // 이번 프레임에서 추가 배속으로 얼마나 따라잡았는지 계산
            float consumedCatchUpTime = extraSpeedMultiplier * deltaTime;

            remainingCatchUpTime = Mathf.Max(
                0f,
                remainingCatchUpTime - consumedCatchUpTime);

            // 최종 배속 = 기본 1배속 + 추가 배속
            return 1f + extraSpeedMultiplier;
        }
    }
}
