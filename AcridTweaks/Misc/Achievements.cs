using R2API;

namespace HIFUAcridTweaks.Misc
{
    internal class Achievements : MiscBase
    {
        public override string Name => "Misc ::::: Achievements";
        public override bool DoesNotKillTheMod => true;
        public static ulong poisonCount;

        public override void Init()
        {
            poisonCount = ConfigOption(100UL, "Acrid: Pandemic Poison Requirement Amount", "Vanilla is 1000");
            base.Init();
        }

        public override void Hooks()
        {
            On.RoR2.Achievements.BaseStatMilestoneAchievement.OnInstall += BaseStatMilestoneAchievement_OnInstall;
            Changes();
        }

        private void BaseStatMilestoneAchievement_OnInstall(On.RoR2.Achievements.BaseStatMilestoneAchievement.orig_OnInstall orig, RoR2.Achievements.BaseStatMilestoneAchievement self)
        {
            if (self is RoR2.Achievements.Croco.CrocoTotalInfectionsMilestoneAchievement)
            {
                Main.HACTLogger.LogError("stat req pre is " + self.statRequirement);
                R2API.Utils.Reflection.SetFieldValue(self.statRequirement, "get_statRequirement", poisonCount);
                Main.HACTLogger.LogError("stat req POST is " + self.statRequirement);
            }
            orig(self);
        }

        private void Changes()
        {
            LanguageAPI.Add("ACHIEVEMENT_CROCOTOTALINFECTIONSMILESTONE_DESCRIPTION", "As Acrid, inflict Poison " + poisonCount + " total times.");
        }
    }
}