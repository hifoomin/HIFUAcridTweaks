using HACT;
using R2API;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUAcridTweaks.Skills
{
    internal class ViciousWounds : TweakBase
    {
        public bool disableCancel;
        public float duration;
        public override string Name => "Primary : Vicious Wounds";

        public override string SkillToken => "primary";

        public override string DescText => "Maul an enemy for <style=cIsDamage>200% damage</style>. Every 3rd hit is <style=cIsHealing>Regenerative</style> and deals <style=cIsDamage>400% damage</style>.";

        public override void Init()
        {
            disableCancel = ConfigOption(true, "Disable M1 Cancel?", "Vanilla is false. For a bit of a backstory, Acrid used to have a choice between dealing more dps and healing with the cancel, now it's straight up better and I can't revert it.");
            duration = ConfigOption(1.3f, "Total Duration", "Vanilla is 1.5");
            base.Init();
        }

        public override void Hooks()
        {
            On.EntityStates.Croco.Slash.OnEnter += Slash_OnEnter;
            Changes();
        }

        private void Slash_OnEnter(On.EntityStates.Croco.Slash.orig_OnEnter orig, EntityStates.Croco.Slash self)
        {
            self.baseDuration = duration;
            self.duration = duration / self.attackSpeedStat;
            EntityStates.Croco.Slash.comboFinisherBaseDurationBeforeInterruptable = duration / 1.5f;
            EntityStates.Croco.Slash.baseDurationBeforeInterruptable = duration / 2.727272727f;
            orig(self);
        }

        private void Changes()
        {
            var v = Addressables.LoadAssetAsync<SteppedSkillDef>("RoR2/Base/Croco/CrocoSlash.asset").WaitForCompletion();
            if (disableCancel)
            {
                v.canceledFromSprinting = false;
            }
            LanguageAPI.Add("CROCO_SECONDARY_ALT_DESCRIPTION", "<style=cIsHealing>Poisonous</style>. <style=cIsDamage>Slayer</style>. <style=cIsHealing>Regenerative</style>. Bite an enemy for <style=cIsDamage>320% damage</style>.");
        }
    }
}