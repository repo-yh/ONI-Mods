using HarmonyLib;
using KMod;

namespace BuildingMeltGrace
{
    internal class Load : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            Localization.RegisterForTranslation(typeof(Unlock_Cheat.Languages));
            Commons.Translation_Patch.TryLoadTranslations(this, out _);
            harmony.PatchAll();
        }
    }
}
