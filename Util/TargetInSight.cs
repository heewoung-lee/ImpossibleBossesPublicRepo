using System;
using System.Collections.Generic;
using Stats.BaseStats;
using UnityEngine;

namespace Util
{
    public class TargetInSight
    {
        public static int ID = 0;

        public static Vector3 BoundaryAngle(float angle, Transform playerTransform)
        {
            angle += playerTransform.eulerAngles.y;
            return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad));
        }

        public static void AttackTargetInSector(IAttackRange attacker, int damage = -1, Func<Transform, Transform, float> damageMultiplierCalculator = null)
        {
            AttackTargetInSector(
                attacker,
                damage,
                attacker.ViewDistance,
                attacker.ViewAngle,
                attacker.OwnerTransform,
                damageMultiplierCalculator // 이름 변경: 의미를 명확하게
            );
        }
        public static bool IsBackAttack(Transform attacker, Transform victim,float backAttackAngle)
        {
            // 타겟의 뒤쪽 방향 벡터 구하기 (-forward)
            Vector3 victimBack = -victim.forward;
    
            //타겟 공격자 방향 벡터
            Vector3 dirToAttacker = (attacker.position - victim.position).normalized;

            // 높낮이 무시
            victimBack.y = 0;
            dirToAttacker.y = 0;
    
            victimBack.Normalize();
            dirToAttacker.Normalize();

            //내적 계산
            // 1.0  : 타겟 완벽한 뒤 (백어택 정중앙)
            // 0.5  : 타겟 뒤쪽 60도 (백어택 경계선)
            // 0.0  : 타겟 옆 (90도)
            // -1.0 : 타겟 정면 (앞)
            float dot = Vector3.Dot(victimBack, dirToAttacker);

            // 각도를 코사인 값(Threshold)으로 변환
            // backAttackAngle이 120도라면, 좌우 60도씩 허용한다는 뜻.
            // Cos(60도) = 0.5 입니다.
            // 즉, 내적값이 0.5보다 크면(1.0에 가까우면) 백어택 범위 안에 있다는 뜻입니다.
    
            float halfAngle = backAttackAngle * 0.5f;
            float threshold = Mathf.Cos(halfAngle * Mathf.Deg2Rad); // 미리 계산해두면 더 좋음

            // 내적 값이 기준치보다 크면 (더 뒤쪽에 가까우면) true
            return dot >= threshold;
        }
        

        public static void AttackTargetInSector(IAttackRange attacker, int specialDamage, float radius, float angle,
            Transform ownerTransform,Func<Transform, Transform, float> damageMultiplierCalculator = null)
        {
            
            int baseDamage = ownerTransform.GetComponent<BaseStats>().Attack;
            
            Vector3 leftBoundary = BoundaryAngle(angle * -0.5f, ownerTransform);
            Vector3 rightBoundary = BoundaryAngle(angle * 0.5f, ownerTransform);

            Debug.DrawRay(ownerTransform.position + ownerTransform.up * 0.4f, leftBoundary * radius, Color.red, 1f);
            Debug.DrawRay(ownerTransform.position + ownerTransform.up * 0.4f, rightBoundary * radius, Color.red, 1f);

            Collider[] targets = Physics.OverlapSphere(ownerTransform.position, radius, attacker.TarGetLayer);
            foreach (Collider target in targets)
            {
                // 자기 자신은 제외
                if (target.transform == ownerTransform) continue;

                Transform targetTr = target.transform;
                if (target.TryGetComponent(out IDamageable idamaged))
                {
                    Vector3 direction = (targetTr.position - ownerTransform.position).normalized;
                    float targetAngle = Vector3.Angle(direction, ownerTransform.forward);

                    if (targetAngle < angle * 0.5f)
                    {
                        if (Physics.Raycast(ownerTransform.position + ownerTransform.up * 0.4f, direction,radius))
                        {
                            int finalDamage = specialDamage > 0 ? specialDamage : baseDamage; 
                            float multiplier = 1.0f;
                            if (damageMultiplierCalculator != null)
                            {
                                multiplier = damageMultiplierCalculator(ownerTransform, targetTr);
                            }

                            int calculatedDamage = Mathf.RoundToInt(finalDamage * multiplier);

                            idamaged.OnAttacked(attacker, calculatedDamage);
                        }
                    }
                }
            }
        }

        public static void AttackTargetInCircle(IAttackRange stats, float radius, int? damage = null)
        {
            Collider[] targets = Physics.OverlapSphere(stats.AttackPosition, radius, stats.TarGetLayer);
            DebugDrawUtill.DrawCircle(stats.AttackPosition, radius, 96, Color.yellow, 5f);
            foreach (Collider target in targets)
            {
                if (target.TryGetComponent(out IDamageable idamaged))
                {
                    if (damage == null)
                        idamaged.OnAttacked(stats);
                    else
                        idamaged.OnAttacked(stats, damage.Value);
                }
            }
        }

        public static bool IsTargetInSight(IAttackRange stats, Transform targetTr, float sightRange = 0.5f)
        {
            Vector3 direction = (targetTr.position - stats.OwnerTransform.position).normalized;
            float angle = Vector3.Angle(direction, stats.OwnerTransform.forward);
            sightRange = Mathf.Clamp(sightRange, 0f, 0.5f);


            if (angle < stats.ViewAngle * sightRange)
            {
                return true;
            }

            return false;
        }

        public static List<Vector3> GeneratePositionsInSector(Transform originTr, float totalAngle, float totalRadius,
            int angleSteps, int radiusStep)
        {
            //현재 캐릭터의 좌표, 부채꼴의 각도, 반지름 길이, 각도에 따라 몇개로 나눌껀지, 길이에 따라 몇개로 나눌껀지
            //포지션을 담을 리스트, 단위당 길이, 단위당 각도

            List<Vector3> positions = new List<Vector3>();
            float anglePerUnit = totalAngle / (angleSteps - 1);
            float radiusPerUnit = totalRadius / radiusStep;
            float halfangle = totalAngle / 2f;

            for (int i = 1; i <= radiusStep; i++)
            {
                float currentRadius = radiusPerUnit * i;
                for (int j = 0; j < angleSteps; j++)
                {
                    //각도를 추출 
                    float currentAngle = -halfangle + anglePerUnit * j; //Degree -> Rad
                    float angleInRadian = (currentAngle + originTr.eulerAngles.y) * Mathf.Deg2Rad;
                    //삼각함수를 통해 단위 원에 대한 좌표를 출력
                    Vector3 perUnitPos = new Vector3(Mathf.Sin(angleInRadian), 0f, Mathf.Cos(angleInRadian));
                    //내위치와 길이단위에 맞게 조정
                    Vector3 currentPos = originTr.position + perUnitPos * currentRadius;
                    positions.Add(currentPos);
                }
            }

            return positions;
        }
        /// <summary>
        /// 부채꼴 형태의 발사 각도(회전값)들을 계산하여 반환합니다.
        /// </summary>
        /// <param name="originTr">발사 주체</param>
        /// <param name="totalAngle">부채꼴 전체 각도</param>
        /// <param name="count">발사체 개수</param>
        /// <returns>각 발사체가 바라볼 회전값 리스트</returns>
        public static List<Quaternion> GenerateSpreadRotations(Transform originTr, float totalAngle, int count)
        {
            List<Quaternion> rotations = new List<Quaternion>();

            if (count <= 0) return rotations;

            if (count == 1) // 주의 1개일땐 정방향 하나를 리턴해야함,
            {
                rotations.Add(originTr.rotation);
                return rotations;
            }

            // 사이각 계산
            float angleStep = totalAngle / (count - 1);
        
            // 시작 각도
            float currentAngle = -totalAngle / 2f;

            for (int i = 0; i < count; i++)
            {
                Quaternion localRotation = Quaternion.Euler(0, currentAngle, 0);

                // 플레이어의 현재 회전값(World)에 로컬 회전값을 곱해서 최종 회전값 도출
                // 기준회전 * 로컬회전
                Quaternion finalRotation = originTr.rotation * localRotation;

                rotations.Add(finalRotation);

                // 다음 각도로 이동
                currentAngle += angleStep;
            }
            return rotations;
        }
        public static List<Vector3> GeneratePositionsInCircle(Transform originTr, float totalRadius, int angleSteps,
            int radiusStep)
        {
            return GeneratePositionsInSector(originTr, 360, totalRadius, angleSteps, radiusStep);
        }
    }
}