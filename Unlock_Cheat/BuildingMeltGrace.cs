using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Unlock_Cheat.BuildingMeltGrace
{
    // sim 判定融化后不立即执行：首条 melted 消息入宽限名单并预警，
    // 期满且仍超温才由扫描器放行执行真融化（官方 DoMelt 自带红色通知）
    [HarmonyPatch(typeof(StructureTemperatureComponents), nameof(StructureTemperatureComponents.DoMelt))]
    public static class StructureTemperatureComponents_DoMelt_Patch
    {
        // 精确放行：仅扫描器点名的建筑在 DoMelt 调用期间放行一次，防止重入误放其他建筑
        public static readonly HashSet<GameObject> bypassList = new HashSet<GameObject>();

        public static bool Prefix(PrimaryElement primary_element)
        {
            if (bypassList.Remove(primary_element.gameObject))
            {
                return true;
            }
            GameObject go = primary_element.gameObject;
            if (!BuildingMeltGraceMonitor.graceList.ContainsKey(go))
            {
                BuildingMeltGraceMonitor.graceList[go] = new BuildingMeltGraceMonitor.GraceEntry
                {
                    FirstSeen = GameClock.Instance.GetTime()
                };
                BuildingMeltGraceMonitor.NotifyMeltWarning(go);
            }
            return false;
        }
    }

    public class BuildingMeltGraceMonitor : KMonoBehaviour, ISim1000ms
    {
        public class GraceEntry
        {
            public float FirstSeen;
            public Notification Notice;
        }

        public static readonly Dictionary<GameObject, GraceEntry> graceList =
            new Dictionary<GameObject, GraceEntry>();

        private const float GRACE_SECONDS = 30f;

        private List<GameObject> finished = new List<GameObject>();

        public void Sim1000ms(float dt)
        {
            foreach (KeyValuePair<GameObject, GraceEntry> kv in graceList)
            {
                GameObject go = kv.Key;
                GraceEntry entry = kv.Value;
                if (go == null)
                {
                    entry.Notice?.Clear();
                    finished.Add(go);
                    continue;
                }
                PrimaryElement pe = go.GetComponent<PrimaryElement>();
                if (pe == null || pe.Temperature < pe.Element.highTemp)
                {
                    entry.Notice?.Clear();
                    finished.Add(go);
                }
                else if (GameClock.Instance.GetTime() - entry.FirstSeen >= GRACE_SECONDS)
                {
                    finished.Add(go);
                    entry.Notice?.Clear();
                    StructureTemperatureComponents_DoMelt_Patch.bypassList.Add(go);
                    try
                    {
                        StructureTemperatureComponents.DoMelt(pe);
                    }
                    finally
                    {
                        StructureTemperatureComponents_DoMelt_Patch.bypassList.Remove(go);
                    }
                }
            }
            for (int i = 0; i < finished.Count; i++)
            {
                graceList.Remove(finished[i]);
            }
            finished.Clear();
        }

        public static void NotifyMeltWarning(GameObject go)
        {
            Vector3 pos = go.transform.GetPosition();
            Notifier notifier = go.AddOrGet<Notifier>();
            Notification notification = new Notification(
                Languages.UI.NOTIFICATIONS.BUILDING_MELT_WARNING.NAME,
                NotificationType.Bad,
                (List<Notification> notificationList, object data) => string.Concat(
                    Languages.UI.NOTIFICATIONS.BUILDING_MELT_WARNING.TOOLTIP,
                    notificationList.ReduceMessages(countNames: false)),
                "/t• " + notifier.GetProperName(),
                expires: true, 0f,
                delegate
                {
                    GameUtil.FocusCamera(pos);
                }, null, null, volume_attenuation: true, clear_on_click: true);
            notifier.Add(notification);
            if (graceList.TryGetValue(go, out GraceEntry entry))
            {
                entry.Notice = notification;
            }
        }
    }

    [HarmonyPatch(typeof(Game), "OnPrefabInit")]
    public static class Game_OnPrefabInit_Patch
    {
        public static void Postfix(Game __instance)
        {
            __instance.gameObject.AddOrGet<BuildingMeltGraceMonitor>();
        }
    }
}
