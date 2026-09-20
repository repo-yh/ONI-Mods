using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace laz_yh.SelectLastCarePackage.Patches
{
    [HarmonyPatch(typeof(Immigration), "RandomCarePackage")]
    public static class ImmigrationRandomCarePackagePatch // 随机补给包 + 官方生成流程后清一次性指定包
    {
        public static CarePackageInfo PendingOverride; // 一次性指定：Replace 用，由 GenerateCharacter Postfix 清除

        public static bool Prefix(Immigration __instance, List<CarePackageInfo> ___carePackages, ref CarePackageInfo __result)
        {
            if (PendingOverride != null)
            {
                __result = PendingOverride; // do-while 每圈都返回指定包，保证 info 最终就是它
                return false;
            }
            var context = SaveGame.Instance.GetComponent<ImmigrantScreenContext>();
            if ((___carePackages is null) || (context.Skip))

            {
                context.Skip = true;
                return true; 
            }
            var lastSelectedCarePackageInfo = context.LastSelectedCarePackageInfo;
            if (lastSelectedCarePackageInfo == null) return true;
            bool Find = ___carePackages.Any(p =>
                (p.requirement == null || p.requirement()) &&
                 p.id == lastSelectedCarePackageInfo.id);
            context.Skip = true;

            if (Find)
            {
                __result = lastSelectedCarePackageInfo;
                return false;
            }
            return true;

        }

        [HarmonyPatch(typeof(CarePackageContainer), "GenerateCharacter")]
        public static void Postfix()
        {
            PendingOverride = null;
        }
    }
}