using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Unlock_Cheat;

namespace BuildingMeltGrace
{
    public static class ModTags
    {
        // 豁免标记:serialize: true 随建筑存档持久化,读档时 InitializeTags 合并回运行时 tags
        public static readonly Tag MeltExempt = TagManager.Create("BuildingMeltGrace_Exempt");
    }

    // sim 判定融化后不立即执行:首条 melted 消息入宽限名单并预警,
    // 期满且仍超温才由扫描器放行执行真融化(官方 DoMelt 自带红色通知)
    [HarmonyPatch(typeof(StructureTemperatureComponents), nameof(StructureTemperatureComponents.DoMelt))]
    public static class StructureTemperatureComponents_DoMelt_Patch
    {
        // 精确放行:仅扫描器点名的建筑在 DoMelt 调用期间放行一次,防止重入误放其他建筑
        public static readonly HashSet<GameObject> bypassList = new HashSet<GameObject>();

        public static bool Prefix(PrimaryElement primary_element)
        {
            if (bypassList.Remove(primary_element.gameObject))
            {
                return true;
            }
            GameObject go = primary_element.gameObject;
            // 玩家豁免:tag 随存档持久化
            if (go.HasTag(ModTags.MeltExempt))
            {
                return false;
            }
            // 玩家禁用建筑 = 临时豁免(仅限带禁用按钮的建筑,无组件按未禁用处理)
            BuildingEnabledButton enabledButton = go.GetComponent<BuildingEnabledButton>();
            if (enabledButton != null && !enabledButton.IsEnabled)
            {
                return false;
            }
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
                BuildingEnabledButton enabledButton = go.GetComponent<BuildingEnabledButton>();
                // 取消融化:冷却回熔点以下 / 玩家豁免 / 玩家禁用——统一撤通知并移出名单;
                // 重新启用、取消豁免或温度回升后由 sim 重发 melted 重新入名单,重新计 30 秒
                if (pe == null || pe.Temperature < pe.Element.highTemp
                    || go.HasTag(ModTags.MeltExempt)
                    || (enabledButton != null && !enabledButton.IsEnabled))
                {
                    entry.Notice?.Clear();
                    finished.Add(go);
                    continue;
                }
                if (GameClock.Instance.GetTime() - entry.FirstSeen >= GRACE_SECONDS)
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

    // 悬浮卡片"豁免融化"按钮组件(官方 BuildingEnabledButton 同款模式:
    // 订阅 RefreshUserMenu,菜单构建时按需注册按钮)
    [SkipSaveFileSerialization]
    public class BuildingMeltExemptButton : KMonoBehaviour
    {
        private static readonly EventSystem.IntraObjectHandler<BuildingMeltExemptButton> OnRefreshUserMenuDelegate =
            new EventSystem.IntraObjectHandler<BuildingMeltExemptButton>(delegate(BuildingMeltExemptButton component, object data)
            {
                component.OnRefreshUserMenu(data);
            });

        protected override void OnPrefabInit()
        {
            Subscribe(493375141, OnRefreshUserMenuDelegate);
        }

        private void OnRefreshUserMenu(object data)
        {
            GameObject go = base.gameObject;
            bool exempt = go.HasTag(ModTags.MeltExempt);
            // 已豁免:按钮始终存在;未豁免:仅对进入融化宽限期的建筑显示
            if (!exempt && !BuildingMeltGraceMonitor.graceList.ContainsKey(go))
            {
                return;
            }
            Game.Instance.userMenu.AddButton(go, new KIconButtonMenu.ButtonInfo(
                "action_building_disabled",
                exempt ? Languages.UI.USERMENUACTIONS.MELT_EXEMPT.NAME_OFF
                       : Languages.UI.USERMENUACTIONS.MELT_EXEMPT.NAME,
                ToggleExempt,
                tooltipText: exempt ? Languages.UI.USERMENUACTIONS.MELT_EXEMPT.TOOLTIP_OFF
                                    : Languages.UI.USERMENUACTIONS.MELT_EXEMPT.TOOLTIP));
        }

        private void ToggleExempt()
        {
            // 只负责豁免标记的切换;宽限名单与预警通知统一由扫描器每秒处理
            KPrefabID id = GetComponent<KPrefabID>();
            if (id == null)
            {
                return;
            }
            if (id.HasTag(ModTags.MeltExempt))
            {
                id.RemoveTag(ModTags.MeltExempt);
            }
            else
            {
                id.AddTag(ModTags.MeltExempt, serialize: true);
            }
        }
    }

    // 所有建筑建造完成(BuildingComplete.OnSpawn)时挂豁免按钮组件,含读档重建与 mod 建筑
    [HarmonyPatch(typeof(BuildingComplete), "OnSpawn")]
    public static class BuildingComplete_OnSpawn_Patch
    {
        public static void Postfix(BuildingComplete __instance)
        {
            __instance.gameObject.AddOrGet<BuildingMeltExemptButton>();
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

namespace Unlock_Cheat
{
    public static partial class Languages
    {
        public partial class UI
        {
            public class NOTIFICATIONS
            {
                public class BUILDING_MELT_WARNING
                {
                    public static LocString NAME = "即将融化";

                    public static LocString TOOLTIP = "以下建筑已超过材料熔点，将在30秒宽限期后融化。冷却至熔点以下、禁用建筑或点击豁免可取消融化:";
                }
            }

            public class USERMENUACTIONS
            {
                public class MELT_EXEMPT
                {
                    public static LocString NAME = "豁免融化";

                    public static LocString NAME_OFF = "取消豁免";

                    public static LocString TOOLTIP = "该建筑将被豁免，不再融化（随存档保存）";

                    public static LocString TOOLTIP_OFF = "取消豁免：若温度仍超过熔点，将重新进入30秒融化宽限期";
                }
            }
        }
    }
}
