using System;
using System.Collections.Generic;
using System.Text;
using Data.DataType.ItemType.Interface;
using DataType.Strategies;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Util;

namespace DataType.Item.Consumable
{
    [CreateAssetMenu(fileName = "Consume_", menuName = "DataSO/Item/Consumable")]
    public class ConsumableItemSO : ItemDataSO
    {
        [Serializable]
        public class ConsumableBuffData
        {
            [HideLabel] 
            [Title("Stat Effect")]
            public StatEffect effect; // 수치 데이터

            [Title("Visual")]
            [VerticalGroup("VisualGroup")]
            [OnValueChanged(nameof(UpdateIconPath))] // 1. 스프라이트 넣으면 경로 자동 입력
            [PreviewField(50, ObjectFieldAlignment.Left)]
            [LabelWidth(50)]
            public Sprite icon; // 에디터에서 눈으로 보는 용도

            [VerticalGroup("VisualGroup")]
            [ReadOnly]
            [LabelText("Resource Path")]
            public string iconPath; // 2. 실제 서버/클라이언트로 보내는 경로 문자열

            // 이 함수가 오딘 인스펙터에 의해 호출됩니다.
            public void UpdateIconPath()
            {
#if UNITY_EDITOR
                if (icon == null)
                {
                    iconPath = "";
                    return;
                }

                string fullPath = AssetDatabase.GetAssetPath(icon);
                // Resources 폴더 안에 있는지 체크
                int resourcesIndex = fullPath.IndexOf("Resources/", StringComparison.Ordinal);

                if (resourcesIndex >= 0)
                {
                    // Resources/ 뒷부분부터 확장자(.png) 앞부분까지 자름
                    string resourcePath = fullPath.Substring(resourcesIndex + 10);
                    int extensionIndex = resourcePath.LastIndexOf('.');
                    if (extensionIndex >= 0)
                        resourcePath = resourcePath.Substring(0, extensionIndex);
                    
                    iconPath = resourcePath;
                }
                else
                {
                    iconPath = "";
                    UtilDebug.LogError($"[Path Error] '{icon.name}' 이미지는 반드시 Resources 폴더 안에 있어야 합니다!");
                }
#endif
            }
        }

        [Title("Consumable Data")]
        [SuffixLabel("sec", Overlay = true)]
        public float duration = 0f; 

        [Title("Effects List")]
        [TableList(ShowIndexLabels = true)] 
        public List<ConsumableBuffData> itemEffects = new List<ConsumableBuffData>();

        public override Type GetStrategyType() => typeof(ConsumableStrategy);

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (itemEffects == null) return;
            
            foreach (var buffData in itemEffects)
            {
                // 스프라이트는 연결되어 있는데, 경로가 틀어졌을 경우를 대비해 
                // 에디터에서 데이터가 로드될 때마다 경로를 다시 계산
                buffData.UpdateIconPath(); 
            }
#endif
        }
        
        public override string GetItemEffectText()
        {
            StringBuilder descriptionBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(description)) descriptionBuilder.AppendLine(description);

            foreach (ConsumableBuffData data in itemEffects)
            {
                string actionText = (duration > 0) ? "증가" : "회복";
                descriptionBuilder.AppendLine($"{Utill.StatTypeConvertToKorean(data.effect.statType)} {data.effect.value} {actionText}");
            }
            if (duration > 0) descriptionBuilder.AppendLine($"지속시간: {duration}초");

            return descriptionBuilder.ToString();
        }
        
        public override ItemType ItemType => ItemType.Consumable;
    }
}