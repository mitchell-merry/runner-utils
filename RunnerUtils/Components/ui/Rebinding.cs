using System;
using System.Collections.Generic;
using System.Linq;
using static RunnerUtils.Components.RUInputManager;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;

namespace RunnerUtils.Components.UI
{
    internal class Rebinding
    {
        // F24 = unbound (nobody uses f24)
        public const string UNBOUND_KEY = "<Keyboard>/f24";

        // This is so that we are loaded into the action map before the game loads the bounds from settings
        // (AttemptApplyRebind does this)
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

        // Load the Bindings into the default action map
        public static void InitialiseCustomBindings(InputActionMap map)
        {
            map.Disable(); // cant add to map while active
            List<InputAction> actions = [];
            foreach (var bindingInfo in Bindings)
            {
                foreach (var ac in InitialiseCustomBinding(map, bindingInfo))
                {
                    actions.Add(ac);
                }
            }

            // Can only enable stuff once everything has been added
            actions.ForEach(action => action.Enable());
            map.Enable();
        }

        public static InputAction[] InitialiseCustomBinding(InputActionMap map, BindingInfo bindingInfo)
        {
            string baseName = $"RunnerUtils {bindingInfo.identifier}";
            string kbmName = $"{baseName} (kbm)";
            string gamepadName = $"{baseName} (gamepad)";

            InputAction kbm = map.actions.FirstOrDefault(act => act.name == kbmName);
            // I have never tested gamepad on this thing. no idea if it would work
            InputAction gamepad = map.actions.FirstOrDefault(act => act.name == gamepadName);

            // If we've already created them, just skip over
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
                    // TODO: maybe this can just be generated off a hash of identifier
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
                // you dont get to have the callback context
                kbm.performed += (_) => bindingInfo.action();
            }

            return [kbm, gamepad];
        }

        // This patch is so that our custom binds appear in the rebind menu
        [HarmonyPatch(typeof(UISettingsSubMenuBindings), "Start")]
        public static class PatchUISettingsSubMenuBindings
        {
            [HarmonyPrefix]
            public static void Prefix(ref UISettingsSubMenuBindings __instance)
            {
                Mod.Logger.LogInfo("Attaching custom binding settings");

                var content = Base.MakeScrollable(__instance.gameObject);

                // this is enabled but it's children are disabled so it takes up space
                // but does nothing, so we just kill it
                UnityEngine.Object.Destroy(content.transform.Find("Reset Bindings").gameObject);

                Base.MakeHeading(content.transform, "RunnerUtils rebinds", "These rebinds are for actions related to the RunnerUtils mod.\nFor other RunnerUtils settings see the RunnerUtils tab.");

                // add another header section (keyboard / gamepad)
                UnityEngine.Object.Instantiate(content.transform.Find("Headers").gameObject, content.transform);

                // use Jump as the prefab (move is composite and weird)
                var prefab = content.transform.Find("Rebind Jump").gameObject;
                foreach (var bindingInfo in Bindings)
                {
                    MakeRebind(content.transform, prefab, bindingInfo);
                }
            }
        }
        public static void MakeRebind(Transform parent, GameObject prefab, BindingInfo bindingInfo)
        {
            var newRebind = UnityEngine.Object.Instantiate(prefab, parent);
            newRebind.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = bindingInfo.identifier;
            var optionRebindComp = newRebind.GetComponent<UISettingsOptionRebind>();
            optionRebindComp.passageActionName = Text.MakeJumper(bindingInfo.identifier);
            optionRebindComp.actionDescription.text = bindingInfo.identifier;

            var map = GameManager.instance.inputManager.playerInput.actions.FindActionMap("Default Action Map");

            InputAction kbm = map.actions.FirstOrDefault(act => act.name == $"RunnerUtils {bindingInfo.identifier} (kbm)");
            InputAction gamepad = map.actions.FirstOrDefault(act => act.name == $"RunnerUtils {bindingInfo.identifier} (gamepad)");
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
                    "(or press DELETE to unbind)"; // this text is new too
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
                            Mod.Logger.LogInfo($"User is rebinding {action.name}, trying to see if delete ({operation.selectedControl.path})");
                            // Check if the key pressed is Delete
                            if (operation.selectedControl.path == "/Keyboard/delete")
                            {
                                Mod.Logger.LogInfo("Deleting bind");

                                // "Remove" the binding by overriding to f24
                                action.ApplyBindingOverride(num, UNBOUND_KEY);
                                operation.Cancel();
                                __instance.RebindComplete();
                            }
                        })
                        .OnComplete(operation =>
                        {
                            // otherwise do the rebind as normal
                            __instance.RebindComplete();
                        }).Start();
                }
                else
                {
                    // not supporting unbinding on controllers at the moment
                    // yell at someone if you want this supported
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

                return false; // dont run the actual function
            }
        }

        public static string GetBindingText(InputBinding binding)
        {
            // My genius knows no bounds
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
                        foreach (var binding in __instance.actions[buttonIdx].action.bindings)
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
}
