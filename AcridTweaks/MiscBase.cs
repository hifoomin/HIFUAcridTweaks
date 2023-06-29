namespace HIFUAcridTweaks
{
    public abstract class MiscBase
    {
        public abstract string Name { get; }
        public virtual bool isEnabled { get; } = true;
        public abstract bool DoesNotKillTheMod { get; }

        public T ConfigOption<T>(T value, string name, string description)
        {
            var config = Main.HACTConfig.Bind<T>(Name, name, value, description);
            ConfigManager.HandleConfig<T>(config, Main.HACTBackupConfig, name);
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
            Main.HACTLogger.LogInfo("Added " + Name);
        }
    }
}