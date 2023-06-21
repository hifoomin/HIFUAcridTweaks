using RoR2;
using MonoMod.Cil;
using R2API;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;
using RoR2.Skills;
using UnityEngine.Networking.Types;
using System.Linq;
using HIFUAcridTweaks.Skills;

namespace HIFUAcridTweaks.Misc
{
    internal class Passives : MiscBase
    {
        public override string Name => "Misc :::: Passives";

        public static float regenHeal;
        public static float regenDur;
        public static float frenSpeed;
        public static float frenDur;
        public static SkillDef regenerativeSD;
        public static SkillDef frenziedSD;
        public static BuffDef regenerative;
        public static BuffDef frenzied;
        public static ModdedDamageType regen = ReserveDamageType();
        public static ModdedDamageType frenzy = ReserveDamageType();
        public static BodyIndex acridBodyIndex;

        public override void Init()
        {
            var passiveFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Croco/CrocoBodyPassiveFamily.asset").WaitForCompletion();

            regenerativeSD = ScriptableObject.CreateInstance<SkillDef>();
            (regenerativeSD as ScriptableObject).name = "Regenerative Passive";
            regenerativeSD.skillName = "Regenerative";
            regenerativeSD.skillNameToken = "HAT_REGEN_NAME";
            regenerativeSD.skillDescriptionToken = "HAT_REGEN_DESCRIPTION";
            regenerativeSD.keywordTokens = new string[] { "KEYWORD_RAPID_REGEN" };
            regenerativeSD.activationStateMachineName = "Weapon";
            regenerativeSD.activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle));
            regenerativeSD.interruptPriority = EntityStates.InterruptPriority.Skill;
            regenerativeSD.baseRechargeInterval = 1f;
            regenerativeSD.baseMaxStock = 1;
            regenerativeSD.rechargeStock = 1;
            regenerativeSD.requiredStock = 1;
            regenerativeSD.stockToConsume = 1;
            regenerativeSD.resetCooldownTimerOnUse = false;
            regenerativeSD.fullRestockOnAssign = true;
            regenerativeSD.dontAllowPastMaxStocks = false;
            regenerativeSD.beginSkillCooldownOnSkillEnd = false;
            regenerativeSD.cancelSprintingOnActivation = true;
            regenerativeSD.forceSprintDuringState = false;
            regenerativeSD.canceledFromSprinting = false;
            regenerativeSD.isCombatSkill = true;
            regenerativeSD.mustKeyPress = false;
            regenerativeSD.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoPassivePoison.asset").WaitForCompletion().icon;

            ContentAddition.AddSkillDef(regenerativeSD);

