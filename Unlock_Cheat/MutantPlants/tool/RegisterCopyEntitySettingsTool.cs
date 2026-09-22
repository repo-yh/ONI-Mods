using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unlock_Cheat.MutantPlants.CopySetting;


namespace Unlock_Cheat.MutantPlants.CopySettingPatch
{
    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnPrefabInit))]
    public static class RegisterCopyEntitySettingsTool
    {
        static void Postfix(PlayerController __instance)
        {
            var interfaceTools = new List<InterfaceTool>(__instance.tools);
            var critterCopyTool = new GameObject(typeof(MutantCopySettingsTool).Name);
            critterCopyTool.AddComponent<MutantCopySettingsTool>();

            // Reparent tool to the player controller, then enable/disable to load it
            critterCopyTool.transform.SetParent(__instance.gameObject.transform);
            critterCopyTool.gameObject.SetActive(true);
            critterCopyTool.gameObject.SetActive(false);

            interfaceTools.Add(critterCopyTool.GetComponent<InterfaceTool>());
            __instance.tools = interfaceTools.ToArray();
        }



    }

    [HarmonyPatch(typeof(Assets), "CreatePrefabs")]
    internal class ApplySettingsToDefs
    {
        private static void Postfix()
        {
            ComponentMapper componentMapper = new ComponentMapper(new()
            {
                 (typeof(Uprootable), typeof(MutantCopyButton))
            });
            foreach (KPrefabID kprefabID in Assets.Prefabs)
            {
                componentMapper.ApplyMap(kprefabID.gameObject);
            }
        }
    }

    public class ComponentMapper : ComponentMapper<object>
    {
        public ComponentMapper(List<(Type flagCmp, Type addCmp)> map) : base(map.Select(x => (x.flagCmp, x.addCmp, (object)null)).ToList())
        { }

        public void ApplyMap(GameObject go) => ApplyMap(go, _ => true);
    }

    public class ComponentMapper<T>
    {
        private readonly List<(Type flagCmp, Type addCmp, T filter)> map;

        public ComponentMapper(List<(Type flagCmp, Type addCmp, T filter)> map) => this.map = map;

        public void ApplyMap(GameObject go, Func<T, bool> shouldAdd)
        {
            var typeToAdd = GetTypeToAdd(go, shouldAdd);
            if (typeToAdd != null)
                go.AddComponent(typeToAdd);
        }

        private Type GetTypeToAdd(GameObject go, Func<T, bool> shouldAdd)
        {
            foreach (var (flagCmp, addCmp, filter) in map)
                if (flagCmp != null && HasComponentOrDef(flagCmp, go) && shouldAdd(filter))
                    return addCmp;
            return null;

            bool HasComponentOrDef(Type cmpOrDef, GameObject go) => go.GetComponent(cmpOrDef) ?? go.GetDef(cmpOrDef) != null;
        }
    }
    public static class GameObjectExt
    {
        public static Component GetReflectionComp(this GameObject go, string typeString)
        {
            var type = AccessTools.TypeByName(typeString);
            if (type == null)
                return null;
            else
                return go.GetComponent(type);
        }

        public static StateMachine.BaseDef GetDef(this GameObject go, Type type)
        {
            var smc = go.GetComponent<StateMachineController>();
            if (smc == null)
                return null;

            return smc.cmpdef.defs.FirstOrDefault(x => x.GetType() == type);
        }
    }
}