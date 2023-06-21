using RoR2;
using RoR2.Skills;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUAcridTweaks.Skills
{
    internal class PassiveController : MonoBehaviour
    {
        public string currentPassive;
        public static SkillFamily passiveFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Croco/CrocoBodyPassiveFamily.asset").WaitForCompletion();

        public void Start()
        {
            GenericSkill passive = (from x in gameObject.GetComponents<GenericSkill>()
                                    where x.skillFamily == passiveFamily
                                    select x).First();

            currentPassive = passive.skillNameToken;
        }
    }
}