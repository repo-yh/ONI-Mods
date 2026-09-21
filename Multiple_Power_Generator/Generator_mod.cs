using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        [HarmonyPatch(typeof(BatteryUI), "SetContent")]
        public class BatteryUI_SetContent
        {
            private static void Prefix(Battery bat, Dictionary<float, float> ___sizeMap)
            {
                // 首次表为空时官方 Initialize 尚未填充，提前插入会占位导致官方三档丢失，直接跳过
                if (___sizeMap == null || ___sizeMap.Count == 0)
                {
                    return;
                }
                ___sizeMap.TryAdd(bat.Capacity, 40f);
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


            }
        }
        [HarmonyPatch(typeof(PowerTransformerSmallConfig), "DoPostConfigureComplete")]
        public class Battery_PowerTransformerSmallConfig
        {
            public static void Postfix(PowerTransformerSmallConfig __instance, GameObject go)
            {
                Battery battery = go.AddOrGet<Battery>();
                battery.capacity *= SingletonOptions<Options>.Instance.BatteryRatio;


            }
        }
    }
}
