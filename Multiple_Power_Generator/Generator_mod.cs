using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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
        [HarmonyPatch(typeof(StructureTemperaturePayload), "OperatingKilowatts", MethodType.Getter)]
        public static class OperatingKilowatts_Patch
        {
            // 有变压器组件(含第三方):放电端电网有用电(>0)→原版热量;没用电(0/未连)→0
            public static bool Prefix(StructureTemperaturePayload __instance, ref float __result)
            {
                Building building = __instance.building;
                if (building == null) return true;
                PowerTransformer transformer = building.GetComponent<PowerTransformer>();
                if (transformer == null) return true;
                float wattsUsed = Game.Instance.circuitManager.GetWattsUsedByCircuit(transformer.CircuitID);
                if (wattsUsed > 0f) return true;
                __result = 0f;
                return false;
            }
        }
    }
}
