using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace DlcUnlockPatcher
{
    public static class Entry
    {
        private static bool _ran;
        private static bool _resolveHooked;
        private static Dictionary<string, string> _proxyIni;

        public static bool LogEnabled = true;

        // Doorstop 官方入口：加载后由其委托到 Doorstop.Entrypoint.Start()
        public static void Start()
        {
            EnsureResolveHook();
            // DlcManager 在 Assembly-CSharp-firstpass，必须等它加载完才能 PatchAll
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            if (IsFirstpassLoaded())
            {
                Run();
            }
        }

        // native bootstrap 模式入口（firstpass 已由 native 层等待）
        public static void Run()
        {
            if (_ran)
            {
                return;
            }
            _ran = true;
            try
            {
                EnsureResolveHook();
                Patches.MatchId = GetMatchId();
                var logEnv = Environment.GetEnvironmentVariable("DLC_PATCHER_LOG");
                if (logEnv == "0")
                {
                    LogEnabled = false;
                }
                LogDoorstopEnv();
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

        private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.GetName().Name == "Assembly-CSharp-firstpass")
            {
                Run();
            }
        }

        private static bool IsFirstpassLoaded()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                if (assemblies[i].GetName().Name == "Assembly-CSharp-firstpass")
                {
                    return true;
                }
            }
            return false;
        }

        private static void EnsureResolveHook()
        {
            if (_resolveHooked)
            {
                return;
            }
            _resolveHooked = true;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private static void LogDoorstopEnv()
        {
            var keys = new[]
            {
                "DOORSTOP_INITIALIZED",
                "DOORSTOP_INVOKE_DLL_PATH",
                "DOORSTOP_PROCESS_PATH",
                "DOORSTOP_MANAGED_FOLDER_DIR",
                "DOORSTOP_DLL_SEARCH_DIRS",
                "DOORSTOP_MONO_LIB_PATH",
            };
            foreach (var key in keys)
            {
                Log(key + "=" + Environment.GetEnvironmentVariable(key));
            }
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            foreach (var dir in GetSearchDirs())
            {
                var path = Path.Combine(dir, name + ".dll");
                if (!File.Exists(path))
                {
                    continue;
                }
                try
                {
                    var assembly = Assembly.LoadFrom(path);
                    Log("resolve " + args.Name + " -> " + path);
                    return assembly;
                }
                catch (Exception ex)
                {
                    // 解析回调里抛异常会传播给游戏调用方，吞掉并继续失败流程
                    Log("resolve failed " + args.Name + ": " + ex.Message);
                }
            }
            return null;
        }

        private static IEnumerable<string> GetSearchDirs()
        {
            var env = Environment.GetEnvironmentVariable("DLC_PATCHER_DIR");
            if (!string.IsNullOrEmpty(env))
            {
                yield return env;
            }
            var dllDir = Path.GetDirectoryName(typeof(Entry).Assembly.Location);
            if (!string.IsNullOrEmpty(dllDir))
            {
                yield return dllDir;
            }
            // Doorstop 自动导出的游戏 Managed 目录（native bootstrap 模式下无此变量）
            var doorstopManaged = Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR");
            if (!string.IsNullOrEmpty(doorstopManaged))
            {
                yield return doorstopManaged;
            }
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            yield return Path.Combine(baseDir, "OxygenNotIncluded_Data", "Managed");
            yield return baseDir;
        }

        private static string GetMatchId()
        {
            var env = Environment.GetEnvironmentVariable("DLC_PATCHER_MATCH_ID");
            if (!string.IsNullOrEmpty(env))
            {
                return env;
            }
            var ini = LoadProxyIni();
            return ini.TryGetValue("match_id", out var value) && !string.IsNullOrEmpty(value)
                ? value
                : "COSMETIC1_ID";
        }

        private static Dictionary<string, string> LoadProxyIni()
        {
            if (_proxyIni != null)
            {
                return _proxyIni;
            }
            _proxyIni = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var dllDir = Path.GetDirectoryName(typeof(Entry).Assembly.Location);
                var candidates = new[]
                {
                    Path.Combine(dllDir ?? "", "..", "proxy.ini"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "proxy.ini"),
                };
                foreach (var candidate in candidates)
                {
                    var fullPath = Path.GetFullPath(candidate);
                    if (!File.Exists(fullPath))
                    {
                        continue;
                    }
                    foreach (var raw in File.ReadAllLines(fullPath))
                    {
                        var line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith("[") || line.StartsWith("#") || line.StartsWith(";"))
                        {
                            continue;
                        }
                        var eq = line.IndexOf('=');
                        if (eq <= 0)
                        {
                            continue;
                        }
                        _proxyIni[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
                    }
                    Log("proxy.ini loaded from " + fullPath);
                    break;
                }
            }
            catch (Exception ex)
            {
                Log("proxy.ini load failed: " + ex.Message);
            }
            return _proxyIni;
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

// Doorstop 官方约定入口：static void Doorstop.Entrypoint.Start()
namespace Doorstop
{
    public static class Entrypoint
    {
        public static void Start()
        {
            DlcUnlockPatcher.Entry.Start();
        }
    }
}