            frenziedSD = ScriptableObject.CreateInstance<SkillDef>();
            (frenziedSD as ScriptableObject).name = "Frenzied Passive";
            frenziedSD.skillName = "Frenzied";
            frenziedSD.skillNameToken = "HAT_FRENZY_NAME";
            frenziedSD.skillDescriptionToken = "HAT_FRENZY_DESCRIPTION";
            frenziedSD.keywordTokens = new string[] { "KEYWORD_RAPID_SPEED" };
            frenziedSD.activationStateMachineName = "Weapon";
            frenziedSD.activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle));
            frenziedSD.interruptPriority = EntityStates.InterruptPriority.Skill;
            frenziedSD.baseRechargeInterval = 1f;
            frenziedSD.baseMaxStock = 1;
            frenziedSD.rechargeStock = 1;
            frenziedSD.requiredStock = 1;
            frenziedSD.stockToConsume = 1;
            frenziedSD.resetCooldownTimerOnUse = false;
            frenziedSD.fullRestockOnAssign = true;
            frenziedSD.dontAllowPastMaxStocks = false;
            frenziedSD.beginSkillCooldownOnSkillEnd = false;
            frenziedSD.cancelSprintingOnActivation = true;
            frenziedSD.forceSprintDuringState = false;
            frenziedSD.canceledFromSprinting = false;
            frenziedSD.isCombatSkill = true;
            frenziedSD.mustKeyPress = false;
            frenziedSD.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoPassiveBlight.asset").WaitForCompletion().icon;

            ContentAddition.AddSkillDef(frenziedSD);

            passiveFamily.variants[0].skillDef = regenerativeSD;
            passiveFamily.variants[1].skillDef = frenziedSD;

            regenerative = ScriptableObject.CreateInstance<BuffDef>();
            regenerative.isDebuff = false;
            regenerative.isCooldown = false;
            regenerative.canStack = true;
            regenerative.isHidden = false;
            regenerative.buffColor = new Color32(201, 242, 77, 255);
            regenerative.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Croco/bdCrocoRegen.asset").WaitForCompletion().iconSprite;
            regenerative.name = "Regenerative";

            frenzied = ScriptableObject.CreateInstance<BuffDef>();
            frenzied.isDebuff = false;
            frenzied.isCooldown = false;
            frenzied.canStack = true;
            frenzied.isHidden = false;
            frenzied.buffColor = new Color32(201, 242, 77, 255);
            frenzied.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/DLC1/MoveSpeedOnKill/bdKillMoveSpeed.asset").WaitForCompletion().iconSprite;
            frenzied.name = "Frenzied";

            ContentAddition.AddBuffDef(regenerative);
            ContentAddition.AddBuffDef(frenzied);

            regenHeal = ConfigOption(0.05f, "Regenerative Heal Percent", "Decimal. Vanilla is 0.05");
            regenDur = ConfigOption(0.75f, "Regenerative Buff Duration", "Vanilla is 0.5");
            frenSpeed = ConfigOption(0.15f, "Frenzied Movement Speed", "Decimal.");
            frenDur = ConfigOption(3f, "Frenzied Buff Duration", "");
            base.Init();
        }

        public override void Hooks()
        {
            On.RoR2.BodyCatalog.Init += BodyCatalog_Init;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.EntityStates.Croco.Slash.OnMeleeHitAuthority += Slash_OnMeleeHitAuthority1;
            On.EntityStates.Croco.Bite.OnMeleeHitAuthority += Bite_OnMeleeHitAuthority1;
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            On.RoR2.HealthComponent.ServerFixedUpdate += HealthComponent_ServerFixedUpdate;
            Changes();
        }

        private void Bite_OnMeleeHitAuthority1(On.EntityStates.Croco.Bite.orig_OnMeleeHitAuthority orig, EntityStates.Croco.Bite self)
        {
            self.hasGrantedBuff = true;
            orig(self);
        }

        private void Slash_OnMeleeHitAuthority1(On.EntityStates.Croco.Slash.orig_OnMeleeHitAuthority orig, EntityStates.Croco.Slash self)
        {
            self.hasGrantedBuff = true;
            orig(self);
        }

        private void BodyCatalog_Init(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();
            acridBodyIndex = BodyCatalog.FindBodyIndex("CrocoBody");
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (body.bodyIndex == acridBodyIndex)
            {
                // Main.HACTLogger.LogError("acrid spawned");
                if (body.GetComponent<PassiveController>() == null)
                {
                    body.gameObject.AddComponent<PassiveController>();
                }
            }
        }

        private void HealthComponent_ServerFixedUpdate(On.RoR2.HealthComponent.orig_ServerFixedUpdate orig, HealthComponent self)
        {
            if (self.alive && self.body)
            {
                /*

                totalTicks = heal duration (0.75) / Time.fixedDeltaTime
                healTotal = full combined health (140) * % heal (0.075)

                regenAccumulator += healTotal / totalTicks

                */
                self.regenAccumulator += (self.fullCombinedHealth * regenHeal) / (regenDur / Time.fixedDeltaTime) * self.body.GetBuffCount(regenerative);
            }
            orig(self);
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var attacker = report.attackerBody;
            if (!attacker)
            {
                return;
            }

            if (report.damageInfo.HasModdedDamageType(regen))
            {
                attacker.AddTimedBuffAuthority(regenerative.buffIndex, regenDur);
            }

            if (report.damageInfo.HasModdedDamageType(frenzy))
            {
                attacker.AddTimedBuffAuthority(frenzied.buffIndex, frenDur);
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender)
            {
                args.moveSpeedMultAdd += frenSpeed * sender.GetBuffCount(frenzied);
            }
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
                c.Next.Operand = 0f;
            }
            else
            {
                Main.HACTLogger.LogError("Failed to apply Regenerative Healing hook");
            }
        }

        private void Changes()
        {
            LanguageAPI.Add("HAT_REGEN_NAME", "Regenerative");
            LanguageAPI.Add("HAT_REGEN_DESCRIPTION", "<style=cIsHealing>Regenerative</style> attacks heal over a short duration.");
            LanguageAPI.Add("HAT_FRENZY_NAME", "Frenzied");
            LanguageAPI.Add("HAT_FRENZY_DESCRIPTION", "Attacks that apply <style=cIsHealing>Regenerative</style> apply <style=cIsDamage>Frenzied</style> instead, which increases movement speed for a short duration.");
            LanguageAPI.Add("KEYWORD_RAPID_REGEN", "<style=cKeywordName>Regenerative</style><style=cSub>Heal for <style=cIsHealing>" + Math.Round(regenHeal * 100, 2) + "%</style> of your maximum health over " + Math.Round(regenDur, 2) + "s. <i>Can stack.</i></style>");
            LanguageAPI.Add("KEYWORD_RAPID_SPEED", "<style=cKeywordName>Frenzied</style><style=cSub>Gain <style=cIsUtility>" + Math.Round(frenSpeed * 100, 2) + "%</style> movement speed for " + Math.Round(frenDur, 2) + "s. <i>Can stack.</i></style>");
        }
    }
}