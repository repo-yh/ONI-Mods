using HarmonyLib;
using KMod;

namespace NuclearResearch
{
    internal class Load : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            harmony.PatchAll();

        }
    }
}
