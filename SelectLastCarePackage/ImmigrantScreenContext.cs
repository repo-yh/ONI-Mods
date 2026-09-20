using HarmonyLib;
using KSerialization;
using SelectLastCarePackage.CarePackagePanel;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace laz_yh.SelectLastCarePackage
{
    public class ImmigrantScreenContext : KMonoBehaviour
    {
        public bool Skip { get; set; }
        [Serialize] public CarePackageInfo LastSelectedCarePackageInfo { get; set; }
    }

    public static class ImmigrantScreenMethod
    {


        public static void Reshuffle(CarePackageContainer container)
        {


            
            CharacterSelectionController controller = Traverse.Create(container).Field("controller").GetValue<CharacterSelectionController>();

            if (controller != null) // fix The original method Equals error
            {
                controller.RemoveLast();
            }
            Traverse.Create(container).Method("Reshuffle", new object[] { false }).GetValue();

        }



        public static void DeselectOtherDeliverable(CharacterSelectionController controller)
        {

            List<ITelepadDeliverable> selectedDeliverables = Traverse.Create(controller).Field("selectedDeliverables").GetValue<List<ITelepadDeliverable>>();
            List<ITelepadDeliverableContainer> containers = Traverse.Create(controller).Field("containers").GetValue<List<ITelepadDeliverableContainer>>();

            if (controller != null && containers != null && selectedDeliverables != null && selectedDeliverables.Count > 0)
            {


                foreach (var item in containers)
                {

                    if (item is CharacterContainer characterContainer)

                    {

                        if (selectedDeliverables.Contains(characterContainer.Stats))
                        {

                            characterContainer.DeselectDeliverable();
                            break;


                        }


                    }
                    else if (item is CarePackageContainer carePackageContainer)
                    {
                        //global::Debug.Log("CarePackageContainer："+ carePackageContainer.Info.id);

                        if (selectedDeliverables.Contains(carePackageContainer.carePackageInstanceData))
                        {

                            carePackageContainer.DeselectDeliverable();
                            break;

                        }
                    }
                


                }

            }
            else {

                global::Debug.Log("其他containers没有找到被选中的");

            }


        }



        public static void ShowButton(ImmigrantScreen __instance)
        {
            if (!ModLoader.Patches2) { return; }
            List<ITelepadDeliverableContainer> deliverableContainerList = Traverse.Create(__instance).Field("containers").GetValue<List<ITelepadDeliverableContainer>>();
            if (deliverableContainerList != null)
            {
                deliverableContainerList.ForEach(c =>
                {
                    if (c is CharacterContainer characterContainer)
                    {
                        characterContainer.SetReshufflingState(true);
                        if (Game.IsDlcActiveForCurrentSave("DLC3_ID"))
                    {
                            DropDown modelDropDown = Traverse.Create(characterContainer).Field("modelDropDown").GetValue<DropDown>();

                            Canvas canvas1 = modelDropDown.transform.Find("ScrollRect")?.GetComponent<Canvas>();
                            Canvas canvas2 = __instance.GetComponent<Canvas>();

                            if (canvas1 != null && canvas2 != null)
                            {
                                canvas1.sortingOrder = canvas2.sortingOrder + 1;
                            }
                            else {
                                modelDropDown.transform.parent.gameObject.SetActive(false);
                            }
                           

                            return;
                     }

                    }

                    else if (c is CarePackageContainer carePackageContainer)
                    {

                        // carePackageContainer.SetReshufflingState(true);
                        KButton reshuffleButton = Traverse.Create(carePackageContainer).Field("reshuffleButton").GetValue<KButton>();
                        //Traverse.Create(carePackageContainer).Field("reshuffleButton").Field("onClick").SetValue(new System.Action(delegate
                        //{

                        //    ImmigrantScreenMethod.Reshuffle(carePackageContainer);

                        //}));

                        KButton kbutton = Util.KInstantiateUI<KButton>(reshuffleButton.gameObject, reshuffleButton.transform.parent.gameObject, true);
                        kbutton.rectTransform().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 200f, 140f);

                        LocText text = kbutton.transform.GetComponentInChildren<LocText>();
                        if (text != null)
                        {
                            if (!text.enabled)
                            {
                                text.enabled = true;
                            }
                            if (!text.gameObject.activeSelf)
                            {
                                text.gameObject.SetActive(true);
                            }
                            text.key = string.Empty;

                            text.alignment = TextAlignmentOptions.MidlineLeft;

                            // 文字较长：禁用换行，在文本框内自动缩放字号，保证单行不超出按钮
                            text.textWrappingMode = TextWrappingModes.NoWrap;
                            text.enableAutoSizing = true;
                            text.fontSizeMin = 10f;
                            text.fontSizeMax = 14f;
                            text.SetText(Languages.MANUALSELECTCAREPACKAGE);
                        }

                        kbutton.onClick += delegate ()
                        {
                            CarePackagePanel.Open(__instance, carePackageContainer);
                        };
                    }




                }

            );

                //    Debug.Log("刷新按钮生成");
            }




        }



    }



}