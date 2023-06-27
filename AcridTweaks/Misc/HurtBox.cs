using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUAcridTweaks.Misc
{
    internal class HurtBox : MiscBase
    {
        public override string Name => "Misc ::: Hurt Box";
        public override bool DoesNotKillTheMod => true;
        public static float sizeMultiplier;

        public override void Init()
        {
            sizeMultiplier = ConfigOption(0.75f, "Size Multiplier", "Vanilla is 1");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
        }

        private void Changes()
        {
            var acrid = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoBody.prefab").WaitForCompletion();
            var mainHurtBox = acrid.transform.GetChild(0).GetChild(2).Find("TempHurtbox").GetComponent<SphereCollider>();
            mainHurtBox.transform.localPosition = new Vector3(0f, 7f, 2f);
            mainHurtBox.radius = 5.26f * sizeMultiplier;
        }
    }
}