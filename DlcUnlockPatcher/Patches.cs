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

	// 不带 [HarmonyPatch] 类注解：PatchAll 在 firstpass 加载时机执行，此时 Assembly-CSharp 尚未加载，
	// 注解里的 typeof(KMod.Manager) 解析失败会炸掉整个 PatchAll（连坐 DLC patch）。
	// 本类由 Assembly-CSharp 加载事件后手动注册（Entry.RunLocalizationPatch）。
	public static class EarlyLocalizationPatches
	{
		private static bool _patched;
		private static bool _ran;

		public static void Patch(Harmony harmony)
		{
			if (_patched)
			{
				return;
			}
			_patched = true;
			try
			{
				var method = typeof(KMod.Manager).GetMethod("Load", new Type[] { typeof(KMod.Content) });
				var prefix = typeof(EarlyLocalizationPatches).GetMethod(nameof(ManagerLoadPrefix));
				harmony.Patch(method, new HarmonyMethod(prefix), null, null, null);
				Entry.Log("early localization patch applied");
			}
			catch (Exception ex)
			{
				Entry.Log("early localization patch FAILED " + ex);
			}
		}

		// 难度面板文本是 CustomGameSettingConfigs 静态构造瞬间的 STRINGS 快照，而 mod dll 的
		// OnLoad 全部晚于官方 Localization.Initialize()；第三方 mod 先碰这些类型会把英文快照定格。
		// 在 KMod.Manager.Load(Content.DLL)（mod 加载开始）前补一次 Initialize。
		public static void ManagerLoadPrefix(KMod.Content content)
		{
			if (_ran)
			{
				return;
			}
			if ((content & KMod.Content.DLL) == 0)
			{
				return;
			}
			// 仅官方内置语言需要提前初始化；UGC 语言用户走官方原时序，避免 ClearLanguage 清掉语言设置
			if (Localization.GetSelectedLanguageType() != Localization.SelectedLanguageType.Preinstalled)
			{
				return;
			}
			if (Localization.GetLocale() != null)
			{
				return;
			}
			_ran = true;
			Localization.Initialize();
			Entry.Log("early localization initialized (preinstalled)");
		}
	}
}
