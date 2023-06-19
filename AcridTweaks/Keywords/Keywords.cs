using HIFUAcridTweaks.Skills;
using R2API;

namespace HIFUAcridTweaks.Keywords
{
    public static class Keywords
    {
        public static void Init()
        {
            LanguageAPI.Add("HAT_POISON", "<style=cKeywordName>Poisonous</style><style=cSub>Deal damage equal to <style=cIsDamage>" + (Poison.percentDamagePerSecond * 100f * Poison.duration) + "%</style> of the enemy's maximum health over " + Poison.duration + "s. <i>Cannot stack.</i></style>");

            LanguageAPI.Add("HAT_BLIGHT", "<style=cKeywordName>Blighted</style><style=cSub>Deal <style=cIsDamage>" + (Blight.damagePerSecond * 100f * Blight.duration) + "%</style> base damage over " + Blight.duration + "s. <i>Can stack.</i></style>");
        }
    }
}