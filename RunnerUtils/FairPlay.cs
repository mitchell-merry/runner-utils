using System.Globalization;
using RunnerUtils.Components;
using UnityEngine;

namespace RunnerUtils;

public static class FairPlay
{
    private static InGameLog igl = new InGameLog($" ", 10);
    public static bool triggersModified = false;

    public static void Init() {
        igl.anchoredPos = new Vector2(800, 567);
        igl.sizeDelta = new Vector2(750, 250);
        igl.textAlignment = TMPro.TextAlignmentOptions.TopRight;
        igl.Setup();
    }

    public static void Update() {

        if (!igl.TextComponent) return;
        igl.Clear();
        foreach (var comp in ComponentBase.components) {
            if (!comp.ShowOnFairPlay || !comp.enabled) continue;
            igl.LogLine(comp.Identifier);
        }
        if (!Mathf.Approximately(TimeScaleControl.CurrentScale, 1f)) {
            igl.LogLine($"Time {TimeScaleControl.CurrentScale.ToString("0.##", CultureInfo.InvariantCulture)}x");
        }
        if (triggersModified) igl.LogLine("Triggers");
        if (LocationSave.savedPosition.HasValue || LocationSave.savedRotation.HasValue) igl.LogLine($"Location Saved{(Configs.SaveLocationVerboseEnabled ? $" ({LocationSave.StringLoc})":"")}");

    }
}