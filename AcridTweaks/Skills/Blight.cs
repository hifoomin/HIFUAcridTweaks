using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DotAPI;
using static RoR2.DotController;

namespace HIFUAcridTweaks.Skills
{
    public class Blight : TweakBase
    {
        public static float duration;
        public static float damagePerSecond;
        public override bool DoesNotKillTheMod => false;

        public static BuffDef blight;
        public static DotDef blightDef;
        public static DotIndex blightIndex;
        public override string Name => "Passive : Blight";

        public override string SkillToken => "passive_alt";

        public override string DescText => "<style=cArtifact>Blighted</style> attacks apply a stacking damage-over-time.";

        public override void Init()
        {
            blight = ScriptableObject.CreateInstance<BuffDef>();
            blight.isDebuff = true;
            blight.isCooldown = false;
            blight.canStack = true;
            blight.isHidden = false;
            blight.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Croco/bdBlight.asset").WaitForCompletion().iconSprite;
            blight.buffColor = new Color32(0, 56, 127, 255); // 177
            blight.name = "Blight";

            ContentAddition.AddBuffDef(blight);

            duration = ConfigOption(3f, "Duration", "Vanilla is 5");
            damagePerSecond = ConfigOption(1.1f, "Base Damage Per Second", "Decimal. Vanilla is 0.6");

            blightDef = new()
            {
                associatedBuff = blight,
                damageCoefficient = 1f,
                damageColorIndex = DamageColorIndex.Void,
                interval = 0.2f
            };
            CustomDotBehaviour behavior = delegate (DotController self, DotStack dotStack)
            {
                var attackerBody = dotStack.attackerObject?.GetComponent<CharacterBody>();
                dotStack.damage = attackerBody.damage * damagePerSecond * 0.2f; // 22% per tick, for 110% dps
            };
            blightIndex = RegisterDotDef(blightDef, behavior);

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

            var attacker = report.attacker;
            if (!attacker)
            {
                return;
            }

            var victim = report.victim;
            if (!victim)
            {
                return;
            }

            if (DamageAPI.HasModdedDamageType(damageInfo, Main.blight))
            {
                InflictDotInfo dotInfo = new()
                {
                    victimObject = victim.gameObject,
                    attackerObject = attacker,
                    totalDamage = null,
                    dotIndex = blightIndex,
                    duration = duration,
                    damageMultiplier = 1f,
                    maxStacksFromAttacker = uint.MaxValue,
                };
                InflictDot(ref dotInfo);
            }
        }

        private void GlobalEventManager_OnHitEnemy(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcI4(1048576)))
            {
                c.Index++;
                c.EmitDelegate<Func<int, int>>((useless) =>
                {
                    return 0;
                });
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Blight Deletion hook");
            }
        }
    }
}