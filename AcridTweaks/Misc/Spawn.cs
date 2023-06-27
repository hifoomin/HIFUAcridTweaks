namespace HIFUAcridTweaks.Misc
{
    internal class Spawn : MiscBase
    {
        public override string Name => "Misc : Spawn Animation";
        public override bool DoesNotKillTheMod => true;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            On.EntityStates.Croco.Spawn.OnEnter += Spawn_OnEnter;
            On.EntityStates.Croco.WakeUp.OnEnter += WakeUp_OnEnter;
        }

        private void WakeUp_OnEnter(On.EntityStates.Croco.WakeUp.orig_OnEnter orig, EntityStates.Croco.WakeUp self)
        {
            EntityStates.Croco.WakeUp.duration = 1.2f;
            orig(self);
        }

        private void Spawn_OnEnter(On.EntityStates.Croco.Spawn.orig_OnEnter orig, EntityStates.Croco.Spawn self)
        {
            EntityStates.Croco.Spawn.minimumSleepDuration = 0.7f;
            orig(self);
        }
    }
}