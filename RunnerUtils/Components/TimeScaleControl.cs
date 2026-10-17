using HarmonyLib;
using RunnerUtils.UI;
using UnityEngine;

namespace RunnerUtils.Components;

public static class TimeScaleControl
{
    private static TimeScale m_timeScale;
    private static bool m_timeScaleInitialized;

    public static float CurrentScale => Configs.TimeScale;

    public static void SetScale(float scale) {
        scale = Mathf.Clamp01(scale);
        Configs.TimeScale = scale;
        if (m_timeScaleInitialized) m_timeScale.SetScale(scale);
        UISettingsSubMenuRunnerUtils.SetTimeScaleSlider(scale);
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