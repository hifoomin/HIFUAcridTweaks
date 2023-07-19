using RoR2.Projectile;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;
using MonoMod.Cil;
using System;

namespace HIFUAcridTweaks.Skills
{
    internal class Neurotoxin : TweakBase
    {
        public static float damage;
        public static float cooldown;
        public static float aoe;
        public override bool DoesNotKillTheMod => true;
        public override string Name => "Secondary : Neurotoxin";

        public override string SkillToken => "secondary";

        public override string DescText => "<style=cIsUtility>Agile</style>. <style=cArtifact>Blighted</style>. Spit toxic bile for <style=cIsDamage>" + d(damage) + " damage</style>.";

        public override void Init()
        {
            damage = ConfigOption(2.6f, "Damage", "Decimal. Vanilla is 2.4");
            cooldown = ConfigOption(3f, "Cooldown", "Vanilla is 2");
            aoe = ConfigOption(8f, "Area of Effect", "Vanilla is 3");
            base.Init();
        }

        public override void Hooks()
        {
            On.EntityStates.Croco.FireSpit.OnEnter += FireSpit_OnEnter1;
            IL.EntityStates.Croco.FireSpit.OnEnter += FireSpit_OnEnter;
            Changes();
        }

        private void FireSpit_OnEnter1(On.EntityStates.Croco.FireSpit.orig_OnEnter orig, EntityStates.Croco.FireSpit self)
        {
            self.baseDuration = 0.3f;

            if (self is not EntityStates.Croco.FireDiseaseProjectile)
            {
                self.damageCoefficient = damage;
            }

            orig(self);
        }

        private void FireSpit_OnEnter(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchStfld("RoR2.Projectile.FireProjectileInfo", "damage"),
                x => x.MatchLdloca(out _)))
            {
                c.Index += 3;
                c.EmitDelegate<Func<int, int>>((useless) =>
                {
                    return 0;
                });
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Spit Damage Type hook");
            }
        }

        private void Changes()
        {
            var neuro = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoSpit.asset").WaitForCompletion();
            neuro.baseRechargeInterval = cooldown;

            neuro.keywordTokens = new string[] { "HAT_BLIGHT" };

            var projectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoSpit.prefab").WaitForCompletion();
            var holder = projectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            holder.Add(Main.blight);

            projectile.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

            var ghost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoSpitGhost.prefab").WaitForCompletion();
            ghost.transform.localScale = new Vector3(2f, 2f, 2f);

            var simple = projectile.GetComponent<ProjectileSimple>();
            simple.desiredForwardSpeed = 100f;
            simple.lifetime = 10f;

            var rigidBody = projectile.GetComponent<Rigidbody>();
            rigidBody.useGravity = true;
            rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidBody.freezeRotation = true;

            var antiGravityForce = projectile.AddComponent<AntiGravityForce>();
            antiGravityForce.rb = projectile.GetComponent<Rigidbody>();
            antiGravityForce.antiGravityCoefficient = 0.25f;

            var impact = projectile.GetComponent<ProjectileImpactExplosion>();
            impact.blastRadius = aoe;
            impact.lifetime = 10f;

            var impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoDiseaseImpactEffect.prefab").WaitForCompletion();
            impactEffect.transform.localScale = new Vector3(aoe, aoe, aoe);

            var flash = impactEffect.transform.GetChild(1).GetComponent<ParticleSystem>().main;
            flash.startLifetime = 0.33f;

            var theFucking = new Gradient();
            theFucking.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 0.165f), new GradientColorKey(Color.black, 0.33f) }, new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.165f), new GradientAlphaKey(0f, 0.33f) });

            var flash2 = impactEffect.transform.GetChild(1).GetComponent<ParticleSystem>().colorOverLifetime;
            flash2.color = theFucking;

            var flash3 = impactEffect.transform.GetChild(1).GetComponent<ParticleSystem>().sizeOverLifetime;
            flash3.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0.165f), new Keyframe(0f, 0.33f)));

            var ring = impactEffect.transform.GetChild(2).GetComponent<ParticleSystemRenderer>();
            ring.mesh = Addressables.LoadAssetAsync<Mesh>("RoR2/Base/Common/VFX/mdlVFXDonut2.fbx").WaitForCompletion();

            var jjj = ring.gameObject.GetComponent<ParticleSystem>().main;
            jjj.startLifetime = 0.5f;

            var j = ring.gameObject.GetComponent<ParticleSystem>().sizeOverLifetime;
            j.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0.1f), new Keyframe(0f, 0.5f)));

            var h = ring.gameObject.GetComponent<ParticleSystem>().colorOverLifetime;

            var theFuckingUnity = new Gradient();
            theFuckingUnity.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.white, 0.1f), new GradientColorKey(Color.black, 0.5f) }, new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(0f, 0.5f) });

            h.color = theFuckingUnity;

            var projectileDamage = projectile.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;
        }
    }
}