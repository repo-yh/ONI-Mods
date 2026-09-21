using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace DlcUnlockPatcher
{
    // Token: 0x02000004 RID: 4
    public static class Entry
	{
        // Token: 0x06000003 RID: 3 RVA: 0x00002068 File Offset: 0x00000268

        public static void Run()
		{
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(OnAssemblyResolve); ;

                Patches.MatchId = Environment.GetEnvironmentVariable("YH_PATCH_MATCH_ID");
                if (string.IsNullOrEmpty(Patches.MatchId))
                {
                    Patches.MatchId = "COSMETIC1_ID";
                }
                Log($"entry, match_id={Patches.MatchId}");

                var harmony = new Harmony("yh.dlcunlock");
                harmony.PatchAll(typeof(Patches).Assembly);
                Log("patched");
            }
            catch (Exception ex)
            {
                Log($"FATAL {ex}");
            }
        }

		// Token: 0x06000004 RID: 4 RVA: 0x0000212C File Offset: 0x0000032C
		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			string environmentVariable = Environment.GetEnvironmentVariable("YH_PATCH_DIR");
			if (string.IsNullOrEmpty(environmentVariable))
			{
				return null;
			}
			string name = new AssemblyName(args.Name).Name;
			string text = Path.Combine(environmentVariable, name + ".dll");
			if (!File.Exists(text))
			{
				return null;
			}
			Entry.Log("resolve " + args.Name + " -> " + text);
			return Assembly.LoadFrom(text);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000219C File Offset: 0x0000039C
		private static void Log(string msg)
		{
			try
			{
				File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yhpatcher.log"), System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + msg + Environment.NewLine);
			}
			catch
			{
			}
		}


	}
}
