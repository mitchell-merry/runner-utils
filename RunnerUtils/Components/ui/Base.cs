using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Fleece;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace RunnerUtils.Components.UI
{
    internal class Base
    {
        private static UISettingsRoot uiSettingsRootInstance;

        public struct CustomSettingTab(string tabName, Type classType)
        {
            public string tabName = tabName;
            public Type classType = classType;
        }

        // just in case this is something we want extendable later
        public static List<CustomSettingTab> CustomTabs { get; } = [
            new() {
                tabName = "RunnerUtils",
                classType = typeof(RunnerUtilsSettings.UISettingsSubMenuCustom)
            }
        ];

        // Attaching our custom tabs
        [HarmonyPatch(typeof(UISettingsRoot), "Start")]
        public static class PatchUISettingsRootStart
        {
            [HarmonyPrefix]
            public static void Prefix(UISettingsRoot __instance)
            {
                Mod.Logger.LogInfo("Attaching custom settings");
                // grab the instance to be used elsewhere
                uiSettingsRootInstance = __instance;

                // attach all our custom menus
                CustomTabs.ForEach(customTabInfo =>
                {
                    if (!typeof(UISettingsSubMenu).IsAssignableFrom(customTabInfo.classType))
                        throw new ArgumentException("Settings submenu type must inherit from UISettingsSubMenu");
                    Mod.Logger.LogInfo($" => Attaching tab {customTabInfo.tabName}");

                    // i'm using the visual settings as a prefab here, will rip it's guts out in Start
                    GameObject listingAnchor = __instance.subMenus[0].gameObject.transform.parent.gameObject;
                    var newMenu = UnityEngine.Object.Instantiate(__instance.subMenus[0].gameObject, listingAnchor.transform);
                    newMenu.name = $"{customTabInfo.tabName} Settings";

                    // add our custom menu
                    var uiSettingsSubMenuCustom = newMenu.AddComponent(customTabInfo.classType) as UISettingsSubMenu;
                    // TODO would be cool to make this text yellow or something configurable
                    uiSettingsSubMenuCustom.menuName = Text.MakeJumper(customTabInfo.tabName);

                    // properly add the menu to the subMenus list and make it's sibling index correct (used for switching tabs)
                    var originalLength = __instance.subMenus.Length;
                    Array.Resize(ref __instance.subMenus, originalLength + 1);
                    __instance.subMenus[originalLength] = uiSettingsSubMenuCustom;
                    newMenu.transform.SetSiblingIndex(originalLength + 1);
                });
            }
        }

        // Now for all the menu elements you can make
        public static UISettingsOptionToggle MakeToggleOption(Transform parent, Jumper text)
        {
            // using 0 (visual), 2 (windowed) as our prefab for a toggle
            GameObject attemptCountShowToggle = UnityEngine.Object.Instantiate(
                uiSettingsRootInstance.subMenus[0].transform.GetChild(2).gameObject,
                parent
            );
            var textSetter = attemptCountShowToggle.transform.GetChild(0).gameObject.GetComponent<FleeceTextSetter>();
            textSetter.passage = text;

            return attemptCountShowToggle.GetComponent<UISettingsOptionToggle>();
        }

        public static void MakeHeading(Transform parent, string text, string subtitle = null)
        {
            // using 5 (assist), 0 (disclaimer), 0 (Text (TMP) (1)) as our prefab for a heading
            GameObject disclaimer = UnityEngine.Object.Instantiate(
                uiSettingsRootInstance.subMenus[5].transform.GetChild(0).gameObject,
                parent
            );
            disclaimer.transform.name = text;

            var vlg = disclaimer.AddComponent<VerticalLayoutGroup>();
            vlg.padding.left = 16;
            vlg.padding.top = 30;
            vlg.CalculateLayoutInputHorizontal();
            vlg.CalculateLayoutInputVertical();

            var fitter = disclaimer.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var headingTransform = disclaimer.transform.GetChild(0);
            headingTransform.name = "RunnerUtils Heading text";
            var headingText = headingTransform.gameObject.GetComponent<TextMeshProUGUI>();
            headingText.text = text;

            var subtitleComp = disclaimer.transform.GetChild(1);
            if (subtitle != null)
            {
                subtitleComp.name = "RunnerUtils Subtitle text";
                subtitleComp.gameObject.GetComponent<TextMeshProUGUI>().text = subtitle;
            }
            else
            {
                UnityEngine.Object.Destroy(subtitleComp.gameObject);
            }
        }

        // To make a tab's content scrollable
        // Might need to change this later when you use it on other tabs, but to start I use this on Rebinding
        public static GameObject MakeScrollable(GameObject uiElement)
        {
            // Idea here is to wrap the regular content (which contains the VerticalLayoutGroup) under a ScrollRect,
            //   which has a scrollbar + viewport (content goes in viewport)

            var oldLayout = uiElement.GetComponent<VerticalLayoutGroup>();
            oldLayout.enabled = false;

            // scroll rectum
            GameObject scrollRectObj = new GameObject("RunnerUtils Scroll", typeof(RectTransform));
            scrollRectObj.transform.SetParent(uiElement.transform, false);

            var scrollRT = scrollRectObj.GetComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            // this autistic tweaking is to make sure the scrollbar isn't pressed weirdly against the edges. more Aesthetically pleasing.
            scrollRT.offsetMin = new Vector2(0, 3f);
            scrollRT.offsetMax = new Vector2(-2, 0f);

            // Scrollbar
            var scrollbarGO = new GameObject("RunnerUtils Scrollbar", typeof(RectTransform), typeof(Scrollbar));
            scrollbarGO.transform.SetParent(scrollRectObj.transform, false);

            var sbRT = scrollbarGO.GetComponent<RectTransform>();
            sbRT.anchorMin = new Vector2(1, 0);
            sbRT.anchorMax = new Vector2(1, 1);
            sbRT.pivot = new Vector2(1, 1);
            sbRT.sizeDelta = new Vector2(20, 0); // 20px width
            sbRT.anchoredPosition = Vector2.zero;

            var slidingArea = new GameObject("RunnerUtils Sliding Area", typeof(RectTransform));
            slidingArea.transform.SetParent(scrollbarGO.transform, false);

            var saRT = slidingArea.GetComponent<RectTransform>();
            saRT.anchorMin = Vector2.zero;
            saRT.anchorMax = Vector2.one;
            saRT.offsetMin = Vector2.zero;
            saRT.offsetMax = Vector2.zero;

            var handle = new GameObject("RunnerUtils Scroll Handle", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            handle.transform.SetParent(slidingArea.transform, false);

            var handleRT = handle.GetComponent<RectTransform>();
            handleRT.anchorMin = Vector2.zero;
            handleRT.anchorMax = Vector2.one;
            handleRT.offsetMin = Vector2.zero;
            handleRT.offsetMax = Vector2.zero;

            // UI military square is the image that's stretched and use in basically every UI element
            Sprite militarySprite = Resources
                .FindObjectsOfTypeAll<Sprite>()
                .FirstOrDefault(img => img.name == "UI_MilitarySquare");

            if (militarySprite != null)
            {
                Image handleImage = handleRT.GetComponent<Image>();
                handleImage.sprite = militarySprite;
                handleImage.type = Image.Type.Sliced;
                handleImage.preserveAspect = false;
                handleImage.pixelsPerUnitMultiplier = 3f;
            }
            else
            {
                Debug.LogWarning("UI_MilitarySquare not found in memory");
            }

            var scrollbar = scrollbarGO.GetComponent<Scrollbar>();

            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.handleRect = handleRT;
            scrollbar.targetGraphic = handle.GetComponent<UnityEngine.UI.Image>();

            ScrollRect scrollRect = scrollRectObj.AddComponent<ScrollRect>();
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            scrollRect.scrollSensitivity = 5f; // same as what level select uses
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;

            var viewport = new GameObject("RunnerUtils Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(scrollRectObj.transform, false);

            var viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = new Vector2(-30, 0); // match scrollbar width

            var img = viewport.gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            img.raycastTarget = true;

            // setup content with layout group and content size filter
            GameObject scrollContentRectObj = new GameObject("RunnerUtils Scroll Content", typeof(RectTransform));
            scrollContentRectObj.transform.SetParent(viewport.transform, false);

            var contentRT = scrollContentRectObj.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            scrollRect.viewport = viewportRT;
            scrollRect.content = contentRT;

            // scroll to top immediately (wait until next frame because it has to calculate layout)
            // otherwise, it starts in the midle
            scrollRect.StartCoroutine(FixScrollOnNextFrame(scrollRect));
            IEnumerator FixScrollOnNextFrame(ScrollRect sr)
            {
                yield return null;
                sr.verticalNormalizedPosition = 1f;
            }

            // Setup our new content game object with our children and what have you
            // move over the vertical layout group as it was
            var newLayout = scrollContentRectObj.AddComponent<VerticalLayoutGroup>();
            newLayout.padding = oldLayout.padding;
            newLayout.spacing = oldLayout.spacing;
            newLayout.childControlWidth = oldLayout.childControlWidth;
            newLayout.childControlHeight = oldLayout.childControlHeight;
            newLayout.childForceExpandWidth = oldLayout.childForceExpandWidth;
            newLayout.childForceExpandHeight = oldLayout.childForceExpandHeight;

            GameObject.Destroy(oldLayout); // Done with you

            var fitter = scrollContentRectObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Move children over
            var childrenCount = uiElement.transform.childCount;
            for (int i = 0; i < childrenCount; i++)
            {
                RectTransform child = uiElement.transform.GetChild(0) as RectTransform;
                Vector2 anchoredPos = child.anchoredPosition;
                Vector2 anchorMin = child.anchorMin;
                Vector2 anchorMax = child.anchorMax;
                Vector2 pivot = child.pivot;
                Vector2 sizeDelta = child.sizeDelta;

                child.SetParent(scrollContentRectObj.transform, false);

                child.anchorMin = anchorMin;
                child.anchorMax = anchorMax;
                child.pivot = pivot;
                child.sizeDelta = sizeDelta;
                child.anchoredPosition = anchoredPos;
            }
            
            // Give the content so the user can change up what's in
            return scrollContentRectObj;
        }
    }
}
