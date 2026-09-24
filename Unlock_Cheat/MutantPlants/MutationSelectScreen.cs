using System.Collections.Generic;
using System.Linq;
using Klei.AI;
using UnityEngine;
using UnityEngine.UI;

namespace Unlock_Cheat.MutantPlants
{
    internal class MutationSelectScreen : KScreen
    {
        private static MutationSelectScreen prefab;
        private static MutationSelectScreen activeInstance;

        public GameObject rowPrefab;
        public RectTransform rowContainer;

        private MutantPlant targetMutant;
        private readonly List<GameObject> rows = new List<GameObject>();

        private int selectObjectHandle = -1;

        public static void Toggle(MutantPlant mutant)
        {
            bool showing = IsShowing(mutant);
            Debug.Log($"[MutantPlants] Toggle变异面板: {mutant.PrefabID().Name}, 当前已打开={showing}");
            if (showing)
            {
                activeInstance.Close();
            }
            else
            {
                Show(mutant);
            }
        }

        public static bool IsShowing(MutantPlant mutant)
        {
            if (activeInstance == null || activeInstance.targetMutant != mutant)
            {
                return false;
            }
            return activeInstance.gameObject.activeSelf;
        }

        public static void Show(MutantPlant mutant)
        {
            if (mutant == null || DetailsScreen.Instance == null)
            {
                return;
            }
            if (prefab == null)
            {
                prefab = CreatePrefab();
                Debug.Log("[MutantPlants] 创建变异面板模板(每存档一次)");
            }
            MutationSelectScreen screen = (MutationSelectScreen)DetailsScreen.Instance.SetSecondarySideScreen(prefab, Languages.UI.USERMENUACTIONS.MUTATOR.NAME);
            screen.SetMutant(mutant);
            Debug.Log($"[MutantPlants] Show变异面板: {mutant.PrefabID().Name}");
        }

        private static MutationSelectScreen CreatePrefab()
        {
            GameObject gameObject = new GameObject("MutationSelectScreenPrefab", typeof(RectTransform));
            gameObject.SetActive(value: false);
            gameObject.transform.SetParent(DetailsScreen.Instance.transform, false);
            MutationSelectScreen screen = gameObject.AddComponent<MutationSelectScreen>();
            screen.BuildUI();
            return screen;
        }

        public void SetMutant(MutantPlant mutant)
        {
            activeInstance = this;
            targetMutant = mutant;
            RefreshRows();
        }

        public void Close()
        {
            activeInstance = null;
            MutantPlant mutant = targetMutant;
            targetMutant = null;
            if (DetailsScreen.Instance != null)
            {
                DetailsScreen.Instance.ClearSecondarySideScreen();
            }
            if (mutant != null && Game.Instance != null)
            {
                Game.Instance.userMenu.Refresh(mutant.gameObject);
            }
        }

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            selectObjectHandle = Subscribe(Game.Instance.gameObject, (int)GameHashes.SelectObject, OnSelectObject);
            Debug.Log("[MutantPlants] 变异面板实例激活, 已订阅SelectObject");
        }

        public override void OnCleanUp()
        {
            Debug.Log("[MutantPlants] 变异面板清理, 退订SelectObject");
            if (selectObjectHandle != -1 && Game.Instance != null)
            {
                Unsubscribe(Game.Instance.gameObject, ref selectObjectHandle);
            }
            base.OnCleanUp();
        }

        private void OnSelectObject(object data)
        {
            if (gameObject.activeSelf)
            {
                Debug.Log($"[MutantPlants] SelectObject广播触发自动关闭: {(data as GameObject)?.name ?? "null"}");
                Close();
            }
        }

        private void BuildUI()
        {
            RectTransform root = (RectTransform)transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            GameObject scrollRoot = new GameObject("ScrollRoot", typeof(RectTransform));
            scrollRoot.transform.SetParent(root, false);
            RectTransform scrollRectTransform = (RectTransform)scrollRoot.transform;
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = Vector2.zero;
            scrollRectTransform.offsetMax = Vector2.zero;
            KScrollRect kScrollRect = scrollRoot.AddComponent<KScrollRect>();

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(scrollRoot.transform, false);
            RectTransform viewportRect = (RectTransform)viewport.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0.01f, 0.01f, 0.01f, 0.01f);
            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 2f;
            layoutGroup.padding = new RectOffset(4, 4, 4, 4);
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            ContentSizeFitter contentSizeFitter = content.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            kScrollRect.content = contentRect;
            kScrollRect.viewport = viewportRect;
            rowContainer = contentRect;

