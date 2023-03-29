using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HIFUAcridTweaks;
using HIFUAcridTweaks.Skilldefs;
using R2API;
using R2API.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HIFUAcridTweaks.VFX;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using RoR2.Skills;

namespace HACT
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(R2APIContentManager.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "HIFU";
        public const string PluginName = "HIFUAcridTweaks";
        public const string PluginVersion = "1.0.6";

        public static ConfigFile HACTConfig;
        public static ManualLogSource HACTLogger;

        private string version = PluginVersion;

        public static ConfigEntry<float> newrotoxinDamage;
        public static ConfigEntry<float> newrotoxinRange;
        public static ConfigEntry<float> newrotoxinRadius;
        public static ConfigEntry<float> newrotoxinProcCoeff;
        public static ConfigEntry<bool> enableNewrotoxin;

        public void Awake()
        {
            HACTLogger = Logger;
            HACTConfig = Config;

            enableNewrotoxin = Config.Bind("Secondary : Neurotoxin", "Enable Rework?", true, "Vanilla is false");
            newrotoxinDamage = Config.Bind("Secondary : Neurotoxin", "Damage", 3.2f, "Decimal. Default is 3.2");
            newrotoxinRange = Config.Bind("Secondary : Neurotoxin", "Range", 45f, "Default is 45");
            newrotoxinRadius = Config.Bind("Secondary : Neurotoxin", "Radius", 7f, "Default is 7");
            newrotoxinProcCoeff = Config.Bind("Secondary : Neurotoxin", "Proc Coefficient", 1f, "Default is 1");

            var acrid = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoBody.prefab").WaitForCompletion();
            var esm = acrid.AddComponent<EntityStateMachine>();
            esm.customName = "Leap";
            esm.initialStateType = new(typeof(EntityStates.Idle));
            esm.mainStateType = new(typeof(EntityStates.Idle));

            var cleap = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoLeap.asset").WaitForCompletion();
            cleap.activationStateMachineName = "Leap";

            var fleap = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoChainableLeap.asset").WaitForCompletion();
            fleap.activationStateMachineName = "Leap";

            var esm2 = acrid.AddComponent<EntityStateMachine>();
            esm2.customName = "Neurotoxin";
            esm2.initialStateType = new(typeof(EntityStates.Idle));
            esm2.mainStateType = new(typeof(EntityStates.Idle));

            var nsm = acrid.GetComponent<NetworkStateMachine>();

            Array.Resize(ref nsm.stateMachines, nsm.stateMachines.Length + 2);
            nsm.stateMachines[nsm.stateMachines.Length - 2] = esm;
            nsm.stateMachines[nsm.stateMachines.Length - 1] = esm2;

            NewrotoxinVFX.Create();
            NewrotoxinSD.Create();
            if (enableNewrotoxin.Value) ReplaceSkill.Create();

            IEnumerable<Type> enumerable = from type in Assembly.GetExecutingAssembly().GetTypes()
                                           where !type.IsAbstract && type.IsSubclassOf(typeof(TweakBase))
                                           select type;

            HACTLogger.LogInfo("==+----------------==TWEAKS==----------------+==");

            foreach (Type type in enumerable)
            {
                TweakBase based = (TweakBase)Activator.CreateInstance(type);
                if (ValidateTweak(based))
                {
                    based.Init();
                }
            }

            IEnumerable<Type> enumerable2 = from type in Assembly.GetExecutingAssembly().GetTypes()
                                            where !type.IsAbstract && type.IsSubclassOf(typeof(MiscBase))
                                            select type;

            HACTLogger.LogInfo("==+----------------==MISC==----------------+==");

            foreach (Type type in enumerable2)
            {
                MiscBase based = (MiscBase)Activator.CreateInstance(type);
                if (ValidateMisc(based))
                {
                    based.Init();
                }
            }
        }

        public bool ValidateTweak(TweakBase tb)
        {
            if (tb.isEnabled)
            {
                bool enabledfr = Config.Bind(tb.Name, "Enable?", true, "Vanilla is false").Value;
                if (enabledfr)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ValidateMisc(MiscBase mb)
        {
            if (mb.isEnabled)
            {
                bool enabledfr = Config.Bind(mb.Name, "Enable?", true, "Vanilla is false").Value;
                if (enabledfr)
                {
                    return true;
                }
            }
            return false;
        }

        private void WITHINDESTRUCTIONMYFUCKINGBELOVED()
        {
        }
    }
}