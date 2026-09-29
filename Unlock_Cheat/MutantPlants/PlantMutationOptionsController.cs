using EventSystem2Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Unlock_Cheat.MutantPlants
{
    [SkipSaveFileSerialization]
    public class PlantMutationOptionsController : KMonoBehaviour, FewOptionSideScreen.IFewOptionSideScreen, ISidescreenButtonControl
    {
        public bool IsPanelOpen { get; private set; }

        public void Toggle()
        {
            this.IsPanelOpen = !this.IsPanelOpen;
            if (this.selectable == null || !this.selectable.IsSelected)
            {
                return;
            }
            base.Trigger(1980521255, null);
        }

        private static readonly EventSystem.IntraObjectHandler<PlantMutationOptionsController> OnSelectChangedHandler =
            new EventSystem.IntraObjectHandler<PlantMutationOptionsController>((component, data) => component.OnSelectChanged(data));

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-1503271301, OnSelectChangedHandler);
        }

        public override void OnCleanUp()
        {
            Unsubscribe(-1503271301, OnSelectChangedHandler, false);
            base.OnCleanUp();
        }

        private void OnSelectChanged(object data)
        {
            if (!(data is Boxed<bool> boxed) || boxed.value || !this.IsPanelOpen)
            {
                return;
            }
            this.IsPanelOpen = false;
        }

        // 备选方案（未启用）：ISim1000ms 每秒轮询替代事件订阅自动关面板。
        // KMonoBehaviour 基类 autoRegisterSimRender 默认 true，实现接口即自动注册/注销，无需手动 Add/Remove。
        // 与事件订阅的差异：关闭最多延迟 1 秒；切走后 1 秒内切回面板仍显示。
        // 启用方式：类声明追加 ", ISim1000ms"，并取消下方方法注释。
        //public void Sim1000ms(float dt)
        //{
        //    if (!this.IsPanelOpen || this.selectable == null || this.selectable.IsSelected)
        //    {
        //        return;
        //    }
        //    this.IsPanelOpen = false;
        //}

        public string SidescreenButtonText
        {
            get
            {
                return Languages.UI.USERMENUACTIONS.MUTATORBUTTON.NAME;
            }
        }


        public string SidescreenButtonTooltip
        {
            get
            {

                return Languages.UI.USERMENUACTIONS.MUTATORBUTTON.TOOLTIP;
            }
        }

        public void SetButtonTextOverride(ButtonMenuTextOverride textOverride)
        {

        }

        public bool SidescreenEnabled()
        {

            return IsPanelOpen;
        }


        public bool SidescreenButtonInteractable()
        {

            return true;
        }

        public void OnSidescreenButtonPressed()
        {


            MutantPlant component = base.GetComponent<MutantPlant>();

            if (!component)
            {
                return;

            }
            if (selectedOptionTag == Tag.Invalid) {

                PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Resource, Languages.UI.USERMENUACTIONS.MUTATORBUTTON.ALERT, component.transform, 3f, false);
                return;

            }

            component.SetSubSpecies(new List<string> { selectedOptionTag.Name });
            component.ApplyMutator();
            //SelectTool.Instance.Select(null, true);
            //SelectTool.Instance.Select(this.selectable, true);
            Toggle();
            Game.Instance.Trigger(-1503271301, this.gameObject);


        }

        [MyCmpReq]
        private KSelectable selectable;
        public int HorizontalGroupID()
        {
            return -1;
        }
        public int ButtonSideScreenSortOrder()
        {
            return -10;
        }

        public string SidescreenTitle
        {
            get
            {
                return Languages.UI.USERMENUACTIONS.MUTATOR.NAME;
            }
        }
        public FewOptionSideScreen.IFewOptionSideScreen.Option[] GetOptions()
        {
            MutantPlant mutant = GetComponent<MutantPlant>();
            if (mutant == null || Db.Get() == null || Db.Get().PlantMutations == null)
                return Array.Empty<FewOptionSideScreen.IFewOptionSideScreen.Option>();

            string prefabID = GetComponent<KPrefabID>().PrefabID().Name;
            return Db.Get().PlantMutations.resources
                .Where(m => !m.originalMutation
                         && !m.restrictedPrefabIDs.Contains(prefabID)
                         && (m.requiredPrefabIDs.Count == 0 || m.requiredPrefabIDs.Contains(prefabID)))
                .Select(m => new FewOptionSideScreen.IFewOptionSideScreen.Option(
                    new Tag(m.Id),
                    m.Name,
                    new Tuple<Sprite, Color>(null, Color.clear),
                    m.GetTooltip()))
                .ToArray();
        }

        private Tag selectedOptionTag = Tag.Invalid;

        public void OnOptionSelected(FewOptionSideScreen.IFewOptionSideScreen.Option option)
        {
            selectedOptionTag = option.tag;
            Debug.LogFormat("[PlantMutationOptions] 点击变异行：{0}", option.tag.Name);
        }

        public Tag GetSelectedOption()
        {
            return selectedOptionTag;
        }
    }
}
