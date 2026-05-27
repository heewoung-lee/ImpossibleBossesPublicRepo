using Unity.Netcode;
using UnityEngine;

namespace NetWork.Sync
{
    public static class ChaseCatchUpCalculator
    {
        /// <summary>
        /// 서버 시작 시각과 현재 서버 시각의 차이를 기준으로,
        /// 늦게 받은 추적 오브젝트가 이번 프레임에 추가로 얼마나 더 이동해야 하는지 계산합니다.
        /// 이 메서드는 첫 호출 시 catch-up 상태를 초기화하고,
        /// 이후 호출마다 남은 catch-up 거리를 소비하면서 추가 이동 거리를 반환합니다.
        /// </summary>
        /// <param name="networkManager">
        /// 서버 기준 시간을 읽기 위한 NGO NetworkManager입니다.
        /// null이면 catch-up 계산을 할 수 없으므로 추가 이동 거리 0f를 반환합니다.
        /// </param>
        /// <param name="serverStartTime">
        /// 서버가 "이 추적 이동이 시작됐다"고 판단한 시각입니다.
        /// 클라는 이 값과 자신의 현재 ServerTime을 비교해 얼마나 늦게 받았는지 계산합니다.
        /// </param>
        /// <param name="isCatchUpInitialized">
        /// catch-up 초기화가 이미 끝났는지 여부입니다.
        /// false이면 첫 호출에서만 remainingCatchUpDistance를 계산하고 true로 바꿉니다.
        /// </param>
        /// <param name="remainingCatchUpDistance">
        /// 아직 따라잡아야 하는 남은 거리입니다.
        /// 첫 호출에서 서버 시간 차이와 기본 이동속도를 이용해 계산되고,
        /// 이후 호출마다 이번 프레임에 추가로 이동한 거리만큼 감소합니다.
        /// </param>
        /// <param name="deltaTime">
        /// 이번 프레임에서 흐른 시간입니다.
        /// 보통 Update에서는 Time.deltaTime,
        /// FixedUpdate에서는 Time.fixedDeltaTime을 전달합니다.
        /// 시간축을 좀더 맞추려면 ServerTime을 쓸것
        /// </param>
        /// <param name="baseMoveSpeed">
        /// catch-up을 적용하지 않았을 때의 기본 이동속도입니다.
        /// 서버에서 정상적으로 움직이고 있었다면 사용했어야 하는 기준 속도입니다.
        /// </param>
        /// <param name="catchUpDuration">
        /// 남은 지연 거리를 대략 몇 초에 걸쳐 따라잡을지 정하는 값입니다.
        /// 값이 작을수록 더 빨리 따라잡고,
        /// 값이 클수록 더 완만하게 따라잡습니다.
        /// </param>
        /// <param name="maxCatchUpMultiplier">
        /// catch-up 중 허용할 최대 속도 배수입니다.
        /// 예를 들어 3f이면 최종 이동속도는 최대 baseMoveSpeed * 3 까지만 허용됩니다.
        /// </param>
        /// <returns>
        /// 이번 프레임에 기본 이동 거리 외에 추가로 더 이동해야 하는 거리입니다.
        /// catch-up이 필요 없으면 0f를 반환하고,
        /// catch-up이 필요하면 0보다 큰 추가 이동 거리를 반환합니다.
        /// </returns>
        public static float ConsumeExtraDistance(
            NetworkManager networkManager,
            double serverStartTime,
            ref bool isCatchUpInitialized,
            ref float remainingCatchUpDistance,
            float deltaTime,
            float baseMoveSpeed,
            float catchUpDuration,
            float maxCatchUpMultiplier)
        {
            if (networkManager == null)
            {
                Debug.LogError("NetworkManager is null");
                return 0f;
            }

            // 첫 소비 시점에만
            // "늦게 받은 시간 * 기본 이동속도"로
            // 따라잡아야 할 거리 예산을 계산합니다.
            if (isCatchUpInitialized == false)
            {
                float elapsedTime = Mathf.Max(
                    0f,
                    (float)(networkManager.ServerTime.Time - serverStartTime));

                remainingCatchUpDistance = elapsedTime * baseMoveSpeed;
                isCatchUpInitialized = true;
            }

            // 더 따라잡을 거리가 없으면 추가 이동 없음
            if (remainingCatchUpDistance <= 0f)
            {
                return 0f;
            }

            float effectiveCatchUpDuration = Mathf.Max(catchUpDuration, 0.01f);

            // 남은 보정 거리를 catchUpDuration 안에 따라잡기 위해 필요한
            // 추가 속도를 계산합니다.
            float catchUpSpeed = Mathf.Min(
                remainingCatchUpDistance / effectiveCatchUpDuration,
                baseMoveSpeed * (maxCatchUpMultiplier - 1f));

            // 이번 프레임에 추가로 전진해야 할 거리
            float extraDistance = catchUpSpeed * deltaTime;

            remainingCatchUpDistance = Mathf.Max(
                0f,
                remainingCatchUpDistance - extraDistance);

            return extraDistance;
        }
    }
}
