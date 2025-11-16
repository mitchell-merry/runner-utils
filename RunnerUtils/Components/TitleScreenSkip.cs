using HarmonyLib;
using Progress;

namespace RunnerUtils.Components;

public class TitleScreenSkip
{
    [HarmonyPatch(typeof(ProgressManager), "ShouldDisplayGameIntroOnStart")]
    public static class PatchTitleScreen
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            Mod.Logger.LogInfo("title screen: returning false");
            return false;
        }
    }
}