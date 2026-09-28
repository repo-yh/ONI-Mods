using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unlock_Cheat.MutantPlants
{
    [SkipSaveFileSerialization]
    public class PlantMutationOptionsController : KMonoBehaviour, FewOptionSideScreen.IFewOptionSideScreen
    {
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
