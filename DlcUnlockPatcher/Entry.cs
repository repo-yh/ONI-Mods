using System;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace DlcUnlockPatcher
{
    public static class Entry
    {
        public static bool LogEnabled = true;

        public static void Run()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                Patches.MatchId = Environment.GetEnvironmentVariable("DLC_PATCHER_MATCH_ID");
                if (string.IsNullOrEmpty(Patches.MatchId))
                {
                    Patches.MatchId = "COSMETIC1_ID";
                }
                var logEnv = Environment.GetEnvironmentVariable("DLC_PATCHER_LOG");
                if (logEnv == "0")
                {
                    LogEnabled = false;
                }
                Log("entry, match_id=" + Patches.MatchId);
                var harmony = new Harmony("dlc.unlock.patcher");
                harmony.PatchAll(typeof(Patches).Assembly);
                Log("patched");
            }
            catch (Exception ex)
            {
                Log("FATAL " + ex);
            }
        }

        private static Assembly? OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var dir = Environment.GetEnvironmentVariable("DLC_PATCHER_DIR");
            if (string.IsNullOrEmpty(dir))
            {
                return null;
            }
            var name = new AssemblyName(args.Name).Name;
            var path = Path.Combine(dir, name + ".dll");
            if (!File.Exists(path))
            {
                return null;
            }
            Log("resolve " + args.Name + " -> " + path);
            return Assembly.LoadFrom(path);
        }

        private static void Log(string msg)
        {
            if (!LogEnabled)
            {
                return;
            }
            try
            {
                // 游戏程序集（Assembly-CSharp）里有全局 DateTime 类型会遮蔽 System.DateTime，必须全限定
                File.AppendAllText(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dlcpatcher.log"),
                    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + msg + Environment.NewLine);
            }
            catch
            {
                // 日志失败不影响主流程
            }
        }
    }
}
