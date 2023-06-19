using RoR2;
using MonoMod.Cil;
using R2API;
using System;

namespace HIFUAcridTweaks.Misc
{
    internal class Regenerative : MiscBase
    {
        public override string Name => "Misc :::: Regenerative";

        public static float totlaMad;
        public static float duration;

        public override void Init()
        {
            totlaMad = ConfigOption(0.075f, "Heal Percent", "Decimal. Vanilla is 0.1");
            duration = ConfigOption(2f / 3f, "Buff Duration", "Vanilla is 0.5");
            base.Init();
        }

        public override void Hooks()
        {
            IL.EntityStates.Croco.Bite.OnMeleeHitAuthority += Bite_OnMeleeHitAuthority;
            IL.EntityStates.Croco.Slash.OnMeleeHitAuthority += Slash_OnMeleeHitAuthority;
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            Changes();
        }

        private void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld("RoR2.RoR2Content/Buffs", "CrocoRegen"),
                x => x.MatchCallOrCallvirt<CharacterBody>("GetBuffCount"),
                x => x.MatchConvR4(),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_maxHealth"),
                x => x.MatchMul(),
                x => x.MatchLdcR4(0.1f)))
            {
                c.Index += 6;
                c.Next.Operand = totlaMad;
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Regenerative Healing hook");
            }
        }

        private void Slash_OnMeleeHitAuthority(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.5f)))
            {
                c.Next.Operand = duration;
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Slash Regenerative Duration hook");
            }
        }

        private void Bite_OnMeleeHitAuthority(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.5f)))
            {
                c.Next.Operand = duration;
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Bite Regenerative Duration hook");
            }
        }

        private void Changes()
        {
            LanguageAPI.Add("KEYWORD_RAPID_REGEN", "<style=cKeywordName>Regenerative</style><style=cSub>Heal for <style=cIsHealing>" + Math.Round(totlaMad * duration * 100, 2) + "%</style> of your maximum health over " + Math.Round(duration, 2) + "s.</style>");
        }
    }
}