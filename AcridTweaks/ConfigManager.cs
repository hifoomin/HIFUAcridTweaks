﻿using BepInEx.Configuration;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HIFUAcridTweaks
{
    public class ConfigManager
    {
        internal static bool ConfigChanged = false;
        internal static bool VersionChanged = false;
        public static MethodInfo method;

        public static void HandleConfigAttributes(Type type, string name, string description, string section, object defaultValue, ConfigFile config)
        {
            TypeInfo info = type.GetTypeInfo();

            foreach (FieldInfo field in info.GetFields())
            {
                if (!field.IsStatic) continue;

                Type t = field.FieldType;

                Debug.LogErrorFormat("data: {0}, {1}, {2}, {3}, {4}, {5}", type, name, description, section, defaultValue, config);
                Main.HACTLogger.LogError("field is " + field.Name + " type: " + t);

                method = typeof(ConfigFile).GetMethods().Where(x => x.Name == nameof(ConfigFile.Bind)).First();

                Debug.LogErrorFormat("methodinfo: {0}, {1}", method, method.Name);

                method = method.MakeGenericMethod(t);

                Debug.LogErrorFormat("generic method: {0}, {1}", method, method.Name);

                ConfigEntryBase val = (ConfigEntryBase)method.Invoke(config, new object[] { new ConfigDefinition(section, name), defaultValue, new ConfigDescription(description) });
                ConfigEntryBase backupVal = (ConfigEntryBase)method.Invoke(Main.HACTBackupConfig, new object[] { new ConfigDefinition(Regex.Replace(config.ConfigFilePath, "\\W", "") + " : " + section, name), val.DefaultValue, new ConfigDescription(description) });
                // problems here -- [Error  : Unity Log] TargetException: Non-static method requires a target.
                Debug.LogErrorFormat("val and backupVal: {0} {1}", val, backupVal);
                if (val != null && backupVal != null)
                {
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