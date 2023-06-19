using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUAcridTweaks.Misc
{
    internal class BaseStats : MiscBase
    {
        public override string Name => "Misc :: Base Stats";

        public static float baseDamage;
        public static float baseHealth;

        public override void Init()
        {
            baseDamage = ConfigOption(12f, "Base Damage", "Vanilla is 15");
            baseHealth = ConfigOption(140f, "Base Health", "Vanilla is 160");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
        }

        private void Changes()
        {
            var acrid = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoBody.prefab").WaitForCompletion().GetComponent<CharacterBody>();
            acrid.baseDamage = baseDamage;
            acrid.levelDamage = baseDamage * 0.2f;
            acrid.baseMaxHealth = baseHealth;
            acrid.levelMaxHealth = baseHealth * 0.3f;
        }
    }
}