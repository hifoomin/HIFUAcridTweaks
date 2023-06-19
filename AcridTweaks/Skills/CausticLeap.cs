using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Skills;
using UnityEngine.AddressableAssets;

namespace HIFUAcridTweaks.Skills
{
    internal class CausticLeap : TweakBase
    {
        public static float damage;
        public static float poolDamage;
        public static float cooldown;
        public override string Name => "Utility : Caustic Leap";

        public override string SkillToken => "utility";

        public override string DescText => "<style=cArtifact>Blighted</style>. <style=cIsDamage>Stunning</style>. Leap in the air, dealing <style=cIsDamage>" + d(damage) + " damage</style>. Leave acid that deals <style=cIsDamage>" + d(poolDamage) + " damage</style>.";

        public override void Init()
        {
            damage = ConfigOption(3f, "Damage", "Decimal. Vanilla is 3.2");
            poolDamage = ConfigOption(2f, "Pool Damage", "Decimal. Vanilla is 1");
            cooldown = ConfigOption(6f, "Cooldown", "Vanilla is 6");
            base.Init();
        }

        public override void Hooks()
        {
            On.EntityStates.Croco.BaseLeap.OnEnter += BaseLeap_OnEnter;
            IL.EntityStates.Croco.BaseLeap.DropAcidPoolAuthority += BaseLeap_DropAcidPoolAuthority;
            Changes();
        }

        private void BaseLeap_DropAcidPoolAuthority(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<EntityStates.BaseState>("damageStat")))
            {
                c.Emit(OpCodes.Ldc_R4, poolDamage);
                c.Emit(OpCodes.Mul);
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Caustic Leap Pool Damage hook");
            }
        }

        private void BaseLeap_OnEnter(On.EntityStates.Croco.BaseLeap.orig_OnEnter orig, EntityStates.Croco.BaseLeap self)
        {
            if (self is not EntityStates.Croco.ChainableLeap)
            {
                self.blastDamageCoefficient = damage;
            }
            if (self is EntityStates.Croco.ChainableLeap)
            {
            }
            orig(self);
        }

        private void Changes()
        {
            var cleap = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoLeap.asset").WaitForCompletion();
            cleap.baseRechargeInterval = cooldown;

            cleap.keywordTokens = new string[] { "HAT_BLIGHT", "KEYWORD_STUNNING" };
        }
    }
}