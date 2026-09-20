using Database;
using HarmonyLib;
using laz_yh.SelectLastCarePackage;
using STRINGS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Localization;
using static STRINGS.UI.STARMAP;

namespace SelectLastCarePackage.CarePackagePanel
{
    public class CarePackagePanel : KScreen
    {
        // 排到输入栈顶（CarePackageContainer 的 GetSortKey 是 50 且会消费按键），优先接收 Esc 等按键
        public override float GetSortKey()
        {
            return MODAL_SCREEN_SORT_KEY;
        }

        public override void OnKeyDown(KButtonEvent e)
        {
            bool flag = e.TryConsume(global::Action.Escape);
            if (flag)
            {
                this.Close();
                e.Consumed = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }
        protected override void OnActivate()
        {
            base.OnActivate();
            if (instance._title != null)
            {
                instance._title.SetText(Languages.TO_REPLACE + " " + instance.GetCurrentContainer());
            }
            instance.ApplyFilter(string.Empty);
        }
        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }
        public static void Open(ImmigrantScreen target, CarePackageContainer container)
        {
            bool flag = target.gameObject == null;
            if (!flag)
            {
                bool flag2 = CarePackagePanel.instance != null;
                if (flag2)
                {
                    CarePackagePanel.instance.Close();
                    CarePackagePanel.instance = null;
                }
                GameObject gameObject = new GameObject("CarePackageUI", new Type[]
				{
					typeof(RectTransform),
					typeof(CanvasRenderer),
					typeof(Image),
					typeof(Canvas)
				});

                gameObject.transform.SetParent(target.transform, false);





                instance =  gameObject.AddComponent<CarePackagePanel>();
                instance.Container = container;
                instance.ConsumeMouseScroll = true;


                instance.BuildUI(gameObject, container);


                instance.Activate();

                //CarePackagePanel.instance.targetObject = target;
                //CarePackagePanel.instance.rootCanvas = gameObject;
                //CarePackagePanel.instance.Activate();
            }
        }
        private string GetCurrentContainer()
        {
            if (this.Container == null)
            {
                return string.Empty;
            }
            CarePackageInfo value = Traverse.Create(this.Container).Field("info").GetValue<CarePackageInfo>();
            if (value == null)
            {
                return string.Empty;
            }
            return CarePackagePanel.GetName(value) + " " + CarePackagePanel.GetQuantity(value);
        }
        private  void BuildUI(GameObject gameObject, CarePackageContainer container)
        {
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            Canvas canvas = gameObject.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            Canvas container_canvas = container.GetComponentInParent<Canvas>();
            canvas.sortingOrder = ((container_canvas != null) ? container_canvas.sortingOrder : 0) + 100;



            gameObject.AddComponent<GraphicRaycaster>();

            Image image = gameObject.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.55f);
            image.raycastTarget = true;


            GameObject gameObject2 = new GameObject("CarePackagePanel", new Type[]
               {
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
               });
            gameObject2.transform.SetParent(rectTransform, false);
            RectTransform rectTransform2 = gameObject2.GetComponent<RectTransform>();
            rectTransform2.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform2.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform2.pivot = new Vector2(0.5f, 0.5f);
            rectTransform2.sizeDelta = new Vector2(580f, 540f);
            rectTransform2.anchoredPosition = Vector2.zero;
            Image image2 = gameObject2.GetComponent<Image>();
            image2.color = new Color(0.09f, 0.1f, 0.12f, 1f);
            image2.raycastTarget = true;


            this.BuildTitle(gameObject2.transform);
            this.BuildSearchBar(gameObject2.transform);
            this.BuildList(gameObject2.transform);
            this.BuildCloseButton(gameObject2.transform);
            this.All = CarePackagePanel.BuildPackages();
            this.FillRows();
        }

        private void BuildTitle(Transform transform)
        {
            this._title = CarePackagePanel.MakeText(transform, "Title", 18f);
            if (this._title == null)
            {
                return;
            }
            RectTransform rectTransform = this._title.rectTransform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.sizeDelta = new Vector2(-20f, 26f);
            rectTransform.anchoredPosition = new Vector2(0f, -6f);
            this._title.alignment = TextAlignmentOptions.Left;
            this._title.color = new Color(0.75f, 0.88f, 1f, 1f);
        }

