using BepInEx.Configuration;
using Enemy;
using Fleece;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

namespace RunnerUtils.Components;

public class RUInputManager
{
    public struct DefaultBindingInfo(string identifier, Action action, string description = "", KeyCode key = KeyCode.None)
    {
        public Action action = action;
        public string identifier = identifier;
        public string description = description;
        public KeyCode key = key;
    }

    private Dictionary<ConfigEntry<KeyCode>, Action> bindings = [];

    // Bind the default config values to a specified config file
    public void BindToConfig(ConfigFile config) {
        foreach (var defaultBinding in DefaultBindings) {
            var configEntry = config.Bind(
                defaultBinding.key != KeyCode.None ? "Keybinds" : "Keybinds.Optional",
                defaultBinding.identifier,
                defaultBinding.key,
                defaultBinding.description
            );

            bindings[configEntry] = defaultBinding.action;
        }
    }

    public void Update() {
        if (GameManager.instance.levelController is null || GameManager.instance.levelController.IsLevelPaused()) return;
        foreach (var binding in bindings) {
            if (Input.GetKeyDown(binding.Key.Value)) { // lol
                binding.Value?.Invoke();
            }
        }
    }

    private static UISettingsRoot uiSettings;

    private static UISettingsSubMenu uiSettingsSubMenuCustom;
    // TODO would be cool to make this text yellow or something
    private static Jumper customSettingsTabMenuName = MakeWithText("RunnerUtils");

    private static int currentOffset = 0;

    private static GameObject MakeScrollable(GameObject uiElement)
    {
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
        ScrollRect scrollRect = scrollRectObj.AddComponent<ScrollRect>();
        //scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

        // Scrollbar
        var scrollbarGO = new GameObject("RunnerUtils Scrollbar", typeof(RectTransform), typeof(Scrollbar));
        scrollbarGO.transform.SetParent(scrollRectObj.transform, false);
        var sbRT = scrollbarGO.GetComponent<RectTransform>();

        sbRT.anchorMin = new Vector2(1, 0);
        sbRT.anchorMax = new Vector2(1, 1);
        sbRT.pivot = new Vector2(1, 1);

        sbRT.sizeDelta = new Vector2(20, 0);   // width = 20px
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

        var scrollbar = scrollbarGO.GetComponent<Scrollbar>();

        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.handleRect = handleRT;
        scrollbar.targetGraphic = handle.GetComponent<UnityEngine.UI.Image>();

        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;

        scrollRect.scrollSensitivity = 5f; // same as what level select uses
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;

        //scrollbarGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        //handle.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI_MilitaryImage");
        handle.GetComponent<Image>().color = new Color(1, 1, 1, 0.8f);

        Sprite militarySprite = Resources
            .FindObjectsOfTypeAll<Sprite>()                  // find all UI Images in memory
            .FirstOrDefault(img => img.name == "UI_MilitarySquare");

        if (militarySprite != null)
        {
            // assign it to your scrollbar
            //Image bgImage = scrollRect.verticalScrollbar.GetComponent<Image>();
            //bgImage.sprite = militarySprite;           // the Sprite, not Texture2D
            //bgImage.type = Image.Type.Sliced;          // IMPORTANT for 9-slice scaling
            //bgImage.preserveAspect = false;            // allow stretching
            //bgImage.color = Color.black;               // reset color if tinted

            Image handleImage = scrollRect.verticalScrollbar.handleRect.GetComponent<Image>();
            handleImage.sprite = militarySprite;
            handleImage.type = Image.Type.Sliced;          // IMPORTANT for 9-slice scaling
            handleImage.preserveAspect = false;            // allow stretching
            handleImage.pixelsPerUnitMultiplier = 3f;
        }
        else
        {
            Debug.LogWarning("UI_MilitarySquare not found in memory");
        }

        //var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(UnityEngine.UI.Mask), typeof(UnityEngine.UI.Image));
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

        // scroll to top immediately
        scrollRect.StartCoroutine(FixScrollOnNextFrame(scrollRect));
        IEnumerator FixScrollOnNextFrame(ScrollRect sr)
        {
            yield return null;
            sr.verticalNormalizedPosition = 1f;
        }

        // Setup our new content game object with our children and what have you
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

        return scrollContentRectObj;
    }

