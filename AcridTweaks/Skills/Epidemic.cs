using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;
using MonoMod.Cil;
using System;

namespace HIFUAcridTweaks.Skills
{
    internal class Epidemic : TweakBase
    {
        public static float damage;
        public static float cooldown;
        public override string Name => "Special : Epidemic";

        public override string SkillToken => "special";

        public override string DescText => "<style=cArtifact>Blighted</style>. Release a deadly disease that deals <style=cIsDamage>" + d(damage) + " damage</style>. The missile knows where it is at all times, it knows this because it knows where it isn't. By subtracting iowasdjoasudjas9du";

        public override void Init()
        {
            damage = ConfigOption(3f, "Damage", "Decimal. Vanilla is 1");
            cooldown = ConfigOption(10f, "Cooldown", "Vanilla is 10");
            base.Init();
        }

        public override void Hooks()
        {
            IL.RoR2.Orbs.LightningOrb.Begin += LightningOrb_Begin;
            Changes();
        }

        private void LightningOrb_Begin(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.6f),
                x => x.MatchCallOrCallvirt<RoR2.Orbs.Orb>("set_duration"),
                x => x.MatchLdarg(out _),
                x => x.MatchLdcI4(2)))
            {
                c.Next.Operand = 0.5f;
                c.Index += 4;
                c.EmitDelegate<Func<int, int>>((useless) =>
                {
                    return 1;
                });
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Epidemic Un-J hook");
            }
        }

        private void Changes()
        {
            var eepy = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoDisease.asset").WaitForCompletion();
            eepy.baseRechargeInterval = cooldown;

            eepy.keywordTokens = new string[] { "HAT_BLIGHT" };

            var projectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoDiseaseProjectile.prefab").WaitForCompletion();
            var holder = projectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            holder.Add(Main.blight);

            var simple = projectile.GetComponent<ProjectileSimple>();
            simple.lifetime = 10f;
            simple.desiredForwardSpeed = 100f;

            // shared damage with targets inflicted would be cool
        }
    }
}