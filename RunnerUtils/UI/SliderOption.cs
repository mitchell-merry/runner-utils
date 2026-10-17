using System.Collections.Generic;
using HarmonyLib;
using UnityEngine.UI;

namespace RunnerUtils.UI;

internal sealed class SliderOption
{
    private static readonly HashSet<UISettingsOptionSlider> options = [];
    private readonly UISettingsOptionSlider m_option;
    private readonly Slider m_slider;

    public SliderOption(UISettingsOptionSlider option, Slider slider) {
        m_option = option;
        m_slider = slider;
        options.Add(option);
    }

    public float Value => m_option.GetValue();

    public void RefreshValue() {
        m_option.valueText.text = Value.ToString("F2");
    }

    public void SetValueWithoutNotify(float value) {
        m_slider.SetValueWithoutNotify(value);
        RefreshValue();
    }

    [HarmonyPatch(typeof(UISettingsOptionSlider), nameof(UISettingsOptionSlider.RefreshValue))]
    public static class RefreshValuePatch
    {
        [HarmonyPostfix]
        public static void FormatReadout(UISettingsOptionSlider __instance) {
            if (options.Contains(__instance)) {
                __instance.valueText.text = __instance.GetValue().ToString("F2");
            }
        }
    }
}