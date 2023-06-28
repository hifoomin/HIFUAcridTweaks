using BepInEx.Configuration;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HIFUAcridTweaks
{
    public class ConfigManager
    {
        internal static bool ConfigChanged = false;
        internal static bool VersionChanged = false;
        public static MethodInfo method;

        public static void HandleConfig(ConfigEntryBase entry, ConfigFile config, string name)
        {
            method = typeof(ConfigFile).GetMethods().Where(x => x.Name == nameof(ConfigFile.Bind)).First(); // method = "ConfigFile.Bind<?>()"
            // Debug.LogError("method pre: " + method); // in int32 -- [Error  : Unity Log] method pre: BepInEx.Configuration.ConfigEntry`1[T] Bind[T](BepInEx.Configuration.ConfigDefinition, T, BepInEx.Configuration.ConfigDescription)
            method = method.MakeGenericMethod(entry.SettingType); // method = "ConfigFile.Bind()"
            // Debug.LogError("method post: " + method); // in int32 -- [Error  : Unity Log] method post: BepInEx.Configuration.ConfigEntry`1[System.Int32] Bind[Int32](BepInEx.Configuration.ConfigDefinition, Int32, BepInEx.Configuration.ConfigDescription)
            // Debug.LogError("entry setting type: " + entry.SettingType); // first System.Int32 gives specified cast is not valid
            // Debug.LogErrorFormat("entry definition section {0} | name overload {1} | entry default value {2} | config description {3}", entry.Definition.Section, name, entry.DefaultValue, entry.Description.Description);

            Debug.LogErrorFormat("entry SettingType: {0} | entry DefaultValue: {1}", entry.SettingType, entry.DefaultValue);

            ConfigEntryBase backupVal = (ConfigEntryBase)method.Invoke(config, new object[] { new ConfigDefinition(entry.Definition.Section, name), entry.DefaultValue, new ConfigDescription(entry.Description.Description) });
            /* calls ConfigFile.Bind regardless of type. see new ConfigEntryBase(); */

            // entry.BoxedValue: current value
            // entry.DefaultValue: default of version 1.0.1
            // backupVal.BoxedValue: default of version 1.0.0
            // backupVal.DefaultValue: default of version 1.0.1 (takes entry.DefaultValue)

            if (!ConfigEqual(backupVal.DefaultValue, backupVal.BoxedValue)) // see if defaults have changed
            {
                if (VersionChanged)
                {
                    entry.BoxedValue = entry.DefaultValue; // override current value to default of version 1.0.1
                    backupVal.BoxedValue = backupVal.DefaultValue; // update old default to new default
                }
            }
            if (!ConfigEqual(entry.DefaultValue, entry.BoxedValue)) ConfigChanged = true;
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