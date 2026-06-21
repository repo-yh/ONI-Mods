using HarmonyLib;
using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NuclearResearch
{

    internal class NuclearResearch_Patch
    {


        [HarmonyPatch(typeof(NuclearResearchCenterConfig))]
        [HarmonyPatch("ConfigureBuildingTemplate")]
        public class NuclearResearchCenterConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(NuclearResearchCenterConfig __instance, GameObject go)
            {
                go.AddOrGet<NuclearResearchCenter_SideScreen>();

            }
        }

    }
    internal class NuclearResearchCenter_SideScreen : KMonoBehaviour, ISingleSliderControl, ISliderControl
    {
        public string SliderTitleKey
        {
            get
            {
                return "Unlock_Cheat.Languages.UI.NUCLEARRESEARCHCENTER.TITLE";
            }
        }


        public string SliderUnits
        {
            get
            {
                return UI.UNITSUFFIXES.HIGHENERGYPARTICLES.PARTRICLES;
            }
        }

        public int SliderDecimalPlaces(int index)
        {
            return 0;
        }

        public float GetSliderMin(int index)
        {
            return (float)this.minSlider;
        }

        public float GetSliderMax(int index)
        {
            return (float)this.maxSlider;
        }

        public float GetSliderValue(int index)
        {
            return storage.capacity;
        }

        public void SetSliderValue(float value, int index)
        {
            storage.capacity = value;
            RefreshStorage.Invoke(storage, Args);

        }

        public string GetSliderTooltipKey(int index)
        {
            return "Unlock_Cheat.Languages.UI.NUCLEARRESEARCHCENTER.TOOLTIP";
        }

        string ISliderControl.GetSliderTooltip(int index)
        {
            return string.Format(Strings.Get("Unlock_Cheat.Languages.UI.NUCLEARRESEARCHCENTER.TOOLTIP"), storage.capacity);
        }

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            base.Subscribe<NuclearResearchCenter_SideScreen>(-905833192, NuclearResearchCenter_SideScreen.OnCopySettingsDelegate);
        }

        public override void OnCleanUp()
        {
            base.Unsubscribe<NuclearResearchCenter_SideScreen>(-905833192, NuclearResearchCenter_SideScreen.OnCopySettingsDelegate, false);
            base.OnCleanUp();
        }
        private void OnCopySettings(object data)
        {
            NuclearResearchCenter_SideScreen component = ((GameObject)data).GetComponent<NuclearResearchCenter_SideScreen>();
            if (component != null)
            {
               storage.capacity = component.storage.capacity;
            }
        }


        private static readonly EventSystem.IntraObjectHandler<NuclearResearchCenter_SideScreen> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<NuclearResearchCenter_SideScreen>(delegate (NuclearResearchCenter_SideScreen component, object data)
        {
            component.OnCopySettings(data);
        });

#pragma warning disable CS0649, CS0169,CS8618 // 禁用 "从未赋值" 警告
        [HideInInspector]
        [MyCmpReq]
        private HighEnergyParticleStorage storage;
#pragma warning restore CS0649, CS0169,CS8618  // 恢复警告

        public int minSlider = 100;

        public int maxSlider = 9999;

        MethodInfo RefreshStorage = typeof(HighEnergyParticleStorage).GetMethod("DeltaParticles", BindingFlags.Instance | BindingFlags.NonPublic);

        private static object[] Args = new object[] { 0};
    }
}
