using RoR2;
using R2API;
using UnityEngine.AddressableAssets;
using RoR2.Skills;

namespace HIFUAcridTweaks.Skills
{
    internal class RavenousBite : TweakBase
    {
        public static float damage;
        public static float cooldown;
        public override string Name => "Secondary :: Ravenous Bite";

        public override string SkillToken => "secondary_alt";

        public override string DescText => "<style=cArtifact>Blighted</style>. <style=cIsDamage>Slayer</style>. <style=cIsHealing>Regenerative</style>. Bite an enemy for <style=cIsDamage>" + d(damage) + " damage</style>.";

        public override void Init()
        {
            damage = ConfigOption(2.7f, "Damage", "Decimal. Vanilla is 3.2");
            cooldown = ConfigOption(2f, "Cooldown", "Vanilla is 2");
            base.Init();
        }

        public override void Hooks()
        {
            On.EntityStates.Croco.Bite.AuthorityModifyOverlapAttack += Bite_AuthorityModifyOverlapAttack;
            Changes();
        }

        private void Bite_AuthorityModifyOverlapAttack(On.EntityStates.Croco.Bite.orig_AuthorityModifyOverlapAttack orig, EntityStates.Croco.Bite self, OverlapAttack overlapAttack)
        {
            overlapAttack.damageType |= DamageType.BonusToLowHealth;
            overlapAttack.AddModdedDamageType(Main.blight);
        }

        private void Changes()
        {
            var bite = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoBite.asset").WaitForCompletion();
            bite.baseRechargeInterval = cooldown;

            bite.keywordTokens = new string[] { "HAT_BLIGHT", "KEYWORD_SLAYER", "KEYWORD_RAPID_REGEN" };
        }
    }
}