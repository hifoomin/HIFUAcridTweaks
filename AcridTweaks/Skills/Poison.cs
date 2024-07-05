using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static R2API.DotAPI;
using static RoR2.DotController;
using System;

namespace HIFUAcridTweaks.Skills
{
    public class Poison : TweakBase
    {
        public static float duration;
        public static float percentDamagePerSecond;
        public static float dpsCap;
        public static float getReal;
        public override bool DoesNotKillTheMod => false;
        public static BuffDef poison;
        public static DotDef poisonDef;
        public static DotIndex poisonIndex;
        public override string Name => "Passive : Poison";

        public override string SkillToken => "passive";

        public override string DescText => "<style=cIsHealing>Poisonous</style> attacks apply a powerful damage-over-time.";

        public override void Init()
        {
            poison = ScriptableObject.CreateInstance<BuffDef>();
            poison.isDebuff = true;
            poison.isCooldown = false;
            poison.canStack = false;
            poison.isHidden = false;
            poison.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Croco/bdPoisoned.asset").WaitForCompletion().iconSprite;
            poison.buffColor = new Color32(201, 242, 77, 255);
            poison.name = "Poison";

            ContentAddition.AddBuffDef(poison);

            duration = ConfigOption(3f, "Duration", "Vanilla is 10");
            percentDamagePerSecond = ConfigOption(0.015f, "Percent Max Health Damage Per Second", "Decimal. Vanilla is 0.01");
            dpsCap = ConfigOption(50f, "Maximum Base Damage Per Second", "Decimal. Vanilla is 50");
            getReal = ConfigOption(1f, "Minimum Base Damage Per Second", "Decimal. Vanilla is 1");

            poisonDef = new()
            {
                associatedBuff = poison,
                damageCoefficient = 1f,
                damageColorIndex = DamageColorIndex.Poison,
                interval = 0.25f
            };
            CustomDotBehaviour behavior = delegate (DotController self, DotStack dotStack)
            {
                var attackerBody = dotStack.attackerObject?.GetComponent<CharacterBody>();
                dotStack.damage = Mathf.Min(Mathf.Max(self.victimHealthComponent ? self.victimHealthComponent.fullCombinedHealth * percentDamagePerSecond * 0.25f : 0, attackerBody.damage * getReal * 0.25f), attackerBody.damage * dpsCap * 0.25f); // 0.375% per tick, for 1.5% max hp per sec
            };
            poisonIndex = RegisterDotDef(poisonDef, behavior);

            base.Init();
        }

        public override void Hooks()
        {
            // IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            var attacker = report.attackerBody;
            if (!attacker)
            {
                return;
            }

            var victim = report.victim;
            if (!victim)
            {
                return;
            }

            if (DamageAPI.HasModdedDamageType(damageInfo, Main.poison))
            {
                InflictDotInfo dotInfo = new()
                {
                    victimObject = victim.gameObject,
                    attackerObject = attacker.gameObject,
                    totalDamage = null,
                    dotIndex = poisonIndex,
                    duration = duration,
                    damageMultiplier = 1f,
                    maxStacksFromAttacker = 1
                };
                InflictDot(ref dotInfo);

                var master = attacker.master;
                if (master && master.playerStatsComponent)
                {
                    master.playerStatsComponent.currentStats.PushStatValue(RoR2.Stats.StatDef.totalCrocoInfectionsInflicted, 1UL);
                }
            }
        }

        private void GlobalEventManager_OnHitEnemy(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcI4(4096)))
            {
                c.Index++;
                c.EmitDelegate<Func<int, int>>((useless) =>
                {
                    return 0;
                });
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Poison Deletion hook");
            }
        }
    }
}