            rowPrefab = CreateRowTemplate();
        }

        private GameObject CreateRowTemplate()
        {
            GeoTunerSideScreen geoTunerScreen = FindGeoTunerSideScreen();
            if (geoTunerScreen == null || geoTunerScreen.rowPrefab == null)
            {
                Debug.LogWarning("[MutantPlants] 未找到官方 GeoTunerSideScreen 行模板");
                return null;
            }
            GameObject row = Util.KInstantiateUI(geoTunerScreen.rowPrefab, gameObject, false);
            row.name = "RowTemplate";
            row.SetActive(value: false);
            Debug.Log("[MutantPlants] 克隆GeoTuner行模板成功");
            return row;
        }

        private static GeoTunerSideScreen FindGeoTunerSideScreen()
        {
            GeoTunerSideScreen[] screens = Resources.FindObjectsOfTypeAll<GeoTunerSideScreen>();
            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i].rowPrefab != null)
                {
                    return screens[i];
                }
            }
            return null;
        }

        private void RefreshRows()
        {
            List<PlantMutation> applicable = GetApplicableMutations(targetMutant);
            Debug.Log($"[MutantPlants] 刷新变异行: {applicable.Count} 条");
            for (int i = 0; i < applicable.Count; i++)
            {
                SetRow(i, applicable[i]);
            }
            for (int j = applicable.Count; j < rows.Count; j++)
            {
                rows[j].SetActive(value: false);
            }
        }

        private static List<PlantMutation> GetApplicableMutations(MutantPlant mutant)
        {
            string prefabID = mutant.PrefabID().Name;
            return Db.Get().PlantMutations.resources.Where((PlantMutation m) => !m.originalMutation
                && !m.restrictedPrefabIDs.Contains(prefabID)
                && (m.requiredPrefabIDs.Count == 0 || m.requiredPrefabIDs.Contains(prefabID))).ToList();
        }

        private void SetRow(int idx, PlantMutation mutation)
        {
            GameObject row;
            if (idx < rows.Count)
            {
                row = rows[idx];
                row.SetActive(value: true);
            }
            else
            {
                if (rowPrefab == null)
                {
                    return;
                }
                row = Util.KInstantiateUI(rowPrefab, rowContainer.gameObject, true);
                row.transform.SetSiblingIndex(idx);
                rows.Add(row);
            }
            HierarchyReferences references = row.GetComponent<HierarchyReferences>();
            if (references != null)
            {
                LocText label = references.GetReference<LocText>("label");
                if (label != null)
                {
                    label.text = mutation.Name;
                }
                Image icon = references.GetReference<Image>("icon");
                if (icon != null)
                {
                    icon.sprite = PopFXManager.Instance.sprite_Resource;
                }
            }
            ToolTip[] componentsInChildren = row.GetComponentsInChildren<ToolTip>();
            if (componentsInChildren.Length > 0)
            {
                componentsInChildren[0].SetSimpleTooltip(mutation.GetTooltip());
                componentsInChildren[0].enabled = true;
            }
            if (componentsInChildren.Length > 1)
            {
                componentsInChildren[1].enabled = false;
            }
            MultiToggle toggle = row.GetComponent<MultiToggle>();
            if (toggle != null)
            {
                PlantMutation clicked = mutation;
                toggle.onClick = delegate
                {
                    ApplyMutation(clicked);
                };
            }
        }

        private void ApplyMutation(PlantMutation mutation)
        {
            if (targetMutant == null)
            {
                return;
            }
            Debug.Log($"[MutantPlants] 应用定向变异: {mutation.Id} → {targetMutant.PrefabID().Name}");
            targetMutant.Mutator(mutation.Id);
            Close();
        }
    }
}
