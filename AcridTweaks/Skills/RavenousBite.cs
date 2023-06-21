using RoR2;
using R2API;
using UnityEngine.AddressableAssets;
using RoR2.Skills;
using HIFUAcridTweaks.Misc;

namespace HIFUAcridTweaks.Skills
{
    internal class RavenousBite : TweakBase
    {
        public static float damage;
        public static float cooldown;
        public override string Name => "Secondary :: Ravenous Bite";

        public override string SkillToken => "secondary_alt";

        public override string DescText => "<style=cIsUtility>Agile</style>. <style=cArtifact>Blighted</style>. <style=cIsHealing>Regenerative</style>. Bite an enemy for <style=cIsDamage>" + d(damage) + " damage</style>.";

        public override void Init()
        {
            damage = ConfigOption(4.5f, "Damage", "Decimal. Vanilla is 3.2");
            cooldown = ConfigOption(3f, "Cooldown", "Vanilla is 2");
            base.Init();
        }

        public override void Hooks()
        {
            On.EntityStates.Croco.Bite.OnEnter += Bite_OnEnter;
            On.EntityStates.Croco.Bite.AuthorityModifyOverlapAttack += Bite_AuthorityModifyOverlapAttack;
            Changes();
        }

        private void Bite_OnEnter(On.EntityStates.Croco.Bite.orig_OnEnter orig, EntityStates.Croco.Bite self)
        {
            self.damageCoefficient = damage;
            orig(self);
        }

        private void Bite_AuthorityModifyOverlapAttack(On.EntityStates.Croco.Bite.orig_AuthorityModifyOverlapAttack orig, EntityStates.Croco.Bite self, OverlapAttack overlapAttack)
        {
            var passiveController = self.GetComponent<PassiveController>();
            if (passiveController != null)
            {
                switch (passiveController.currentPassive)
                {
                    case "HAT_FRENZY_NAME":
                        overlapAttack.AddModdedDamageType(Passives.frenzy);
                        break;

                    default:
                        overlapAttack.AddModdedDamageType(Passives.regen);
                        break;
                }
            }
            overlapAttack.AddModdedDamageType(Main.blight);
        }

        private void Changes()
        {
            var bite = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoBite.asset").WaitForCompletion();
            bite.baseRechargeInterval = cooldown;
            bite.cancelSprintingOnActivation = false;

            bite.keywordTokens = new string[] { "HAT_BLIGHT", "KEYWORD_AGILE", "KEYWORD_RAPID_REGEN" };
        }
    }
}