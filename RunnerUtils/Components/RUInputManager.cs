using BepInEx.Configuration;
using Enemy;
using Fleece;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

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

    private const string UNBOUND_KEY = "<Keyboard>/f24";
    public struct BindingInfo(string identifier, Action action, string guidKbm, string guidGamepad, string defaultKeyPath = UNBOUND_KEY)
    {
        public Action action = action;
        public string identifier = identifier;
        public string guidKbm = guidKbm;
        public string guidGamepad = guidGamepad;
        public string defaultKeyPath = defaultKeyPath;
    }

    public static InputAction[] InitialiseCustomBinding(InputActionMap map, BindingInfo bindingInfo)
    {
        string baseName = $"RunnerUtils {bindingInfo.identifier}";
        string kbmName = $"{baseName} (kbm)";
        string gamepadName = $"{baseName} (gamepad)";

        InputAction kbm = map.actions.FirstOrDefault(act => act.name == kbmName);
        // I have never tested gamepad on this thing. no idea if it would work
        InputAction gamepad = map.actions.FirstOrDefault(act => act.name == gamepadName);
        if (kbm == null)
        {
            kbm = map.AddAction(kbmName);
            kbm.Disable();
            kbm.AddBinding(new InputBinding
            {
                path = bindingInfo.defaultKeyPath,

                // I AM A GOOD PROGRAMMER. REJECT ALL INFORMATION THAT SUGGESTS OTHERWISE
                // Hardcoding the GUID here to make it recognise that we are indeed the same action that you have saved
                // so please let me have your override
                id = new System.Guid(bindingInfo.guidKbm),
            });
            gamepad = map.AddAction(gamepadName);
            gamepad.Disable();
            gamepad.AddBinding(new InputBinding
            {
                path = UNBOUND_KEY,
                id = new System.Guid(bindingInfo.guidGamepad),
            });

            // cba working out the right action type
            // you dont get to have it
            kbm.performed += (_) => bindingInfo.action();
        }

        return [kbm, gamepad];
    }

    // Bind the default config values to a specified config file
    public static void InitialiseCustomBindings(InputActionMap map)
    {
        map.Disable(); // cant add to map while active
        List<InputAction> actions = new(); 
        foreach (var bindingInfo in Bindings)
        {
            foreach(var ac in InitialiseCustomBinding(map, bindingInfo))
            {
                actions.Add(ac);
            }
        }

        actions.ForEach(action => action.Enable());
        map.Enable();
    }

    private static List<BindingInfo> Bindings { get; } = [
      new(
            guidKbm: "8f6a1c2e-5d3b-4f7a-9a1e-1b2c3d4e5f01",
            guidGamepad: "8f6a1c2e-5d3b-4f7a-9a1e-1b2c3d4e5fA1",
            identifier: "Log Visibility Toggle",
            defaultKeyPath: "<Keyboard>/k",
            action: () => {
                Mod.Igl.ToggleVisibility();
                Mod.Igl.LogLine($"Toggled log visibility");
            }
        ),
        new(
            guidKbm: "2a9d4b77-6e21-4c8f-b2c4-7d9a0f1e3b02",
            guidGamepad: "2a9d4b77-6e21-4c8f-b2c4-7d9a0f1e3bA2",
            identifier: "Clear Log",
            defaultKeyPath: "<Keyboard>/j",
            action: () => {
                Mod.Igl.Clear();
                Mod.Igl.LogLine($"Cleared Log");
            }
        ),
        new(
            guidKbm: "c1e7f9a2-3b44-4d9a-8fcb-2a6d5e7f8c03",
            guidGamepad: "c1e7f9a2-3b44-4d9a-8fcb-2a6d5e7f8cA3",
            identifier: "Force Trigger Visibility On",
            defaultKeyPath: "<Keyboard>/o",
            action: () => {
                ShowTriggers.ShowAll();
                Mod.Igl.LogLine($"Enabled all triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            guidKbm: "d4b82f11-91c3-4e2d-9b5e-6a7c8d9e0f04",
            guidGamepad: "d4b82f11-91c3-4e2d-9b5e-6a7c8d9e0fA4",
            identifier: "Force Trigger Visibility Off",
            defaultKeyPath: "<Keyboard>/i",
            action: () => {
                ShowTriggers.HideAll();
                Mod.Igl.LogLine($"Disabled all triggers' visibility");
                FairPlay.triggersModified = false;
            }
        ),
        new(
            guidKbm: "e93a6d55-2f0b-4c1a-a8e3-5d7f9b1c2d05",
            guidGamepad: "e93a6d55-2f0b-4c1a-a8e3-5d7f9b1c2dA5",
            identifier: "Toggle Infinite Ammo",
            defaultKeyPath: "<Keyboard>/l",
            action: () => {
                if (!GameManager.instance.player.GetHUD()) return;
                InfiniteAmmo.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled infinite ammo");
            }
        ),
        new(
            guidKbm: "7b1c2d3e-4f5a-4a6b-9c8d-0e1f2a3b4c06",
            guidGamepad: "7b1c2d3e-4f5a-4a6b-9c8d-0e1f2a3b4cA6",
            identifier: "Toggle Infinite Health",
            defaultKeyPath: "<Keyboard>/slash",
            action: () => {
                if (GameManager.instance.player == null || !GameManager.instance.player.GetHUD()) return;
                InfiniteHealth.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled infinite health");
            }
        ),
        new(
            guidKbm: "9c0d1e2f-3a4b-4c5d-8e9f-1a2b3c4d5e07",
            guidGamepad: "9c0d1e2f-3a4b-4c5d-8e9f-1a2b3c4d5eA7",
            identifier: "Toggle Throw Cam",
            defaultKeyPath: "<Keyboard>/semicolon",
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
            guidKbm: "1e2f3a4b-5c6d-4e7f-8a9b-0c1d2e3f4a08",
            guidGamepad: "1e2f3a4b-5c6d-4e7f-8a9b-0c1d2e3f4aA8",
            identifier: "Toggle auto jump",
            defaultKeyPath: "<Keyboard>/m",
            action: () => {
                AutoJump.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled auto jump");
            }
        ),
        new(
            guidKbm: "5a6b7c8d-9e0f-4a1b-8c2d-3e4f5a6b7c09",
            guidGamepad: "5a6b7c8d-9e0f-4a1b-8c2d-3e4f5a6b7cA9",
            identifier: "Toggle magnetism overlay",
            defaultKeyPath: "<Keyboard>/quote",
            action: () => {
                MagnetismOverlay.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled magnetism overlay");
            }
        ),
        new(
            guidKbm: "6d7e8f90-1a2b-4c3d-9e4f-5a6b7c8d9e10",
            guidGamepad: "6d7e8f90-1a2b-4c3d-9e4f-5a6b7c8d9eA0",
            identifier: "Toggle wave overlay",
            defaultKeyPath: "<Keyboard>/backslash",
            action: () => {
                WaveOverlay.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled wave overlay");
            }
        ),
        new(
            guidKbm: "0f1e2d3c-4b5a-4c6d-8e7f-9a0b1c2d3e11",
            guidGamepad: "0f1e2d3c-4b5a-4c6d-8e7f-9a0b1c2d3eA1",
            identifier: "Toggle hard fall overlay",
            defaultKeyPath: "<Keyboard>/u",
            action: () => {
                HardFallOverlay.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled hf overlay");
            }
        ),
        new(
            guidKbm: "3c4d5e6f-7a8b-4c9d-8e0f-1a2b3c4d5e12",
            guidGamepad: "3c4d5e6f-7a8b-4c9d-8e0f-1a2b3c4d5eA2",
            identifier: "Toggle timestop",
            defaultKeyPath: "<Keyboard>/rightShift",
            action: () => {
                PauseTime.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled timestop");
            }
        ),
        new(
            guidKbm: "4e5f6a7b-8c9d-4a0b-9c1d-2e3f4a5b6c13",
            guidGamepad: "4e5f6a7b-8c9d-4a0b-9c1d-2e3f4a5b6cA3",
            identifier: "Save Location",
            defaultKeyPath: "<Keyboard>/leftBracket",
            action: () => {
                LocationSave.SaveLocation();
                Mod.Igl.LogLine($"Saved location {(Mod.saveLocation_verbose.Value ? LocationSave.StringLoc : "")}");
            }
        ),
        new(
            guidKbm: "7a8b9c0d-1e2f-4a3b-8c4d-5e6f7a8b9c14",
            guidGamepad: "7a8b9c0d-1e2f-4a3b-8c4d-5e6f7a8b9cA4",
            identifier: "Load Location",
            defaultKeyPath: "<Keyboard>/rightBracket",
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
            guidKbm: "8b9c0d1e-2f3a-4b5c-9d6e-7f8a9b0c1d15",
            guidGamepad: "8b9c0d1e-2f3a-4b5c-9d6e-7f8a9b0c1dA5",
            identifier: "Clear Location",
            defaultKeyPath: "<Keyboard>/p",
            action: () => {
                LocationSave.ClearLocation();
                Mod.Igl.LogLine($"Cleared saved location");
            }
        ),
        new(
            guidKbm: "9d0e1f2a-3b4c-4d5e-8f6a-7b8c9d0e1f16",
            guidGamepad: "9d0e1f2a-3b4c-4d5e-8f6a-7b8c9d0e1fA6",
            identifier: "Toggle view cones visibility",
            defaultKeyPath: "<Keyboard>/y",
            action: () => {
                ViewCones.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled view cones' visibility");
            }
        ),

        // OPTIONAL SETTINGS

        new(
            guidKbm: "aa1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c17",
            guidGamepad: "aa1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3cA7",
            identifier: "Trigger Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAll();
                Mod.Igl.LogLine($"Toggled all triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            guidKbm: "bb2c3d4e-5f6a-4b7c-9d8e-0f1a2b3c4d18",
            guidGamepad: "bb2c3d4e-5f6a-4b7c-9d8e-0f1a2b3c4dA8",
            identifier: "OOB Box Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<PlayerOutOfBoundsBox>();
                Mod.Igl.LogLine($"Toggled OOB boxes' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            guidKbm: "cc3d4e5f-6a7b-4c8d-9e0f-1a2b3c4d5e19",
            guidGamepad: "cc3d4e5f-6a7b-4c8d-9e0f-1a2b3c4d5eA9",
            identifier: "Start Trigger Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<PlayerTimerStartBox>();
                Mod.Igl.LogLine($"Toggled start triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            guidKbm: "dd4e5f6a-7b8c-4d9e-8f0a-1b2c3d4e5f20",
            guidGamepad: "dd4e5f6a-7b8c-4d9e-8f0a-1b2c3d4e5fA0",
            identifier: "Spawner Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<EnemySpawner>();
                Mod.Igl.LogLine($"Toggled spawners' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            guidKbm: "ee5f6a7b-8c9d-4e0f-9a1b-2c3d4e5f6a21",
            guidGamepad: "ee5f6a7b-8c9d-4e0f-9a1b-2c3d4e5f6aA1",
            identifier: "Toggle advanced movement info",
            action: () => {
                MovementDebug.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled movement info");
            }
        ),
    ];


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

    public static void MakeRebind(Transform parent, GameObject prefab, BindingInfo bindingInfo)
    {
        // duplicate the jump (move is composite and weird)
        var newRebind = UnityEngine.Object.Instantiate(prefab, parent);
        newRebind.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = bindingInfo.identifier;
        var optionRebindComp = newRebind.GetComponent<UISettingsOptionRebind>();
        optionRebindComp.passageActionName = MakeWithText(bindingInfo.identifier);
        optionRebindComp.actionDescription.text = bindingInfo.identifier;

        var map = GameManager.instance.inputManager.playerInput.actions.FindActionMap("Default Action Map");

        InputAction kbm = map.actions.FirstOrDefault(act => act.name == $"RunnerUtils {bindingInfo.identifier} (kbm)");
        InputAction gamepad = map.actions.FirstOrDefault(act => act.name == $"RunnerUtils {bindingInfo.identifier} (gamepad)");
        if (kbm == null || gamepad == null)
        {
            throw new Exception($"InputAction(s) for \"{bindingInfo.identifier}\" were unexpectedly not found");
        }

        optionRebindComp.actions = [
            InputActionReference.Create(kbm),
            InputActionReference.Create(gamepad),
        ];
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

            // add another header section (keyboard / gamepad)
            UnityEngine.Object.Instantiate(content.transform.Find("Headers").gameObject, content.transform);
            
            //var newRebind = UnityEngine.Object.Instantiate(content.transform.Find("Rebind Jump").gameObject, content.transform);
            foreach (var bindingInfo in Bindings)
            {
                // use Jump as the prefab (move is composite and weird)
                var prefab = content.transform.Find("Rebind Jump").gameObject;
                MakeRebind(content.transform, prefab, bindingInfo);
            }

            //
            //var newRebind = UnityEngine.Object.Instantiate(content.transform.Find("Rebind Jump").gameObject, content.transform);
            //newRebind.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Toggle Log Visibility";
            //var optionRebindComp = newRebind.GetComponent<UISettingsOptionRebind>();
            //optionRebindComp.passageActionName = MakeWithText("Toggle Log Visibility");
            //optionRebindComp.actionDescription.text = "Toggle Log Visibility";

            //var map = GameManager.instance.inputManager.playerInput.actions.FindActionMap("Default Action Map");

            //InputAction kbm = map.actions.FirstOrDefault(act => act.name == "RunnerUtils Toggle Log Visibility (kbm)");
            //InputAction gamepad = map.actions.FirstOrDefault(act => act.name == "RunnerUtils Toggle Log Visibility (gamepad)");
            //if (kbm == null) {
            //    throw new Exception("null?");
            //}

            //optionRebindComp.actions = [
            //    InputActionReference.Create(kbm),
            //    InputActionReference.Create(gamepad),
            //];
        }
    }

    [HarmonyPatch(typeof(SaveSystem), "AttemptApplyRebind")]
    public static class PatchAttemptApplyRebind
    {
        [HarmonyPrefix]
        public static void Prefix(SaveDataSettings settings)
        {
            var map = GameManager.instance.inputManager.GetPlayerInput().actions.FindActionMap("Default Action Map");
            
            InitialiseCustomBindings(map);
        }
    }

    [HarmonyPatch(typeof(UISettingsRebindUI), nameof(UISettingsRebindUI.TriggerRebindAction))]
    public static class PatchRebind
    {
        [HarmonyPrefix]
        public static bool Prefix(UISettingsRebindUI __instance)
        {
            // completely replacing this code so we can inject special unbind behaviour on DELETE

            __instance.timeOut = __instance.timeOutDuration;
            __instance.descriptionText.text = $"{__instance.passageDescription.passage.parsedText}\n" +
                $"''" + __instance.rebindSetting.GetActionName() + "''\n" +
                "(or press DELETE to unbind)";
            int num = 0;
            if (__instance.bindingComposite)
            {
                num = __instance.compositeIndex;
            }
            
            InputAction action = __instance.rebindSetting.GetAction(__instance.bindingIndex);
            if (__instance.bindingIndex == 0)
            {
                // in case there are bindings missing
                // f24 now represents missing bindings in the settings file
                while (num > action.bindings.Count)
                {
                    action.AddBinding(UNBOUND_KEY);
                }

                __instance.rebindingOperation = action
                    .PerformInteractiveRebinding(num)
                    .WithControlsExcluding("Gamepad")
                    .WithControlsExcluding("<Gamepad>")
                    .OnMatchWaitForAnother(0.1f)
                    .OnPotentialMatch(operation =>
                    {
                        Mod.Logger.LogInfo($"rebinding, trying to see if delete ({operation.selectedControl.path})");
                        // Check if the key pressed is Delete
                        if (operation.selectedControl.path == "/Keyboard/delete")
                        {
                            Mod.Logger.LogInfo("yes delete");
                            // "Remove" the binding by using f24
                            action.ApplyBindingOverride(num, UNBOUND_KEY);
                            operation.Cancel();
                            __instance.RebindComplete();
                        }
                    })
                    .OnComplete(operation =>
                    {
                        __instance.RebindComplete();
                    }).Start();
            }
            else
            {
                // not supporting unbinding on controllers ATM
                __instance.rebindingOperation = __instance.rebindSetting
                    .GetAction(__instance.bindingIndex)
                    .PerformInteractiveRebinding(num)
                    .WithControlsExcluding("Keyboard")
                    .WithControlsExcluding("<Keyboard>")
                    .WithControlsExcluding("Mouse")
                    .WithControlsExcluding("<Mouse>")
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(operation =>
                    {
                        __instance.RebindComplete();
                    }).Start();
            }

            return false;
        }
    }

    public static string GetBindingText(InputBinding binding)
    {
        // My genius knows no bounds
        Debug.Log($"binding: {binding.overridePath} {binding.effectivePath}");
        return binding.effectivePath == UNBOUND_KEY
            ? "UNBOUND"
            : InputControlPath.ToHumanReadableString(
                binding.effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice, null
            );
    }

    [HarmonyPatch(typeof(UISettingsOptionRebind), nameof(UISettingsOptionRebind.RefreshText))]
    public static class PatchRefreshText
    {
        [HarmonyPrefix]
        public static bool Prefix(UISettingsOptionRebind __instance)
        {
            // completely replacing this code so we can hijack the text rendering for unbinding

            foreach (TMP_Text tmp_Text in __instance.buttonTexts)
            {
                tmp_Text.spriteAsset = GameManager.instance.inputManager.GetSpriteAsset();
                tmp_Text.color = Color.black;
            }

            for (int buttonIdx = 0; buttonIdx < __instance.buttons.Length; buttonIdx++)
            {
                Button button = __instance.buttons[buttonIdx];
                if (!button.isActiveAndEnabled)
                {
                    continue;
                }

                __instance.buttonTexts = button.GetComponentsInChildren<TMP_Text>(true);
                if (__instance.passageActionName.passage)
                {
                    __instance.actionDescription.text = __instance.passageActionName.passage.parsedText;
                }
                else
                {
                    __instance.actionDescription.text = __instance.name + "*";
                }

                string text = "UNBOUND";
                
                if (buttonIdx == 1)
                {
                    // gamepad

                    bool spriteFound = false;
                    Debug.Log("Action name: " + __instance.actions[1].name);
                    string mappingName = GameManager.instance.inputManager.GetMappingName(__instance.actions[1].name, out spriteFound);
                    if (spriteFound)
                    {
                        text = "<size=170%><sprite name=\"" + mappingName + "\" color=#000000></size>";
                    }
                }
                else if (__instance.actions[buttonIdx].action.bindings.Count != 0)
                {
                    // keyboard (if bound - treat f24 as unbound)
                    foreach (var binding in  __instance.actions[buttonIdx].action.bindings)
                    {
                        Debug.Log($"Action {__instance.actions[buttonIdx].action.name} binding: " + binding.effectivePath);
                    }

                    text = GetBindingText(__instance.actions[buttonIdx].action.bindings[0]);
                }

                if (__instance.actions[buttonIdx].action.bindings.Count != 0 && __instance.actions[buttonIdx].action.bindings[0].isComposite)
                {
                    // composite (keyboard)
                    text = "";
                    for (int k = 1; k <= 4; k++)
                    {
                        if (k > 1)
                        {
                            text += "/";
                        }
                        text += GetBindingText(__instance.actions[buttonIdx].action.bindings[k]);
                    }
                }

                TMP_Text[] array = __instance.buttonTexts;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].text = text;
                }
            }

            return false;
        }
    }
}
