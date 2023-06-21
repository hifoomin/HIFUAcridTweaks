using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;
using MonoMod.Cil;
using System;
using RoR2;
using UnityEngine.Networking;
using RoR2.Orbs;

namespace HIFUAcridTweaks.Skills
{
    internal class Epidemic : TweakBase
    {
        public static float damage;
        public static float cooldown;
        public static int maxTargets;
        public static float maxDistance;
        public static float shareDuration;
        public static DamageAPI.ModdedDamageType shared = DamageAPI.ReserveDamageType();
        public static BuffDef shareDamage;
        public static ProcType sharedMask = (ProcType)12561269;
        public override string Name => "Special : Epidemic";

        public override string SkillToken => "special";

        public override string DescText => "<style=cArtifact>Blighted</style>. Release a deadly disease that deals <style=cIsDamage>" + d(damage) + " damage</style> and spreads up to <style=cIsDamage>" + maxTargets + "</style> times. All enemies hit <style=cIsDamage>share damage taken</style> for <style=cIsDamage>" + shareDuration + "s</style>.";

        public override void Init()
        {
            shareDamage = ScriptableObject.CreateInstance<BuffDef>();
            shareDamage.isDebuff = true;
            shareDamage.canStack = false;
            shareDamage.isCooldown = false;

            shareDamage.isHidden = true;
            shareDamage.name = "Epidemic Shared Damage";

            ContentAddition.AddBuffDef(shareDamage);

            damage = ConfigOption(1.5f, "Damage", "Decimal. Vanilla is 1");
            cooldown = ConfigOption(10f, "Cooldown", "Vanilla is 10");
            maxTargets = ConfigOption(4, "Max Targets", "Vanilla is 41");
            maxDistance = ConfigOption(45f, "Max Range", "Vanilla is 30");
            shareDuration = ConfigOption(5f, "Damage Sharing Duration", "");
            base.Init();
        }

        public override void Hooks()
        {
            IL.RoR2.Orbs.LightningOrb.Begin += LightningOrb_Begin;
            On.EntityStates.Croco.FireSpit.OnEnter += FireSpit_OnEnter;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            Changes();
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo info)
        {
            orig(self, info);
            if (NetworkServer.active && info.attacker && info.attacker.GetComponent<TeamComponent>())
            {
                if (DamageAPI.HasModdedDamageType(info, shared))
                {
                    self.body.AddTimedBuffAuthority(shareDamage.buffIndex, shareDuration);
                }

                if (self.body.HasBuff(shareDamage) && !info.procChainMask.HasProc(sharedMask))
                {
                    SphereSearch search = new()
                    {
                        origin = info.position,
                        radius = 2000f,
                        mask = LayerIndex.entityPrecise.mask,
                        queryTriggerInteraction = QueryTriggerInteraction.Ignore
                    };

                    var teamIndex = info.attacker.GetComponent<TeamComponent>().teamIndex;

                    search.RefreshCandidates();
                    search.FilterCandidatesByDistinctHurtBoxEntities();
                    HurtBox[] boxes = search.GetHurtBoxes();
                    foreach (HurtBox box in boxes)
                    {
                        if (box.teamIndex != teamIndex && box.healthComponent && box.healthComponent != self && box.healthComponent.body.HasBuff(shareDamage))
                        {
                            LightningOrb orb = new()
                            {
                                lightningType = LightningOrb.LightningType.CrocoDisease,
                                canBounceOnSameTarget = false,
                                bouncesRemaining = maxTargets - 1,
                                damageColorIndex = DamageColorIndex.Poison,
                                damageValue = info.damage,
                                damageCoefficientPerBounce = 1f,
                                attacker = info.attacker,
                                isCrit = info.crit,
                                target = box,
                                teamIndex = teamIndex,
                                targetsToFindPerBounce = 1,
                                speed = 20000f,
                                origin = info.position,
                                procCoefficient = 0f
                            };

                            ProcChainMask mask = new();
                            mask.AddProc(sharedMask);

                            orb.procChainMask = mask;

                            OrbManager.instance.AddOrb(orb);
                        }
                    }
                }
            }
        }

        private void FireSpit_OnEnter(On.EntityStates.Croco.FireSpit.orig_OnEnter orig, EntityStates.Croco.FireSpit self)
        {
            self.baseDuration = 0.3f;

            if (self is EntityStates.Croco.FireDiseaseProjectile)
            {
                self.damageCoefficient = damage;
            }

            orig(self);
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
                c.Next.Operand = 0.25f;
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
            holder.Add(shared);

            var simple = projectile.GetComponent<ProjectileSimple>();
            simple.lifetime = 10f;
            simple.desiredForwardSpeed = 100f;

            var prox = projectile.GetComponent<ProjectileProximityBeamController>();
            prox.attackRange = maxDistance;
            prox.bounces = maxTargets;

            var rigidBody = projectile.GetComponent<Rigidbody>();
            rigidBody.useGravity = true;
            rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidBody.freezeRotation = true;

            var antiGravityForce = projectile.AddComponent<AntiGravityForce>();
            antiGravityForce.rb = projectile.GetComponent<Rigidbody>();
            antiGravityForce.antiGravityCoefficient = 0.25f;

            // shared damage with targets inflicted would be cool
        }
    }
}