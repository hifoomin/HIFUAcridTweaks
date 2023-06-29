using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2.Achievements;
using System;

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
            IL.RoR2.Achievements.BaseStatMilestoneAchievement.ProgressForAchievement += BaseStatMilestoneAchievement_ProgressForAchievement;
            IL.RoR2.Achievements.BaseStatMilestoneAchievement.Check += BaseStatMilestoneAchievement_Check;
            Changes();
        }

        private void BaseStatMilestoneAchievement_Check(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<BaseStatMilestoneAchievement>("get_statRequirement")))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<ulong, BaseStatMilestoneAchievement, ulong>>((orig, self) =>
                {
                    if (self is RoR2.Achievements.Croco.CrocoTotalInfectionsMilestoneAchievement)
                    {
                        return poisonCount;
                    }
                    return orig;
                });
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Poison Count 2 hook");
            }
        }

        private void BaseStatMilestoneAchievement_ProgressForAchievement(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<BaseStatMilestoneAchievement>("get_statRequirement")))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<ulong, BaseStatMilestoneAchievement, ulong>>((orig, self) =>
                {
                    if (self is RoR2.Achievements.Croco.CrocoTotalInfectionsMilestoneAchievement)
                    {
                        return poisonCount;
                    }
                    return orig;
                });
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Poison Count 1 hook");
            }
        }

        private void Changes()
        {
            LanguageAPI.Add("ACHIEVEMENT_CROCOTOTALINFECTIONSMILESTONE_DESCRIPTION", "As Acrid, inflict Poison " + poisonCount + " total times.");
        }
    }
}