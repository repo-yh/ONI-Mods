using System.Reflection;
using HarmonyLib;
using KMod;
using laz_yh.Commons;
using System.Collections.Generic;

namespace laz_yh.SelectLastCarePackage
{
    public class ModLoader : UserMod2
    {
        internal static bool Patches2 = true;
        static Dictionary<string, string> translations;
        public override void OnLoad(Harmony harmony)
        {

            ManualPatch.ManualPatch_init(harmony, this.assembly.GetTypes());
            ManualPatch.ManualPatch_NS("laz_yh.SelectLastCarePackage.Patches");
#if DEBUG
            ModUtil.RegisterForTranslation(typeof(Languages));
#else
            Localization.RegisterForTranslation(typeof(Languages));
#endif
            global::Commons.Translation_Patch.TryLoadTranslations(this, out translations);

        }

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
        {
            if ( !ModUtils.HasModbydlc(mods, new List<string>() {"2856555858"}))
            {
                ManualPatch.ManualPatch_NS("laz_yh.SelectLastCarePackage.Patches2");
                Debug.Log("[最后的补给包-Fix] 刷新按钮启用");
            }

            else
            {
                Patches2 = false;
                Debug.LogFormat("[最后的补给包-Fix] 刷新按钮按钮被mod_workshop_id: {0} 启用", "2856555858");
            }
            foreach (MethodBase method in harmony.GetPatchedMethods())
            {
                Debug.LogFormat("[最后的补给包-Fix] 修补了：{0}.{1}", method.DeclaringType.FullName, method.Name);
            }
        }
    }
}