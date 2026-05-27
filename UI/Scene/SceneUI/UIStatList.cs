using System;
using System.Collections.Generic;
using DataType.Item.Consumable;
using GameManagers.ResourcesExManagement;
using Stats.BaseStats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.Scene.SceneUI
{
    public class UIStatList : MonoBehaviour
    {
        private const string HpStatIconPath = "Art/UI/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/IconMisc/IconSet_Stat_0";
        private const string AttackStatIconPath = "Art/UI/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/IconMisc/IconSet_Stat_1";
        private const string DefenceStatIconPath = "Art/UI/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/IconMisc/IconSet_Stat_3";
        private const string MoveSpeedStatIconPath = "Art/UI/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/Icon_PictoIcons/Original/function_icon_boot_fly";

        private static readonly Dictionary<StatType, string> StatIconPaths = new Dictionary<StatType, string>
        {
            { StatType.MaxHP, HpStatIconPath },
            { StatType.CurrentHp, HpStatIconPath },
            { StatType.Attack, AttackStatIconPath },
            { StatType.Defence, DefenceStatIconPath },
            { StatType.MoveSpeed, MoveSpeedStatIconPath },
        };

        private readonly Dictionary<StatType, Sprite> _statIconCache = new Dictionary<StatType, Sprite>();
        private readonly List<StatRowView> _rows = new List<StatRowView>();
        private bool _isStatIconCached;

        private struct StatRowView
        {
            public GameObject Root;
            public Image Icon;
            public TMP_Text Text;
        }

        public void Initialize()
        {
            CacheRows();
            Hide();
        }

        public void CacheStatIcons(IResourcesServices resourcesServices)
        {
            EnsureResourceService(resourcesServices);

            if (_isStatIconCached)
            {
                return;
            }

            foreach (KeyValuePair<StatType, string> statIconPath in StatIconPaths)
            {
                Sprite icon = resourcesServices.Load<Sprite>(statIconPath.Value);
                if (icon == null)
                {
                    throw new MissingReferenceException($"{statIconPath.Key} stat icon could not be loaded: {statIconPath.Value}");
                }

                _statIconCache[statIconPath.Key] = icon;
            }

            _isStatIconCached = true;
        }

        public void SetStats(IReadOnlyList<ConsumableItemSO.ConsumableBuffData> buffDataList)
        {
            if (buffDataList == null || buffDataList.Count == 0)
            {
                Hide();
                return;
            }

            EnsureStatIconCached();
            EnsureRowCapacity(buffDataList.Count);
            ShowRows(buffDataList.Count);

            for (int i = 0; i < buffDataList.Count; i++)
            {
                SetRow(_rows[i], buffDataList[i].effect);
            }
        }

        public void SetStats(IReadOnlyList<StatEffect> effects)
        {
            if (effects == null || effects.Count == 0)
            {
                Hide();
                return;
            }

            EnsureStatIconCached();
            EnsureRowCapacity(effects.Count);
            ShowRows(effects.Count);

            for (int i = 0; i < effects.Count; i++)
            {
                SetRow(_rows[i], effects[i]);
            }
        }

        public void SetDisplayRow(Sprite icon, string text)
        {
            EnsureRowCapacity(1);
            ShowRows(1);

            _rows[0].Icon.gameObject.SetActive(icon != null);
            _rows[0].Icon.sprite = icon;
            _rows[0].Text.text = text;
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            for (int i = 0; i < _rows.Count; i++)
            {
                _rows[i].Root.SetActive(false);
            }
        }

        private void CacheRows()
        {
            _rows.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject rowRoot = transform.GetChild(i).gameObject;
                Image icon = FindChildComponent<Image>(rowRoot, "StatIcon");
                TMP_Text text = FindChildComponent<TMP_Text>(rowRoot, "StatText");

                if (icon == null || text == null)
                {
                    continue;
                }

                _rows.Add(new StatRowView
                {
                    Root = rowRoot,
                    Icon = icon,
                    Text = text,
                });
            }
        }

        private void EnsureRowCapacity(int count)
        {
            if (_rows.Count < count)
            {
                throw new MissingReferenceException($"StatList has too few StatRows. Required: {count}, Current: {_rows.Count}");
            }
        }

        private static void EnsureResourceService(IResourcesServices resourcesServices)
        {
            if (resourcesServices == null)
            {
                throw new MissingReferenceException("UIStatList requires IResourcesServices when caching stat icons.");
            }
        }

        private void EnsureStatIconCached()
        {
            if (_isStatIconCached == false)
            {
                throw new MissingReferenceException("UIStatList stat icons have not been cached yet.");
            }
        }

        private void ShowRows(int count)
        {
            gameObject.SetActive(true);

            for (int i = 0; i < _rows.Count; i++)
            {
                _rows[i].Root.SetActive(i < count);
            }
        }

        private void SetRow(StatRowView row, StatEffect effect)
        {
            row.Icon.gameObject.SetActive(true);
            row.Icon.sprite = GetStatIcon(effect.statType);
            row.Text.text = $"{Utill.StatTypeConvertToKorean(effect.statType)} {FormatSignedValue(effect.value)}";
        }

        private Sprite GetStatIcon(StatType statType)
        {
            if (_statIconCache.TryGetValue(statType, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            throw new MissingReferenceException($"{statType} stat icon has not been cached in UIStatList.");
        }

        private static T FindChildComponent<T>(GameObject target, string childName) where T : Component
        {
            T[] components = target.GetComponentsInChildren<T>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].name == childName)
                {
                    return components[i];
                }
            }

            return null;
        }

        private static string FormatSignedValue(float value)
        {
            if (value < 0f)
            {
                return $"-{FormatNumber(Mathf.Abs(value))}";
            }

            return $"+{FormatNumber(value)}";
        }

        private static string FormatNumber(float value)
        {
            float roundedValue = Mathf.Round(value);
            if (Mathf.Approximately(value, roundedValue))
            {
                return roundedValue.ToString("0");
            }

            return value.ToString("0.##");
        }
    }
}
