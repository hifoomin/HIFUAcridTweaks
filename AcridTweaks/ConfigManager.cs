using BepInEx.Configuration;
using IL.RoR2.Achievements.Merc;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

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

            Main.HACTLogger.LogError(method.Invoke(config, new object[] { new ConfigDefinition(entry.Definition.Section, name), entry.DefaultValue, new ConfigDescription(entry.Description.Description) }));

            // method.Invoke(config, new object[] { new ConfigDefinition(entry.Definition.Section, name), entry.DefaultValue, new ConfigDescription(entry.Description.Description) });

            // ConfigEntryBase backupVal = (ConfigEntryBase)method.Invoke(config, new object[] { new ConfigDefinition(entry.Definition.Section, name), entry.DefaultValue, new ConfigDescription(entry.Description.Description) });
            /* calls ConfigFile.Bind regardless of type. see new ConfigEntryBase(); */

            // entry.BoxedValue: current value
            // entry.DefaultValue: default of version 1.0.1
            // backupVal.BoxedValue: default of version 1.0.0
            // backupVal.DefaultValue: default of version 1.0.1 (takes entry.DefaultValue)
            /*
            if (!ConfigEqual(backupVal.DefaultValue, backupVal.BoxedValue)) // see if defaults have changed
            {
                if (VersionChanged) // sanity check
                {
                    entry.BoxedValue = entry.DefaultValue; // Override Current Value to Default of Version 1.0.1
                    backupVal.BoxedValue = backupVal.DefaultValue; // Update Old Default to New Default
                }
            }
            */
        }

        /*
        public static void HandleConfig(Type type, string name, string description, string section, object defaultValue, ConfigFile config)
        {
            TypeInfo info = type.GetTypeInfo();

            foreach (FieldInfo field in info.GetFields())
            {
                if (!field.IsStatic) continue;

                if (field.IsLiteral) continue; //  [Error  : Unity Log] FieldAccessException: Cannot set a constant field 5uhreyFGXVCJ*E$JRDS*OICVJ

                Type t = field.FieldType;

                // if (!Convert.ToBoolean(t)) continue; // [Error  : Unity Log] FormatException: String was not recognized as a valid Boolean. FUCK FKFUCKF U FIF JDGUF JFEDGUJOTTYJREJTGYERYTG
                // if (!bool.Parse(t.ToString())) continue; // [Error  : Unity Log] InvalidCastException: Specified cast is not valid. AERILFJIFODSJIG

                Debug.LogErrorFormat("data: {0}, {1}, {2}, {3}, {4}, {5}", type, name, description, section, defaultValue, config);
                Main.HACTLogger.LogError("field is " + field.Name + " type: " + t);

                method = typeof(ConfigFile).GetMethods().Where(x => x.Name == nameof(ConfigFile.Bind)).First();

                Debug.LogErrorFormat("methodinfo: {0}, {1}", method, method.Name);

                method = method.MakeGenericMethod(t);

                Debug.LogErrorFormat("generic method: {0}, {1}", method, method.Name);

                ConfigEntryBase val = (ConfigEntryBase)method.Invoke(config, new object[] { new ConfigDefinition(section, name), defaultValue, new ConfigDescription(description) });
                ConfigEntryBase backupVal = (ConfigEntryBase)method.Invoke(Main.HACTBackupConfig, new object[] { new ConfigDefinition(Regex.Replace(config.ConfigFilePath, "\\W", "") + " : " + section, name), val.DefaultValue, new ConfigDescription(description) });

                Debug.LogErrorFormat("val and backupVal: {0} {1}", val, backupVal);
                Main.HACTLogger.LogError(section + " : " + name + " " + val.DefaultValue + " / " + val.BoxedValue + " ... " + backupVal.DefaultValue + " / " + backupVal.BoxedValue + " >> " + VersionChanged);

                if (!ConfigEqual(backupVal.DefaultValue, backupVal.BoxedValue))
                {
                    Main.HACTLogger.LogError("Config Updated: " + section + " : " + name + " from " + val.BoxedValue + " to " + val.DefaultValue);
                    if (VersionChanged)
                    {
                        Main.HACTLogger.LogError("Autosyncing...");
                        val.BoxedValue = val.DefaultValue;
                        backupVal.BoxedValue = backupVal.DefaultValue;
                    }
                }
                if (!ConfigEqual(val.DefaultValue, val.BoxedValue)) ConfigChanged = true;
                field.SetValue(null, val.BoxedValue);
            }
        }
        */

        private static bool ConfigEqual(object a, object b)
        {
            if (a.Equals(b)) return true;
            float fa, fb;
            if (float.TryParse(a.ToString(), out fa) && float.TryParse(b.ToString(), out fb) && Mathf.Abs(fa - fb) < 0.0001) return true;
            return false;
        }
    }
}