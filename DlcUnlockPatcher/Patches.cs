using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DlcUnlockPatcher
{
	[HarmonyPatch]
	public static class Patches
	{
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

		public static string MatchId = "COSMETIC1_ID";

		static Traverse CheckForDLCFileInstallation = Traverse.Create(typeof(DlcManager)).Method("CheckForDLCFileInstallation", new Type[] { typeof(string) });

        static Traverse IsContentSettingEnabled = Traverse.Create(typeof(DlcManager)).Method("IsContentSettingEnabled", new Type[] { typeof(string) });

    }
}
