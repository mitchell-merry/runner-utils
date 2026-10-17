using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fleece;
using HarmonyLib;
using RunnerUtils.Components;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RunnerUtils.UI
{
    internal class Base
    {
        private static UISettingsRoot m_uiSettingsRootInstance;
        private static Sprite m_uiMilitarySquareSprite = 
            Resources
            .FindObjectsOfTypeAll<Sprite>()
            .FirstOrDefault(img => img.name == "UI_MilitarySquare");

        public struct CustomSettingTab(string name, Type type)
        {
            public readonly string name = name;
            public readonly Type type = type;
        }

        // just in case this is something we want extendable later
        public static List<CustomSettingTab> CustomTabs { get; } = [
            new(
                name: "RunnerUtils",
                type: typeof(UISettingsSubMenuRunnerUtils)
            )
        ];

        // Attaching our custom tabs
        [HarmonyPatch(typeof(UISettingsRoot), nameof(UISettingsRoot.Start))]
        public static class UISettingsRootPatch
        {
            [HarmonyPrefix]
            public static void AttachTabs(UISettingsRoot __instance) {
                Mod.Logger.LogInfo("Attaching custom settings");
                // grab the instance to be used elsewhere
                m_uiSettingsRootInstance = __instance;

                // attach all our custom menus
                CustomTabs.ForEach(customTabInfo =>
                {
                    if (!typeof(UISettingsSubMenu).IsAssignableFrom(customTabInfo.type)) {
                        throw new ArgumentException($"Settings submenu type \"{customTabInfo.type.FullName}\" must inherit from UISettingsSubMenu");
                    }

                    Mod.Logger.LogInfo($" => Attaching tab {customTabInfo.name}");

                    // i'm using the visual settings as a prefab here, will rip it's guts out in Start
                    var listingAnchor = __instance.subMenus[0].gameObject.transform.parent.gameObject;
                    var newMenu = Object.Instantiate(__instance.subMenus[0].gameObject, listingAnchor.transform);
                    newMenu.name = $"{customTabInfo.name} Settings";

                    // clear out the children and component
                    foreach (Transform child in newMenu.transform)
                    {
                        Object.Destroy(child.gameObject);
                    }
                    Object.Destroy(newMenu.GetComponent<UISettingsSubMenuVisual>());

                    // add our custom menu
                    var uiSettingsSubMenuCustom = (newMenu.AddComponent(customTabInfo.type) as UISettingsSubMenu)!;
                    // TODO would be cool to make this text yellow or something configurable
                    uiSettingsSubMenuCustom.menuName = FleeceUtil.MakeJumper(customTabInfo.name);

                    // properly add the menu to the subMenus list and make it's sibling index correct (used for switching tabs)
                    var originalLength = __instance.subMenus.Length;
                    Array.Resize(ref __instance.subMenus, originalLength + 1);
                    __instance.subMenus[originalLength] = uiSettingsSubMenuCustom;
                    newMenu.transform.SetSiblingIndex(originalLength + 1);
                });
            }
        }

        // Now for all the menu elements you can make
        public static UISettingsOptionToggle MakeToggleOption(Transform parent, Jumper text, bool initialValue) {
            // using 0 (visual), 2 (windowed) as our prefab for a toggle
            var attemptCountShowToggle = Object.Instantiate(
                m_uiSettingsRootInstance.subMenus[0].transform.GetChild(2).gameObject,
                parent
            );
            var textSetter = attemptCountShowToggle.transform.GetChild(0).gameObject.GetComponent<FleeceTextSetter>();
            textSetter.passage = text;

            var component = attemptCountShowToggle.GetComponent<UISettingsOptionToggle>();
            component.SetToggled(initialValue);

            return component;
        }

        public static SliderOption MakeSliderOption(Transform parent, Jumper text, float minValue, float maxValue, float initialValue, Action<float> onValueChanged) {
            var prefab = m_uiSettingsRootInstance.subMenus
                .SelectMany(subMenu => subMenu.GetComponentsInChildren<UISettingsOptionSlider>(true))
                .First();
            var sliderObject = Object.Instantiate(prefab.gameObject, parent);
            sliderObject.SetActive(true);
            sliderObject.name = "RunnerUtils Slider Option";
            sliderObject.GetComponentInChildren<FleeceTextSetter>(true).passage = text;

            var option = sliderObject.GetComponentInChildren<UISettingsOptionSlider>(true);
            var slider = sliderObject.GetComponentInChildren<Slider>(true);
            slider.onValueChanged.RemoveAllListeners();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.wholeNumbers = false;
            option.valueMultiplier = 1f;
            option.defaultSetting = initialValue;
            var sliderOption = new SliderOption(option, slider);
            sliderOption.SetValueWithoutNotify(initialValue);
            slider.onValueChanged.AddListener(value => {
                sliderOption.RefreshValue();
                onValueChanged?.Invoke(value);
            });

            return sliderOption;
        }

        public static GameObject MakeHeading(Transform parent, string text, string subtitle = null) {
            // using 5 (assist), 0 (disclaimer), 0 (Text (TMP) (1)) as our prefab for a heading
            var disclaimer = Object.Instantiate(
                m_uiSettingsRootInstance.subMenus[5].transform.GetChild(0).gameObject,
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
                Object.Destroy(subtitleComp.gameObject);
            }

            return disclaimer;
        }
        
        public static void MakeRebind(Transform parent, GameObject prefab, RUInputManager.BindingInfo bindingInfo)
        {
            var newRebind = Object.Instantiate(prefab, parent);
            newRebind.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = bindingInfo.identifier;
            var optionRebindComp = newRebind.GetComponent<UISettingsOptionRebind>();
            optionRebindComp.passageActionName = FleeceUtil.MakeJumper(bindingInfo.identifier);
            optionRebindComp.actionDescription.text = bindingInfo.identifier;

            var map = GameManager.instance.inputManager.playerInput.actions.FindActionMap("Default Action Map");

            var kbm = map.actions.FirstOrDefault(act => act.name == $"RunnerUtils {bindingInfo.identifier} (kbm)");
            var gamepad = map.actions.FirstOrDefault(act => act.name == $"RunnerUtils {bindingInfo.identifier} (gamepad)");
            if (kbm == null || gamepad == null)
            {
                // by this point these should have already been made
                throw new Exception($"InputAction(s) for \"{bindingInfo.identifier}\" were unexpectedly not found");
            }

            optionRebindComp.actions = [
                InputActionReference.Create(kbm),
                InputActionReference.Create(gamepad),
            ];
        }

        // To make a tab's content scrollable
        // Might need to change this later when you use it on other tabs, but to start I use this on Rebinding
        public static GameObject MakeMenuScrollable(UISettingsSubMenu menu) {
            // Idea here is to wrap the regular content (which contains the VerticalLayoutGroup) under a ScrollRect,
            //   which has a scrollbar + viewport (content goes in viewport)

            var oldLayout = menu.GetComponent<VerticalLayoutGroup>();
            oldLayout.enabled = false;

            // scroll rectum
            var scrollRectObj = new GameObject("RunnerUtils Scroll", typeof(RectTransform));
            scrollRectObj.transform.SetParent(menu.transform, false);

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
            if (m_uiMilitarySquareSprite)
            {
                var handleImage = handleRT.GetComponent<Image>();
                handleImage.sprite = m_uiMilitarySquareSprite;
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

            var scrollRect = scrollRectObj.AddComponent<ScrollRect>();
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
            var scrollContentRectObj = new GameObject("RunnerUtils Scroll Content", typeof(RectTransform));
            scrollContentRectObj.transform.SetParent(viewport.transform, false);

            var contentRT = scrollContentRectObj.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;
            contentRT.pivot = new Vector2(0, 1);

            scrollRect.viewport = viewportRT;
            scrollRect.content = contentRT;

            // scroll to top immediately (wait until next frame because it has to calculate layout)
            // otherwise, it starts in the midle
            scrollRect.StartCoroutine(FixScrollOnNextFrame());
            IEnumerator FixScrollOnNextFrame()
            {
                yield return null;
                scrollRect.verticalNormalizedPosition = 1f;
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
            newLayout.childAlignment = TextAnchor.UpperCenter;

            Object.Destroy(oldLayout); // Done with you

            var fitter = scrollContentRectObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Move children over
            var childrenCount = menu.transform.childCount;
            for (int i = 0; i < childrenCount; i++)
            {
                var child = (menu.transform.GetChild(0) as RectTransform)!;
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
