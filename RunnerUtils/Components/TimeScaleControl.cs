using HarmonyLib;
using RunnerUtils.UI;
using UnityEngine;

namespace RunnerUtils.Components;

public static class TimeScaleControl
{
    private static TimeScale m_timeScale;
    private static bool m_timeScaleInitialized;
    private static float m_lastLoggedScale = 1f;

    public static float CurrentScale => Configs.TimeScale;

    public static void SetScale(float scale, bool logChange = true) {
        scale = Mathf.Clamp01(scale);
        var changed = !Mathf.Approximately(Configs.TimeScale, scale);
        Configs.TimeScale = scale;
        if (m_timeScaleInitialized) m_timeScale.SetScale(scale);
        UISettingsSubMenuRunnerUtils.SetTimeScaleSlider(scale);
        if (changed && logChange) LogCurrentScaleChange();
    }

    public static void LogCurrentScaleChange() {
        var scale = CurrentScale;
        if (Mathf.Approximately(scale, m_lastLoggedScale)) return;
        m_lastLoggedScale = scale;
        if (Mod.Igl.TextComponent) {
            Mod.Igl.LogLine($"Timescale set to {scale:0.00}x");
        }
    }

    [HarmonyPatch(typeof(LevelController), nameof(LevelController.Initialize))]
    public static class LevelControllerPatch
    {
        [HarmonyPostfix]
        public static void InitializeScale() {
            m_timeScale = GameManager.instance.timeManager.CreateTimeScale(Configs.TimeScale);
            m_timeScaleInitialized = true;
        }
    }
}