using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Unlock_Cheat.MutantPlants
{

    [HarmonyPatch(typeof(FewOptionSideScreen), "IsValidForTarget")]
    internal static class StandaloneConversionListValidationPatch
    {
        private static bool Prefix(GameObject target, ref bool __result)
        {
            PlantMutationOptionsController component = target != null ? target.GetComponent<PlantMutationOptionsController>() : null;
            if (component == null)
            {
                return true;
            }
            __result = component.IsPanelOpen;
            return false;
        }
    }

    //[HarmonyPatch(typeof(ButtonMenuSideScreen), "IsValidForTarget")]
    //internal static class StandaloneButtonMenuValidationPatch
    //{
    //    private static bool Prefix(GameObject target, ref bool __result)
    //    {
    //        if (target == null || target.GetComponent<MutantPlant>() == null)
    //        {
    //            return true;
    //        }
    //        MutantPlant component1 = target.GetComponent<MutantPlant>();

    //        __result = target.GetComponents<ISidescreenButtonControl>().Any((ISidescreenButtonControl control) => control.SidescreenEnabled());
    //        return false;
    //    }
    //}

    //[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
    //public static class DetailsScreenSideScreenDumpPatch
    //{
    //    public static void Postfix(DetailsScreen __instance)
    //    {
    //        List<DetailsScreen.SideScreenRef> sideScreens = typeof(DetailsScreen)
    //            .GetField("sideScreens", BindingFlags.NonPublic | BindingFlags.Instance)
    //            ?.GetValue(__instance) as List<DetailsScreen.SideScreenRef>;

    //        if (sideScreens == null)
    //        {
    //            Debug.Log("[PlantMutationOptions] sideScreens 反射失败");
    //            return;
    //        }

    //        Debug.LogFormat("[PlantMutationOptions] === sideScreens dump，共 {0} 条 ===", sideScreens.Count);
    //        for (int i = 0; i < sideScreens.Count; i++)
    //        {
    //            DetailsScreen.SideScreenRef s = sideScreens[i];
    //            string prefabType = s.screenPrefab != null ? s.screenPrefab.GetType().Name : "<null>";
    //            string mark = s.screenPrefab is FewOptionSideScreen ? "  ← 本mod控制器挂载点" : "";
    //            Debug.LogFormat("[PlantMutationOptions] [{0}] name={1} tab={2} prefab={3}{4}",
    //                i, s.name, s.tab, prefabType, mark);

    //            if (s.screenPrefab is FewOptionSideScreen)
    //            {
    //                foreach (KPrefabID kprefabID in Assets.Prefabs)
    //                {
    //                    if (kprefabID.GetComponent<MutantPlant>() != null)
    //                    {
    //                        bool valid = s.screenPrefab.IsValidForTarget(kprefabID.gameObject);
    //                        Debug.LogFormat("[PlantMutationOptions] IsValidForTarget({0}) = {1}",
    //                            kprefabID.PrefabID().Name, valid);
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}
