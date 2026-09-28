using EventSystem2Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static DiscreteShadowCaster;

namespace Unlock_Cheat.MutantPlants
{
    [SkipSaveFileSerialization]
    public class PlantMutationOptionsController : KMonoBehaviour, FewOptionSideScreen.IFewOptionSideScreen, ISidescreenButtonControl
    {
        public bool IsPanelOpen { get; private set; }

        public void Toggle()
        {
            this.IsPanelOpen = !this.IsPanelOpen;
            base.Trigger(493375141, null);
            if (this.selectable == null || !this.selectable.IsSelected || UIScheduler.Instance == null)
            {
                return;
            }
            UIScheduler.Instance.ScheduleNextFrame("refresh standalone geyser conversion", delegate (object _)
            {
                if (this == null || this.selectable == null || !this.selectable.IsSelected || SelectTool.Instance == null)
                {
                    return;
                }
                SelectTool.Instance.Select(null, true);
                SelectTool.Instance.Select(this.selectable, true);
            }, null, null);
        }

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

            {

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
            return 60;
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
                    Def.GetUISprite("icon_category_food"),
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
