using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using R2API;
using R2API.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using HarmonyLib;

namespace HIFUAcridTweaks
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(R2APIContentManager.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(DamageAPI.PluginGUID)]
    [BepInDependency(DotAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "HIFU";
        public const string PluginName = "HIFUAcridTweaks";
        public const string PluginVersion = "1.2.0";

        public static ConfigFile HACTConfig;
        public static ConfigFile HACTBackupConfig;
        public static ConfigEntry<bool> enableAutoConfig { get; set; }
        public static ConfigEntry<string> latestVersion { get; set; }
        public static ManualLogSource HACTLogger;

        public static DamageAPI.ModdedDamageType poison = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType blight = DamageAPI.ReserveDamageType();

        public static bool _preVersioning = false;

        public static AssetBundle iHateThis;

        public void Awake()
        {
            HACTLogger = Logger;
            HACTConfig = Config;

            iHateThis = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("HIFUAcridTweaks.dll", "hifuacridtweaks"));

            HACTBackupConfig = new(Paths.ConfigPath + "\\" + PluginAuthor + "." + PluginName + ".Backup.cfg", true);
            HACTBackupConfig.Bind(": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :");

            enableAutoConfig = HACTConfig.Bind("Config", "Enable Auto Config Sync", true, "Disabling this would stop HIFUAcridTweaks from syncing config whenever a new version is found.");
            _preVersioning = !((Dictionary<ConfigDefinition, string>)AccessTools.DeclaredPropertyGetter(typeof(ConfigFile), "OrphanedEntries").Invoke(HACTConfig, null)).Keys.Any(x => x.Key == "Latest Version");
            latestVersion = HACTConfig.Bind("Config", "Latest Version", PluginVersion, "DO NOT CHANGE THIS");
            if (enableAutoConfig.Value && (_preVersioning || (latestVersion.Value != PluginVersion)))
            {
                latestVersion.Value = PluginVersion;
                ConfigManager.VersionChanged = true;
                HACTLogger.LogInfo("Config Autosync Enabled.");
            }

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

            Keywords.Keywords.Init();
        }

        public bool ValidateTweak(TweakBase tb)
        {
            if (!tb.DoesNotKillTheMod)
            {
                return true;
            }
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
            if (!mb.DoesNotKillTheMod)
            {
                return true;
            }
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