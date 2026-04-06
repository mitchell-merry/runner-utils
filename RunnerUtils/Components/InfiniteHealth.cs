using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace RunnerUtils.Components;

public class InfiniteHealth : ComponentBase<InfiniteHealth>
{
    public override string Identifier => "Infinite Heatlh";
    public override bool ShowOnFairPlay => true;

    [HarmonyPatch(typeof(DebugManager), nameof(DebugManager.GetInfiniteHealth))]
    public class PatchInfiniteHealth
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result) {
            if (Instance.enabled) {
                __result = true;
                return false;
            } else {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerHealthManager), nameof(PlayerHealthManager.Update))]
    public class PatchAlwaysMaxHealth
    {
        [HarmonyPrefix]
        public static void Postfix()
        {
            if (!Instance.enabled)
            {
                return;
            }

            var healthManager = GameManager.instance.player.GetHealthManager();
            healthManager.SetHealth(healthManager.maxHealth);
        }
    }

    [HarmonyPatch(typeof(PlayerHealthManager), nameof(PlayerHealthManager.GetBleeding))]
    public class PatchBleeding
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (Instance.enabled)
            {
                __result = false;
                return false;
            } else {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(HUDHealthHeartbeatMonitor), nameof(HUDHealthHeartbeatMonitor.FixedUpdate))]
    public class PatchHeartbeatMonitorUpdate
    {
        [HarmonyPostfix]
        public static void Postfix(HUDHealthHeartbeatMonitor __instance)
        {
            if (!Instance.enabled)
            {
                return;
            }


            __instance.line.color = Color.white;
            __instance.backingMain.color = Color.black;
            __instance.backingHaze.color = Color.white * 0.25f + Color.black * 0.75f;
        }
    }

    static Color originalBleedColor = Color.black;

    [HarmonyPatch(typeof(HUDHealthBleedingIndicator), nameof(HUDHealthBleedingIndicator.Update))]
    public class PatchHUDHealthBleedingIndicator
    {
        [HarmonyPostfix]
        public static void Postfix(HUDHealthBleedingIndicator __instance)
        {
            if (!Instance.enabled)
            {
                // if we have had the mod enabled yet
                if (originalBleedColor != Color.black)
                {
                    // this is usually always enabled, it's just the parent object that is disabled
                    // but we don't want an icon for this
                    __instance.mainAnchor.transform.Find("Image").gameObject.SetActive(true);
                    __instance.mainAnchor.transform.Find("Icon").gameObject.SetActive(true);
                    __instance.mainAnchor.transform.Find("Fill").gameObject.SetActive(true);
                    __instance.bleedingText.color = originalBleedColor;
                    // other two things we change are set by the original function
                }

                return;
            }

            if (originalBleedColor == Color.black)
            {
                originalBleedColor = __instance.bleedingText.color;
            }

            __instance.mainAnchor.SetActive(true);
            __instance.bleedingText.text = "INFINITE HEALTH";
            __instance.bleedingText.color = Color.white;
            __instance.mainAnchor.transform.Find("Image").gameObject.SetActive(false);
            __instance.mainAnchor.transform.Find("Icon").gameObject.SetActive(false);
            __instance.mainAnchor.transform.Find("Fill").gameObject.SetActive(false);
        }
    }
}