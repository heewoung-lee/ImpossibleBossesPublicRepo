using System;
using System.Collections.Generic;
using DataType;
using DataType.Skill.Factory.Effect.Def;
using DataType.Skill.Factory.Sequence.Def;
using DataType.Skill.Factory.Sequence.GetLength.Def;
using DataType.Skill.ShareDataDef;
using Stats;
using UnityEngine;

namespace DataType.Skill
{
    public static class SkillDescriptionKeywordResolver
    {
        private static readonly Dictionary<string, Func<SkillDataSO, PlayerStats, string>> Resolvers =
            new Dictionary<string, Func<SkillDataSO, PlayerStats, string>>
            {
                { "AttackDamage", ResolveAttackDamage },
                { "EffectValue", ResolveEffectValue },
                { "EffectDuration", ResolveEffectDuration },
                { "ProjectileCount", ResolveProjectileCount },
                { "HitCount", ResolveHitCount },
                { "CastTime", ResolveCastTime },
            };

        public static string Resolve(string keyword, BaseDataSO data, PlayerStats stats)
        {
            SkillDataSO skillData = data as SkillDataSO;
            if (skillData == null)
            {
                return keyword;
            }

            Func<SkillDataSO, PlayerStats, string> resolver;
            if (Resolvers.TryGetValue(keyword, out resolver) == false)
            {
                return keyword;
            }

            return resolver.Invoke(skillData, stats);
        }

        private static string ResolveAttackDamage(SkillDataSO skillData, PlayerStats stats)
        {
            if (skillData.effect is AttackEffectDef attackEffect)
            {
                float finalDamage = stats.Attack * attackEffect.multiplier + attackEffect.additional;
                return FormatColoredNumber(finalDamage, "red");
            }

            if (skillData.effect is BufferEffectDef bufferEffect && bufferEffect.useCasterAttack)
            {
                float finalDamage = stats.Attack * bufferEffect.attackMultiplier;
                return FormatColoredNumber(finalDamage, "green");
            }

            if (skillData.effect is ArcSpreadProjectileDef)
            {
                return FormatColoredNumber(stats.Attack, "red");
            }

            if (skillData.effect is GenerateProjectileDef projectileEffect)
            {
                float finalDamage = stats.Attack * projectileEffect.floatParams;
                return FormatColoredNumber(finalDamage, "red");
            }

            return "AttackDamage";
        }

        private static string ResolveEffectValue(SkillDataSO skillData, PlayerStats stats)
        {
            if (skillData.effect is BufferEffectDef bufferEffect)
            {
                float value = bufferEffect.Value;
                string color = value < 0f ? "red" : "green";
                return FormatColoredNumber(Mathf.Abs(value), color);
            }

            return "EffectValue";
        }

        private static string ResolveEffectDuration(SkillDataSO skillData, PlayerStats stats)
        {
            if (skillData.effect is BufferEffectDef bufferEffect)
            {
                float duration = ResolveDuration(skillData, bufferEffect.skillduration);
                if (duration >= 0f)
                {
                    return FormatColoredNumber(duration, "yellow");
                }
            }

            if (skillData.effect is CrowdControlDef crowdControlEffect)
            {
                float duration = ResolveDuration(skillData, crowdControlEffect.hitVfxDuration);
                if (duration >= 0f)
                {
                    return FormatColoredNumber(duration, "yellow");
                }
            }

            return "EffectDuration";
        }

        private static float ResolveDuration(SkillDataSO skillData, DurationRefDef durationRef)
        {
            if (durationRef.Source == DurationSourceType.None)
            {
                return -1f;
            }

            if (durationRef.Source == DurationSourceType.UseShared)
            {
                SkillSharedDurationDef sharedDuration = FindSharedDuration(skillData);
                if (sharedDuration != null)
                {
                    return sharedDuration.commonDuration;
                }
            }

            return durationRef.Resolve(null);
        }

        private static SkillSharedDurationDef FindSharedDuration(SkillDataSO skillData)
        {
            for (int i = 0; i < skillData.sharedDefs.Count; i++)
            {
                if (skillData.sharedDefs[i] is SkillSharedDurationDef sharedDuration)
                {
                    return sharedDuration;
                }
            }

            return null;
        }

        private static string ResolveProjectileCount(SkillDataSO skillData, PlayerStats stats)
        {
            if (skillData.effect is ArcSpreadProjectileDef projectileEffect)
            {
                return FormatColoredNumber(projectileEffect.projectileCount, "blue");
            }

            return "ProjectileCount";
        }

        private static string ResolveHitCount(SkillDataSO skillData, PlayerStats stats)
        {
            if (skillData.sequence is MeleeComboSequenceDef meleeCombo && meleeCombo.hits != null)
            {
                return FormatColoredNumber(meleeCombo.hits.Length, "yellow");
            }

            if (skillData.sequence is ChannelingSequenceDef channeling && channeling.hits != null)
            {
                return FormatColoredNumber(channeling.hits.Length, "yellow");
            }

            return "HitCount";
        }

        private static string ResolveCastTime(SkillDataSO skillData, PlayerStats stats)
        {
            if (skillData.sequence is ChannelingSequenceDef channeling &&
                channeling.channelingLength is LengthCustom length)
            {
                return FormatColoredNumber(length.seconds, "yellow");
            }

            return "CastTime";
        }

        private static string FormatColoredNumber(float value, string color)
        {
            return $"<color={color}>{FormatNumber(value)}</color>";
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
