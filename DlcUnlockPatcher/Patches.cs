using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DlcUnlockPatcher
{
	// Token: 0x02000005 RID: 5
	[HarmonyPatch]
	public static class Patches
	{
		// Token: 0x06000006 RID: 6 RVA: 0x000021F4 File Offset: 0x000003F4
		[HarmonyPrefix]
		[HarmonyPatch(typeof(DlcManager), "IsContentSubscribed")]
		public static bool PreIsContentSubscribed(string dlcId, ref bool __result,ref  Dictionary<string, bool>  ___dlcSubscribedCache)
		{
			if (dlcId != Patches.MatchId)
			{
				return true;
			}
            ___dlcSubscribedCache[dlcId] = true;
			__result = (CheckForDLCFileInstallation.GetValue<bool>(new object[] { dlcId }) && IsContentSettingEnabled.GetValue<bool>(new object[] { dlcId })  );
			return false;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002225 File Offset: 0x00000425
		[HarmonyPrefix]
		[HarmonyPatch(typeof(DlcManager), "IsContentOwned")]
		public static bool PreIsContentOwned(string dlcId, ref bool __result, ref Dictionary<string, bool> ___dlcPurchasedCache)
		{
			if (dlcId != Patches.MatchId)
			{
				return true;
			}
            ___dlcPurchasedCache[dlcId] = true;
			__result = true;
			return false;
		}

		// Token: 0x04000002 RID: 2
		public static string MatchId = "COSMETIC1_ID";

		static Traverse CheckForDLCFileInstallation = Traverse.Create(typeof(DlcManager)).Method("CheckForDLCFileInstallation", new Type[] { typeof(string) });

        static Traverse IsContentSettingEnabled = Traverse.Create(typeof(DlcManager)).Method("IsContentSettingEnabled", new Type[] { typeof(string) });

    }
}