        // Token: 0x06000035 RID: 53 RVA: 0x00003584 File Offset: 0x00001784
        private void BuildSearchBar(Transform transform)
        {
            try
            {
                GameObject gameObject = new GameObject("SearchBar", new Type[]
                {
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image)
                });
                gameObject.transform.SetParent(transform, false);
                RectTransform component = gameObject.GetComponent<RectTransform>();
                component.anchorMin = new Vector2(0f, 1f);
                component.anchorMax = new Vector2(1f, 1f);
                component.pivot = new Vector2(0.5f, 1f);
                component.sizeDelta = new Vector2(-20f, 30f);
                component.anchoredPosition = new Vector2(0f, -36f);
                gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);
                GameObject gameObject2 = new GameObject("Field", new Type[]
                {
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image)
                });
                gameObject2.transform.SetParent(gameObject.transform, false);
                Image component2 = gameObject2.GetComponent<Image>();
                component2.color = new Color(0f, 0f, 0f, 0.01f);
                component2.raycastTarget = true;
                RectTransform component3 = gameObject2.GetComponent<RectTransform>();
                component3.anchorMin = Vector2.zero;
                component3.anchorMax = Vector2.one;
                component3.offsetMin = new Vector2(6f, 2f);
                component3.offsetMax = new Vector2(-6f, -2f);
                TMP_Text tmp_Text = CarePackagePanel.MakeText(gameObject2.transform, "Hint", 16f);
                TMP_Text tmp_Text2 = CarePackagePanel.MakeText(gameObject2.transform, "Text", 16f);
                if (tmp_Text2 != null)
                {
                    tmp_Text2.color = Color.white;
                    RectTransform rectTransform = tmp_Text2.rectTransform;
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    tmp_Text2.alignment = TextAlignmentOptions.MidlineLeft;
                }
                if (tmp_Text != null)
                {
                    tmp_Text.color = new Color(1f, 1f, 1f, 0.4f);
                    RectTransform rectTransform2 = tmp_Text.rectTransform;
                    rectTransform2.anchorMin = Vector2.zero;
                    rectTransform2.anchorMax = Vector2.one;
                    rectTransform2.offsetMin = Vector2.zero;
                    rectTransform2.offsetMax = Vector2.zero;
                    tmp_Text.alignment = TextAlignmentOptions.MidlineLeft;
                    tmp_Text.SetText(Languages.SEARCH_HINT);
                }
                gameObject2.AddComponent<CarePackagePanel.SearchTyping>().Setup(tmp_Text2, new Action<string>(this.ApplyFilter));
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x06000036 RID: 54 RVA: 0x0000384C File Offset: 0x00001A4C
        private void BuildList(Transform transform)
        {
            GameObject gameObject = new GameObject("Scroll", new Type[]
            {
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
            });
            gameObject.transform.SetParent(transform, false);
            RectTransform component = gameObject.GetComponent<RectTransform>();
            component.anchorMin = Vector2.zero;
            component.anchorMax = Vector2.one;
            component.offsetMin = new Vector2(7f, 38f);
            component.offsetMax = new Vector2(-7f, -70f);
            gameObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.3f);
            ScrollRect scrollRect = gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30f;
            GameObject gameObject2 = new GameObject("Viewport", new Type[]
            {
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(Mask)
            });
            gameObject2.transform.SetParent(gameObject.transform, false);
            RectTransform component2 = gameObject2.GetComponent<RectTransform>();
            component2.anchorMin = Vector2.zero;
            component2.anchorMax = Vector2.one;
            component2.offsetMin = Vector2.zero;
            component2.offsetMax = Vector2.zero;
            gameObject2.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            gameObject2.GetComponent<Mask>().showMaskGraphic = false;
            GameObject gameObject3 = new GameObject("Content", new Type[]
            {
                    typeof(RectTransform)
            });
            gameObject3.transform.SetParent(gameObject2.transform, false);
            this._content = gameObject3.GetComponent<RectTransform>();
            this._content.anchorMin = new Vector2(0f, 1f);
            this._content.anchorMax = new Vector2(1f, 1f);
            this._content.pivot = new Vector2(0.5f, 1f);
            this._content.offsetMin = Vector2.zero;
            this._content.offsetMax = Vector2.zero;
            VerticalLayoutGroup verticalLayoutGroup = gameObject3.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childControlHeight = true;
            verticalLayoutGroup.childForceExpandWidth = true;
            verticalLayoutGroup.childForceExpandHeight = false;
            verticalLayoutGroup.spacing = 2f;
            verticalLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
            gameObject3.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = component2;
            scrollRect.content = this._content;
        }

        // Token: 0x06000037 RID: 55 RVA: 0x00003AF8 File Offset: 0x00001CF8
        private void BuildCloseButton(Transform transform)
        {
            GameObject gameObject = new GameObject("Close", new Type[]
            {
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
            });
            gameObject.transform.SetParent(transform, false);
            RectTransform component = gameObject.GetComponent<RectTransform>();
            component.anchorMin = new Vector2(0.5f, 0f);
            component.anchorMax = new Vector2(0.5f, 0f);
            component.pivot = new Vector2(0.5f, 0f);
            component.sizeDelta = new Vector2(170f, 30f);
            component.anchoredPosition = new Vector2(0f, 8f);
            gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.16f);
            TMP_Text tmp_Text = CarePackagePanel.MakeText(gameObject.transform, "Label", 17f);
            if (tmp_Text != null)
            {
                tmp_Text.SetText(Languages.BACK);
                RectTransform rectTransform = tmp_Text.rectTransform;
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                tmp_Text.alignment = TextAlignmentOptions.Center;
            }
            Button button = gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(delegate ()
            {
                this.Close();
            });
        }



        private void FillRows()
        {
            if (this._content == null)
            {
                return;
            }
            int num = 0;
            foreach (CarePackagePanel.CarePackageOption captured2 in this.All)
            {
                CarePackagePanel.CarePackageOption captured = captured2;
                GameObject gameObject = new GameObject("Row" + num.ToString(), new Type[]
                {
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image),
                        typeof(LayoutElement)
                });
                gameObject.transform.SetParent(this._content, false);
                LayoutElement component = gameObject.GetComponent<LayoutElement>();
                component.minHeight = 27f;
                component.preferredHeight = 27f;
                gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, (num % 2 == 0) ? 0.05f : 0.02f);
                GameObject gameObject2 = new GameObject("Icon", new Type[]
                {
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image)
                });
                gameObject2.transform.SetParent(gameObject.transform, false);
                RectTransform component2 = gameObject2.GetComponent<RectTransform>();
                component2.anchorMin = new Vector2(0f, 0.5f);
                component2.anchorMax = new Vector2(0f, 0.5f);
                component2.pivot = new Vector2(0f, 0.5f);
                component2.sizeDelta = new Vector2(26f, 26f);
                component2.anchoredPosition = new Vector2(5f, 0f);
                Image component3 = gameObject2.GetComponent<Image>();
                component3.raycastTarget = false;
                if (captured.icon != null)
                {
                    component3.sprite = captured.icon;
                }
                TMP_Text tmp_Text = CarePackagePanel.MakeText(gameObject.transform, "Label", 18f);
                if (tmp_Text != null)
                {
                    tmp_Text.SetText(captured.displayName + "   " + captured.quantity);
                    RectTransform rectTransform = tmp_Text.rectTransform;
                    rectTransform.anchorMin = new Vector2(0f, 0.5f);
                    rectTransform.anchorMax = new Vector2(1f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.sizeDelta = new Vector2(-40f, 24f);
                    rectTransform.anchoredPosition = new Vector2(20f, 0f);
                    tmp_Text.alignment = TextAlignmentOptions.Left;
                }
                Button button = gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(delegate ()
                {
                    this.Select(captured);
                });
                this._rows[captured.info] = gameObject;
                num++;
            }
        }


        private void Select(CarePackagePanel.CarePackageOption option)
        {
           CarePackagePanel.Replace(this.Container, option);
            this.Close();
        }
        private static List<CarePackagePanel.CarePackageOption> BuildPackages()
        {
            List<CarePackagePanel.CarePackageOption> list = new List<CarePackagePanel.CarePackageOption>();
            if (Immigration.Instance == null)
            {
                return list;
            }
            List<CarePackageInfo> value = Traverse.Create(Immigration.Instance).Field("carePackages").GetValue<List<CarePackageInfo>>();
            if (value == null)
            {
                return list;
            }
            foreach (CarePackageInfo carePackageInfo in value)
            {
                if (carePackageInfo != null && (carePackageInfo.requirement == null || carePackageInfo.requirement()))
                {
                    list.Add(new CarePackagePanel.CarePackageOption
                    {
                        info = carePackageInfo,
                        displayName = CarePackagePanel.GetName(carePackageInfo),
                        quantity = CarePackagePanel.GetQuantity(carePackageInfo),
                        icon = CarePackagePanel.GetIcon(carePackageInfo)
                    });
                }
            }
            list.Sort((CarePackagePanel.CarePackageOption a, CarePackagePanel.CarePackageOption b) => string.Compare(a.displayName, b.displayName, StringComparison.CurrentCulture));
            return list;
        }


        private static Sprite GetIcon(CarePackageInfo info)
        {
            GameObject prefab = Assets.GetPrefab(info.id);
            if (prefab != null)
            {
                if (!string.IsNullOrEmpty(info.facadeID) && info.facadeID != "SELECTRANDOM")
                {
                    return Def.GetUISprite(prefab.PrefabID(), info.facadeID).first;
                }
                return Def.GetUISprite(prefab, "ui", false).first;
            }
            else
            {
                Element element = ElementLoader.FindElementByName(info.id);
                if (element != null)
                {
                    return Def.GetUISpriteFromMultiObjectAnim(element.substance.anim, "ui", false, "");
                }
                return null;
            }
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00002954 File Offset: 0x00000B54
        private static string GetName(CarePackageInfo info)
        {
            GameObject prefab = Assets.GetPrefab(info.id);
            if (prefab != null)
            {
                if (!string.IsNullOrEmpty(info.facadeID) && info.facadeID != "SELECTRANDOM")
                {
                    return EquippableFacade.GetNameOverride(info.id, info.facadeID);
                }
                return prefab.GetProperName();
            }
            else
            {
                Element element = ElementLoader.FindElementByName(info.id);
                if (element == null)
                {
                    return info.id;
                }
                return element.substance.name;
            }
        }

        // Token: 0x06000018 RID: 24 RVA: 0x000029D4 File Offset: 0x00000BD4
        private static string GetQuantity(CarePackageInfo info)
        {
            if (ElementLoader.FindElementByName(info.id) != null)
            {
                return string.Format(UI.IMMIGRANTSCREEN.CARE_PACKAGE_ELEMENT_COUNT_ONLY, GameUtil.GetFormattedMass(info.quantity, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.UseThreshold, true, "{0:0.#}"));
            }
            EdiblesManager.FoodInfo foodInfo = EdiblesManager.GetFoodInfo(info.id);
            if (foodInfo != null && foodInfo.CaloriesPerUnit > 0f)
            {
                return string.Format(UI.IMMIGRANTSCREEN.CARE_PACKAGE_ELEMENT_COUNT_ONLY, GameUtil.GetFormattedCaloriesForItem(info.id, info.quantity, GameUtil.TimeSlice.None, true));
            }
            return string.Format(UI.IMMIGRANTSCREEN.CARE_PACKAGE_ELEMENT_COUNT_ONLY, info.quantity.ToString());
        }

        public void ApplyFilter(string filter)
        {
            string text = (filter ?? string.Empty).Trim().ToLowerInvariant();
            foreach (CarePackagePanel.CarePackageOption carePackageOption in this.All)
            {
                bool active = text.Length == 0 || CarePackagePanel.StripLink(carePackageOption.displayName).ToLowerInvariant().Contains(text) || carePackageOption.quantity.ToLowerInvariant().Contains(text) || carePackageOption.info.id.ToLowerInvariant().Contains(text);
                GameObject gameObject;
                if (this._rows.TryGetValue(carePackageOption.info, out gameObject) && gameObject != null)
                {
                    gameObject.SetActive(active);
                }
            }
        }

        private static string StripLink(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return Regex.Replace(s, "<[^>]*>", string.Empty);
        }
        private static TMP_Text MakeText(Transform parent, string name, float size)
        {
            GameObject gameObject = new GameObject(name, new Type[]
            {
        typeof(RectTransform),
        typeof(CanvasRenderer)
            });
            gameObject.transform.SetParent(parent, false);
            TextMeshProUGUI textMeshProUGUI = gameObject.AddComponent<TextMeshProUGUI>();

            textMeshProUGUI.font = Font;
            textMeshProUGUI.fontStyle = FontStyles.Normal;
          
            textMeshProUGUI.fontSize = size;
            textMeshProUGUI.color = Color.white;
            textMeshProUGUI.raycastTarget = false;
            textMeshProUGUI.textWrappingMode = TextWrappingModes.NoWrap;
            textMeshProUGUI.overflowMode = TextOverflowModes.Overflow;
            return textMeshProUGUI;
        }


        private static void Replace(CarePackageContainer container, CarePackagePanel.CarePackageOption option)
        {
            if (container == null || option == null || option.info == null)
            {
                return;
            }
            CharacterSelectionController value = Traverse.Create(container).Field("controller").GetValue<CharacterSelectionController>();
            bool flag = false;
            if (value != null && container.carePackageInstanceData != null)
            {
                flag = value.IsSelected(container.carePackageInstanceData);
                if (flag)
                {
                    container.DeselectDeliverable();
                }
            }
            Traverse.Create(container).Field("info").SetValue(option.info);
            container.carePackageInstanceData = new CarePackageContainer.CarePackageInstanceData
            {
                info = option.info,
                facadeID = ((option.info.facadeID == "SELECTRANDOM") ? CarePackagePanel.RandomFacade(option.info) : option.info.facadeID)
            };
            Traverse.Create(container).Method("ClearEntryIcons", Array.Empty<object>()).GetValue();
            Traverse.Create(container).Method("SetAnimator", Array.Empty<object>()).GetValue();
            Traverse.Create(container).Method("SetInfoText", Array.Empty<object>()).GetValue();
            if (flag)
            {
                container.SelectDeliverable();
            }
            SaveGame instance = SaveGame.Instance;
            ImmigrantScreenContext immigrantScreenContext = (instance != null) ? instance.GetComponent<ImmigrantScreenContext>() : null;
            if (immigrantScreenContext != null)
            {
                immigrantScreenContext.LastSelectedCarePackageInfo = option.info;
            }
        }

        private static string RandomFacade(CarePackageInfo info)
        {
            List<EquippableFacadeResource> list = Db.GetEquippableFacades().resources.FindAll((EquippableFacadeResource match) => match.DefID == info.id);
            if (list.Count <= 0)
            {
                return null;
            }
            return list.GetRandom<EquippableFacadeResource>().Id;
        }


        public  void Close()
        {
            gameObject.SetActive(false);
            this.Deactivate();

            if (CarePackagePanel.instance != null && CarePackagePanel.instance.gameObject != null)
            {
                UnityEngine.Object.Destroy(CarePackagePanel.instance.gameObject);

            }
            CarePackagePanel.instance = null;
        }


        private const string DEFAULT_FONT_TEXT = "NotoSansCJKsc-Regular";

        // Token: 0x04000806 RID: 2054
        private const string DEFAULT_FONT_UI = "GRAYSTROKE REGULAR SDF";

        private static TMP_Text fontTemplate;

        private static TMP_FontAsset font;
        private static FontStyles FontStyle = FontStyles.Normal;



        public static TMP_FontAsset Font
        {
            get
            {
                if (font)
                {
                    return font;
                }

                Locale locale = Localization.GetLocale();
                if (locale == null)
                {
                    font= Localization.GetFont(DEFAULT_FONT_TEXT);
                }
                else
                {
                    font = Localization.GetFont(locale.FontName);
                }
                return font;
            }

        }

        private RectTransform _content;

        // Token: 0x0400001A RID: 26
        private TMP_Text _title;
        private static bool cameraControlDisabled;

        private static CarePackagePanel instance;

        CarePackageContainer Container;
        private GameObject targetObject;

        private GameObject listContent;

        private Button confirmButton;

        private Dictionary<HashedString, GameObject> optionButtons = new Dictionary<HashedString, GameObject>();

        private HashedString selectedTypeId;

        private string selectedTypeIdString;

        private GameObject rootCanvas;

        public List<CarePackagePanel.CarePackageOption> All = new List<CarePackagePanel.CarePackageOption>();

        private readonly Dictionary<CarePackageInfo, GameObject> _rows = new Dictionary<CarePackageInfo, GameObject>();

        public class SearchTyping : KMonoBehaviour
        {
            // Token: 0x0600002C RID: 44 RVA: 0x00002F34 File Offset: 0x00001134
            public void Setup(TMP_Text text, Action<string> onChanged)
            {
                this._text = text;
                this._onChanged = onChanged;
                this._rect = base.GetComponent<RectTransform>();
                GameObject gameObject = new GameObject("Caret", new Type[]
                {
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                });
                gameObject.transform.SetParent(base.transform, false);
                this._caretImage = gameObject.GetComponent<Image>();
                this._caretImage.color = Color.white;
                this._caretImage.raycastTarget = false;
                this._caret = gameObject.GetComponent<RectTransform>();
                this._caret.anchorMin = new Vector2(0f, 0f);
                this._caret.anchorMax = new Vector2(0f, 1f);
                this._caret.pivot = new Vector2(0f, 0.5f);
                this._caret.sizeDelta = new Vector2(1.5f, -6f);
                this._caret.anchoredPosition = new Vector2(2f, 0f);
                this._caret.gameObject.SetActive(false);
            }

            private void Update()
            {
                if (Input.GetMouseButtonDown(0))
                {
                    bool editing = this._rect != null && RectTransformUtility.RectangleContainsScreenPoint(this._rect, Input.mousePosition, null);
                    this.SetEditing(editing);

                }
                if (this._caret == null || !this._caret.gameObject.activeSelf)
                {
                    return;
                }
                string inputString = Input.inputString;
                if (!string.IsNullOrEmpty(inputString))
                {
                    foreach (char c in inputString)
                    {
                        if (c == '\b')
                        {
                            if (this._value.Length > 0)
                            {
                                this._value = this._value.Substring(0, this._value.Length - 1);
                            }
                        }
                        else if (c != '\n' && c != '\r')
                        {
                            this._value += c.ToString();
                        }
                    }
                    this.Refresh();
                }
                this._blink += Time.unscaledDeltaTime;
                if (this._caretImage != null)
                {
                    this._caretImage.color = new Color(1f, 1f, 1f, (this._blink % 1f < 0.5f) ? 1f : 0f);
                }
            }

            // Token: 0x0600002E RID: 46 RVA: 0x000031B4 File Offset: 0x000013B4
            private void SetEditing(bool on)
            {
                if (this._caret == null)
                {
                    return;
                }
                if (this._caret.gameObject.activeSelf == on)
                {
                    return;
                }
                this._caret.gameObject.SetActive(on);
                this._blink = 0f;
                Input.imeCompositionMode = (on ? IMECompositionMode.On : IMECompositionMode.Auto);
            }

            // Token: 0x0600002F RID: 47 RVA: 0x0000320C File Offset: 0x0000140C
            private void Refresh()
            {
                if (this._text != null)
                {
                    this._text.SetText(this._value);
                }
                if (this._onChanged != null)
                {
                    this._onChanged(this._value);
                }
            }

            // Token: 0x06000030 RID: 48 RVA: 0x00003246 File Offset: 0x00001446
            private new void OnDisable()
            {
                Input.imeCompositionMode = IMECompositionMode.Auto;
            }

            // Token: 0x0400000F RID: 15
            private TMP_Text _text;

            // Token: 0x04000010 RID: 16
            private Action<string> _onChanged;

            // Token: 0x04000011 RID: 17
            private string _value = string.Empty;

            // Token: 0x04000012 RID: 18
            private RectTransform _rect;

            // Token: 0x04000013 RID: 19
            private RectTransform _caret;

            // Token: 0x04000014 RID: 20
            private Image _caretImage;

            // Token: 0x04000015 RID: 21
            private float _blink;
        }


        public class CarePackageOption
        {
            // Token: 0x0400001B RID: 27
            public CarePackageInfo info;

            // Token: 0x0400001C RID: 28
            public string displayName;

            // Token: 0x0400001D RID: 29
            public string quantity;

            // Token: 0x0400001E RID: 30
            public Sprite icon;
        }
    }
}
