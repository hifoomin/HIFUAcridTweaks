using BepInEx.Configuration;
using R2API;
using static Mono.Security.X509.X520;
using static System.Collections.Specialized.BitVector32;
using System.Text.RegularExpressions;

namespace HIFUAcridTweaks
{
    public abstract class TweakBase
    {
        public abstract string Name { get; }
        public abstract string SkillToken { get; }
        public abstract string DescText { get; }
        public virtual bool isEnabled { get; } = true;
        public abstract bool DoesNotKillTheMod { get; }

        public T ConfigOption<T>(T value, string name, string description)
        {
            var config = Main.HACTConfig.Bind<T>(Name, name, value, description); // make the config
            ConfigManager.HandleConfig(config, Main.HACTConfig, Name); // config versioning
            return config.Value;
        }

        public abstract void Hooks();

        public string d(float f)
        {
            return (f * 100f).ToString() + "%";
        }

        public virtual void Init()
        {
            Hooks();
            string descriptionToken = "CROCO_" + SkillToken.ToUpper() + "_DESCRIPTION";
            LanguageAPI.Add(descriptionToken, DescText);
            Main.HACTLogger.LogInfo("Added " + Name);
        }
    }
}