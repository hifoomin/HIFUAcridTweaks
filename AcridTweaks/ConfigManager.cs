using BepInEx.Configuration;
using System.Linq;
using UnityEngine;

namespace HIFUAcridTweaks
{
    public class ConfigManager
    {
        internal static bool ConfigChanged = false;
        internal static bool VersionChanged = false;

        public static void HandleConfig(ConfigEntryBase entry, ConfigFile config, string name)
        {
            Main.HACTLogger.LogError("config description string: " + entry.Description.Description);

            var method = typeof(ConfigFile).GetMethods().Where(x => x.Name == nameof(ConfigFile.Bind)).First();
            method = method.MakeGenericMethod(entry.SettingType);

            var newConfigEntry = new object[] { new ConfigDefinition(entry.Definition.Section, name), entry.DefaultValue, new ConfigDescription(entry.Description.Description) };

            // var backupVal = Main.HACTBackupConfig.Bind(new ConfigDefinition(entry.Definition.Section, name), entry.SettingType, new ConfigDescription(entry.Description.Description));
            // System.Type is not supported by bepinex configs, also tried but entry.DefaultValue it's an object, also tried get type of DefaultValue and same System.Type error again guh

            // converts
            // BepInEx.Configuration.ConfigEntry`1[T] Bind[T](BepInEx.Configuration.ConfigDefinition, T, BepInEx.Configuration.ConfigDescription)
            // =>
            // BepInEx.Configuration.ConfigEntry`1[System.Int32] Bind[Int32](BepInEx.Configuration.ConfigDefinition, Int32, BepInEx.Configuration.ConfigDescription)

            var backupVal = (ConfigEntryBase)method.Invoke(config, newConfigEntry);
            // BepInEx tries to convert our converted Int32 or Boolean into T and somehow fails (BepInEx.Configuration.ConfigFile.Bind(ConfigDefinition, T, ConfigDescription), @ IL_0080)
            // removing MakeGenericMethod makes it throw even at converting System.Single to T

            if (!ConfigEqual(backupVal.DefaultValue, backupVal.BoxedValue))
            {
                if (VersionChanged)
                {
                    entry.BoxedValue = entry.DefaultValue;
                    backupVal.BoxedValue = backupVal.DefaultValue;
                }
            }
        }

        private static bool ConfigEqual(object a, object b)
        {
            if (a.Equals(b)) return true;
            float fa, fb;
            if (float.TryParse(a.ToString(), out fa) && float.TryParse(b.ToString(), out fb) && Mathf.Abs(fa - fb) < 0.0001) return true;
            return false;
        }
    }
}