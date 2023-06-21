using HIFUAcridTweaks.Misc;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUAcridTweaks.Skills
{
    internal class FrenziedLeap : TweakBase
    {
        public static float damage;
        public static float cdr;
        public static float cooldown;
        public static float radius;
        public override string Name => "Utility :: Frenzied Leap";

        public override string SkillToken => "utility_alt1";

        public override string DescText => "<style=cIsHealing>Regenerative</style>. <style=cIsDamage>Stunning</style>. Leap in the air, dealing <style=cIsDamage>" + d(damage) + " damage</style> in a small area.";

        public override void Init()
        {
            damage = ConfigOption(6f, "Damage", "Decimal. Vanilla is 5.5");
            cooldown = ConfigOption(7f, "Cooldown", "Vanilla is 10");
            radius = ConfigOption(6.5f, "Area of Effect", "Vanilla is 10");

            base.Init();
        }

        public override void Hooks()
        {
            On.EntityStates.Croco.BaseLeap.OnEnter += BaseLeap_OnEnter;
            On.EntityStates.Croco.BaseLeap.OnExit += BaseLeap_OnExit;
            On.EntityStates.Croco.BaseLeap.DetonateAuthority += BaseLeap_DetonateAuthority;
            On.EntityStates.Croco.ChainableLeap.DoImpactAuthority += ChainableLeap_DoImpactAuthority;
            Changes();
        }

        private BlastAttack.Result BaseLeap_DetonateAuthority(On.EntityStates.Croco.BaseLeap.orig_DetonateAuthority orig, EntityStates.Croco.BaseLeap self)
        {
            Vector3 footPosition = self.characterBody.footPosition;
            EffectManager.SpawnEffect(self.blastEffectPrefab, new EffectData
            {
                origin = footPosition,
                scale = radius
            }, true);
            var ba = new BlastAttack
            {
                attacker = self.gameObject,
                baseDamage = self.damageStat * self.blastDamageCoefficient,
                baseForce = self.blastForce,
                bonusForce = self.blastBonusForce,
                crit = self.isCritAuthority,
                falloffModel = BlastAttack.FalloffModel.None,
                procCoefficient = EntityStates.Croco.BaseLeap.blastProcCoefficient,
                radius = radius,
                damageType = DamageType.Stun1s,
                position = footPosition,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                impactEffect = EffectCatalog.FindEffectIndexFromPrefab(self.blastImpactEffectPrefab),
                teamIndex = self.teamComponent.teamIndex
            };

            var passiveController = self.GetComponent<PassiveController>();
            if (passiveController != null)
            {
                switch (passiveController.currentPassive)
                {
                    case "KEYWORD_RAPID_SPEED":
                        ba.AddModdedDamageType(Passives.frenzy);
                        break;

                    default:
                        ba.AddModdedDamageType(Passives.regen);
                        break;
                }
            }

            return ba.Fire();
        }

        private void BaseLeap_OnExit(On.EntityStates.Croco.BaseLeap.orig_OnExit orig, EntityStates.Croco.BaseLeap self)
        {
            orig(self);
            self.characterBody.isSprinting = true;
        }

        private void ChainableLeap_DoImpactAuthority(On.EntityStates.Croco.ChainableLeap.orig_DoImpactAuthority orig, EntityStates.Croco.ChainableLeap self)
        {
            EntityStates.Croco.ChainableLeap.refundPerHit = 0f;
            orig(self);
        }

        private void BaseLeap_OnEnter(On.EntityStates.Croco.BaseLeap.orig_OnEnter orig, EntityStates.Croco.BaseLeap self)
        {
            if (self is EntityStates.Croco.ChainableLeap)
            {
                self.blastDamageCoefficient = damage;
            }
            orig(self);
        }

        private void Changes()
        {
            var fleap = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoChainableLeap.asset").WaitForCompletion();
            fleap.baseRechargeInterval = cooldown;

            fleap.keywordTokens = new string[] { "KEYWORD_RAPID_REGEN", "KEYWORD_STUNNING" };
        }
    }
}