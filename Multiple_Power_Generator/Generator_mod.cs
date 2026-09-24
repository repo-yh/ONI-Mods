using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Multiple_Power_Generator
{

    internal class HarmonyPatches : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {

            PUtil.InitLibrary(false);
            new POptions().RegisterOptions(this, typeof(Options));
            base.OnLoad(harmony);

        }
    }

        internal class Patches
    {
        

        [HarmonyPatch(typeof(Generator), "WattageRating", MethodType.Getter)]
        public class Generator_WattageRating
        {
            private static void Postfix(ref float __result)
            {
                __result *= SingletonOptions<Options>.Instance.PowerRatio;
            }
        }

        [HarmonyPatch(typeof(Generator), "BaseWattageRating", MethodType.Getter)]
        public class Generator_BaseWattageRating
        {
            private static void Postfix(ref float __result)
            {
                __result *= SingletonOptions<Options>.Instance.PowerRatio;
            }
        }

        [HarmonyPatch(typeof(Generator), "CalculateCapacity")]
        public class Generator_CalculateCapacity
        {
            private static void Postfix(ref float __result)
            {
                __result *= SingletonOptions<Options>.Instance.PowerRatio;
            }
        }

        [HarmonyPatch(typeof(Wire), "GetMaxWattageAsFloat")]
        public class Wire_GetMaxWattageAsFloat
        {
            private static void Postfix(ref float __result)
            {
                __result *= SingletonOptions<Options>.Instance.WireRatio;
            }
        }



        [HarmonyPatch(typeof(Battery), "PercentFull", MethodType.Getter)]
        public class Battery_PercentFull
        {
            private static void Postfix(ref float __result)
            {
                __result = Mathf.Min(__result,1);
            }
        }

        [HarmonyPatch(typeof(Battery), "PreviousPercentFull", MethodType.Getter)]
        public class Battery_PreviousPercentFull
        {
            private static void Postfix(ref float __result)
            {
                __result = Mathf.Min(__result, 1);
            }
        }

        [HarmonyPatch(typeof(BatteryUI), "Initialize")]
        public class BatteryUI_Initialize
        {
            private static void Prefix(ref Dictionary<float, float> ___sizeMap)
            {
                if (___sizeMap != null && ___sizeMap.Count > 0)
                {
                    return;
                }
                ___sizeMap = new Dictionary<float, float>();                
                ___sizeMap.TryAdd(20000f * SingletonOptions<Options>.Instance.BatteryRatio, 10f);
                ___sizeMap.TryAdd(40000f * SingletonOptions<Options>.Instance.BatteryRatio, 25f);
                ___sizeMap.TryAdd(60000f * SingletonOptions<Options>.Instance.BatteryRatio, 40f);
            }
        }


        [HarmonyPatch(typeof(BatteryUI), "SetContent")]
        public class BatteryUI_SetContent
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                float limit = 40000f * SingletonOptions<Options>.Instance.BatteryRatio;

                Debug.Log(" === Transpiler applied === ");
                return instructions.Manipulator(
                    instr => instr.opcode == OpCodes.Ldc_R4 && ((float)instr.operand) == 40000f,  // 匹配条件
                    instr =>  instr.operand = limit // 修改动作：替换 operand
                );
            }

        }

        [HarmonyPatch(typeof(BatteryMediumConfig), "DoPostConfigureComplete")]
        public class Battery_BatteryMediumConfig
        {
            public static void Postfix(BatteryMediumConfig __instance, GameObject go)
            {
                Battery battery = go.AddOrGet<Battery>();
                battery.capacity = battery.capacity * SingletonOptions<Options>.Instance.BatteryRatio;


            }
        }

        [HarmonyPatch(typeof(BatteryConfig), "DoPostConfigureComplete")]
        public class Battery_BatteryConfig
        {
            public static void Postfix(BatteryConfig __instance, GameObject go)
            {
                Battery battery = go.AddOrGet<Battery>();
                battery.capacity = battery.capacity * SingletonOptions<Options>.Instance.BatteryRatio;


            }
        }
        [HarmonyPatch(typeof(BatteryModuleConfig), "DoPostConfigureComplete")]
        public class Battery_BatteryModuleConfig
        {
            public static void Postfix(BatteryModuleConfig __instance, GameObject go)
            {
                Battery battery = go.AddOrGet<Battery>();
                battery.capacity = battery.capacity * SingletonOptions<Options>.Instance.BatteryRatio;


            }
        }
        [HarmonyPatch(typeof(BatterySmartConfig), "DoPostConfigureComplete")]
        public class Battery_BatterySmartConfig
        {
            public static void Postfix(BatterySmartConfig __instance, GameObject go)
            {
                Battery battery = go.AddOrGet<Battery>();
                battery.capacity = battery.capacity * SingletonOptions<Options>.Instance.BatteryRatio;


            }
        }
        [HarmonyPatch(typeof(PowerTransformerConfig), "DoPostConfigureComplete")]
        public class Battery_PowerTransformerConfig
        {
            public static void Postfix(PowerTransformerConfig __instance, GameObject go)
            {
                Battery battery = go.AddOrGet<Battery>();
                battery.capacity *= SingletonOptions<Options>.Instance.BatteryRatio;
                // 充电侧倍率取发电机/电池/电线三项最小值，充电负担不超过最薄弱环节
                battery.chargeWattage *= Mathf.Min(SingletonOptions<Options>.Instance.PowerRatio, SingletonOptions<Options>.Instance.BatteryRatio, SingletonOptions<Options>.Instance.WireRatio);
            }
        }
        [HarmonyPatch(typeof(PowerTransformerSmallConfig), "DoPostConfigureComplete")]
        public class Battery_PowerTransformerSmallConfig
        {
            public static void Postfix(PowerTransformerSmallConfig __instance, GameObject go)
            {
                Battery battery = go.AddOrGet<Battery>();
                battery.capacity *= SingletonOptions<Options>.Instance.BatteryRatio;
                battery.chargeWattage *= Mathf.Min(SingletonOptions<Options>.Instance.PowerRatio, SingletonOptions<Options>.Instance.BatteryRatio, SingletonOptions<Options>.Instance.WireRatio);
            }
        }
        [HarmonyPatch(typeof(Battery), "OnSpawn")]
        public class Battery_OnSpawn
        {
            // 变压器充电端挂"当前充电功率"状态项:悬浮卡片与选中侧边栏实时显示从上游抽取的功率(含第三方变压器)
            public static void Postfix(Battery __instance)
            {
                if (__instance.powerTransformer == null) return;
                // SetStatusItem 按 category 单槽位,通电/断电时官方 OnTagsChanged 会把 BatteryJoulesAvailable
                // 挂回 Power 分类挤掉本项;AddStatusItem 走无分类条目,与官方项共存
                __instance.GetComponent<KSelectable>()?.AddStatusItem(TransformerChargeWattageItem.Item, __instance);
            }
        }
        [HarmonyPatch(typeof(Battery), "EnergySim200ms")]
        public static class Battery_EnergySim200ms
        {
            // Sim 端自发热功率仅在 StructureTemperatureHeader.dirty 时经 UpdateSimState 提交,
            // dirty 只随通电/断电翻转置位;判据依赖连续量 WattsUsed,负载变化不触发 dirty,
            // Sim 端会冻结在上次提交值(空载仍产热/带载不产热)。检测判据翻转,翻转时
            // 同步 Sim 端温度回 InternalTemperature 并置 dirty,让官方链路提交新值。
            internal static readonly Dictionary<Battery, bool> lastHeatState = new Dictionary<Battery, bool>();

            public static void Postfix(Battery __instance)
            {
                if (__instance.powerTransformer == null) return;
                bool shouldHeat = TransformerChargeWattageItem.ShouldHeat(__instance);
                if (lastHeatState.TryGetValue(__instance, out bool boxed)  && boxed == shouldHeat) return;
                lastHeatState[__instance]= shouldHeat;
                HandleVector<int>.Handle handle = GameComps.StructureTemperatures.GetHandle(__instance.gameObject);
                if (!handle.IsValid()) return;
                GameComps.StructureTemperatures.GetData(handle, out var header, out var payload);
                if (payload.primaryElement == null) return;
                payload.primaryElement.InternalTemperature = payload.primaryElement.Temperature;
                header.dirty = true;
                GameComps.StructureTemperatures.SetHeader(handle, header);
            }
        }

        [HarmonyPatch(typeof(Battery), "OnCleanUp")]
        public static class Battery_OnCleanUp
        {
            public static void Prefix(Battery __instance)
            {
                Battery_EnergySim200ms.lastHeatState.Remove(__instance);
            }
        }
        public static class TransformerChargeWattageItem
        {
            public static readonly StatusItem Item = new StatusItem(
                "MPG_TransformerChargeWattage",
                STRINGS.BUILDING.STATUSITEMS.SOLARPANELWATTAGE.NAME,
                "",
                "",
                StatusItem.IconType.Info,
                NotificationType.Neutral,
                allow_multiples: false,
                OverlayModes.Power.ID);

            // 发热判据:实际从上游抽电超自泄漏+1W 才发热(OperatingKilowatts patch 与 dirty 同步共用)
            public static bool ShouldHeat(Battery battery)
            {
                return battery.WattsUsed > battery.joulesLostPerSecond + 1f;
            }

            static TransformerChargeWattageItem()
            {
                Item.resolveStringCallback = delegate (string str, object data)
                {
                    Battery battery = (Battery)data;
                    str = str.Replace("{Wattage}", GameUtil.GetFormattedWattage(battery.WattsUsed));
                    return str;
                };
            }
        }
        [HarmonyPatch(typeof(StructureTemperaturePayload), "OperatingKilowatts", MethodType.Getter)]
        public static class OperatingKilowatts_Patch
        {
            // 有变压器组件(含第三方):实际在从上游抽电(WattsUsed>自泄漏+1J/s)→原版热量;否则(空载/断线补漏)→0
            public static bool Prefix(StructureTemperaturePayload __instance, ref float __result)
            {
                Building building = __instance.building;
                if (building == null) return true;
                PowerTransformer transformer = building.GetComponent<PowerTransformer>();
                if (transformer == null) return true;
                Battery battery = building.GetComponent<Battery>();
                if (battery == null) return true;
                if (TransformerChargeWattageItem.ShouldHeat(battery)) return true;
                __result = 0f;
                return false;
            }
        }
    }
}