    public static Jumper MakeWithText(string text)
    {
        // not sure what is needed here
        // see https://kaiclavier.com/docs/Fleece.html#what-is-a-parser
        Passage customPassage = new();
        customPassage.id = Story.active.passages.Count + 100000 + currentOffset++; // trying to protect from collisions
        customPassage.text = text; // name in the tab
        Story.active.passages.Add(customPassage);

        Jumper j = new();
        j.passage = customPassage;
        return j;
    }

    public static UISettingsOptionToggle MakeToggleOption(Transform parent, Jumper text)
    {
        // using 0 (visual), 2 (windowed) as our prefab for a toggle
        GameObject attemptCountShowToggle = UnityEngine.Object.Instantiate(
            uiSettings.subMenus[0].transform.GetChild(2).gameObject,
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
            uiSettings.subMenus[5].transform.GetChild(0).gameObject,
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



        //return attemptCountShowToggle.GetComponent<UISettingsOptionToggle>();
    }

    [HarmonyPatch(typeof(UISettingsRoot), "Start")]
    public static class PatchUISettingsRootStart
    {
        [HarmonyPrefix]
        public static void Prefix(ref UISettingsRoot __instance)
        {
            Mod.Logger.LogInfo("attaching custom settings");
            uiSettings = __instance;

            // i'm using the visual settings as a prefab here, will rip it's guts out in Start
            GameObject listingAnchor = __instance.subMenus[0].gameObject.transform.parent.gameObject;
            var newMenu = UnityEngine.Object.Instantiate(__instance.subMenus[0].gameObject, listingAnchor.transform);
            newMenu.name = "RunnerUtils Settings";

            // add our custom menu
            uiSettingsSubMenuCustom = newMenu.AddComponent<UISettingsSubMenuCustom>();
            uiSettingsSubMenuCustom.menuName = customSettingsTabMenuName;

            // properly add the menu to the subMenus list and make it's sibling index correct (used for switching tabs)
            var originalLength = __instance.subMenus.Length;
            Array.Resize(ref __instance.subMenus, originalLength + 1);
            __instance.subMenus[originalLength] = uiSettingsSubMenuCustom;
            newMenu.transform.SetSiblingIndex(originalLength + 1);
        }
    }

    public class UISettingsSubMenuCustom : UISettingsSubMenu
    {
        private UISettingsOptionToggle showAttemptCount;

        private static Jumper attemptShowToggleText = MakeWithText("SHOW ATTEMPT COUNT");

        public override void Start()
        {
            base.Start();

            // clear out the children and component
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            Destroy(GetComponent<UISettingsSubMenuVisual>());

            // setup our settings
            showAttemptCount = MakeToggleOption(transform, attemptShowToggleText);
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            if (showAttemptCount != null)
                Mod.Logger.LogInfo(showAttemptCount.GetToggled());

            // TODO
        }
    }


    private static List<DefaultBindingInfo> DefaultBindings { get; } = [
        new(
            identifier: "Log Visibility Toggle",
            key: KeyCode.K,
            action: () => {
                Mod.Igl.ToggleVisibility();
                Mod.Igl.LogLine($"Toggled log visibility");
            }
        ),
        new(
            identifier: "Clear Log",
            key: KeyCode.J,
            action: () => {
                Mod.Igl.Clear();
                Mod.Igl.LogLine($"Cleared Log");
            }
        ),
        new(
            identifier: "Force Trigger Visibility On",
            key: KeyCode.O,
            action: () => {
                ShowTriggers.ShowAll();
                Mod.Igl.LogLine($"Enabled all triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            identifier: "Force Trigger Visibility Off",
            key: KeyCode.I,
            action: () => {
                ShowTriggers.HideAll();
                Mod.Igl.LogLine($"Disabled all triggers' visibility");
                FairPlay.triggersModified = false;
            }
        ),
        new(
            identifier: "Toggle Infinite Ammo",
            key: KeyCode.L,
            action: () => {
                if (!GameManager.instance.player.GetHUD()) return;
                InfiniteAmmo.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled infinite ammo");
            }
        ),
        new(
            identifier: "Toggle Infinite Health",
            key: KeyCode.Slash,
            action: () => {
                if (!GameManager.instance.player.GetHUD()) return;
                InfiniteHealth.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled infinite health");
            }
        ),
        new(
            identifier: "Toggle Throw Cam",
            key: KeyCode.Semicolon,
            action: () => {
                if (ThrowCam.cameraAvailable) {
                    ThrowCam.ToggleCam();
                    Mod.Igl.LogLine($"Toggled throw cam");
                } else {
                    Mod.Igl.LogLine($"Unable to switch to throw cam ~ no thrown weapons are in the air");
                }
            }
        ),
        new(
            identifier: "Toggle auto jump",
            key: KeyCode.M,
            action: () => {
                AutoJump.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled auto jump");
            }
        ),
        new(
            identifier: "Toggle magnetism overlay",
            key: KeyCode.Quote,
            action: () => {
                MagnetismOverlay.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled magnetism overlay");
            }
        ),
        new(
            identifier: "Toggle wave overlay",
            key: KeyCode.Backslash,
            action: () => {
                WaveOverlay.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled wave overlay");
            }
        ),
        new(
            identifier: "Toggle hard fall overlay",
            key: KeyCode.U,
            action: () => {
                HardFallOverlay.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled hf overlay");
            }
        ),
        new(
            identifier: "Toggle timestop",
            key: KeyCode.RightShift,
            action: () => {
                PauseTime.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled timestop");
            }
        ),
        new(
            identifier: "Save Location",
            key: KeyCode.LeftBracket,
            action: () => {
                LocationSave.SaveLocation();
                Mod.Igl.LogLine($"Saved location {(Mod.saveLocation_verbose.Value ? LocationSave.StringLoc : "")}");
            }
        ),
        new(
            identifier: "Load Location",
            key: KeyCode.RightBracket,
            action: () => {
                if (LocationSave.savedPosition is not null) {
                    LocationSave.RestoreLocation();
                    Mod.Igl.LogLine($"Loaded previous location {(Mod.saveLocation_verbose.Value ? LocationSave.StringLoc : "")}");
                } else {
                    Mod.Igl.LogLine("No location saved!");
                }
            }
        ),
        new(
            identifier: "Clear Location",
            key: KeyCode.P,
            action: () => {
                LocationSave.ClearLocation();
                Mod.Igl.LogLine($"Cleared saved location");
            }
        ),
        new(
            identifier: "Toggle view cones visibility",
            key: KeyCode.Y,
            action: () => {
                ViewCones.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled view cones' visibility");
            }
        ),

        // OPTIONAL SETTINGS

        new(
            identifier: "Trigger Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAll();
                Mod.Igl.LogLine($"Toggled all triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            identifier: "OOB Box Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<PlayerOutOfBoundsBox>();
                Mod.Igl.LogLine($"Toggled OOB boxes' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            identifier: "Start Trigger Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<PlayerTimerStartBox>();
                Mod.Igl.LogLine($"Toggled start triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            identifier: "Spawner Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<EnemySpawner>();
                Mod.Igl.LogLine($"Toggled spawners' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            identifier: "Toggle advanced movement info",
            action: () => {
                MovementDebug.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled movement info");
            }
        ),
    ];

    [HarmonyPatch(typeof(UISettingsSubMenuBindings), "Start")]
    public static class PatchUISettingsSubMenuBindings
    {
        [HarmonyPrefix]
        public static void Prefix(ref UISettingsSubMenuBindings __instance)
        {
            Mod.Logger.LogInfo("attaching custom binding settings");

            var content = MakeScrollable(__instance.gameObject);

            // this is enabled but it's children are disabled so it takes up space
            // but does nothing, so we just kill it
            UnityEngine.Object.Destroy(content.transform.Find("Reset Bindings").gameObject);

            MakeHeading(content.transform, "RunnerUtils rebinds", "These rebinds are for actions related to the RunnerUtils mod.\nFor other RunnerUtils settings see the RunnerUtils tab.");
            // TODO: display binds
        }
    }
}
