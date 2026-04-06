using System.Runtime.CompilerServices;
using CurvedUI;
using HarmonyLib;
using Progress;
using UnityEngine;

namespace RunnerUtils.Components;

public class AttemptCount
{
    static Fleece.Jumper attemptCountJumper = RUInputManager.MakeWithText("ATTEMPTS");
    static GameObject attemptAnchor = null;

    [HarmonyPatch(typeof(UILevelSelectFeature), "Refresh")]
    public static class PatchAttemptCount
    {
        static bool once = false;

        [HarmonyPrefix]
        public static void Postfix(ref UILevelSelectFeature __instance, ref SceneInformation info)
        {
            Mod.Logger.LogInfo("refreshing level select feature, postfix");

            var levelInfo = __instance.transform.GetChild(0).GetChild(1);
            if (!once)
            {
                levelInfo.GetChild(0).localScale = new Vector3(1, 1.15f, 1);
                levelInfo.SetLocalPositionAndRotation(new Vector3(levelInfo.localPosition.x, levelInfo.localPosition.y - 33f, levelInfo.localPosition.z), levelInfo.rotation);
                var one = levelInfo.GetChild(1);
                Mod.Logger.LogInfo(" b " + levelInfo.GetChild(1).localPosition.y);
                one.SetLocalPositionAndRotation(new Vector3(one.localPosition.x, one.localPosition.y + 30f, one.localPosition.z), one.rotation);
                Mod.Logger.LogInfo(" b " + levelInfo.GetChild(1).localPosition.y);

                var startButtonAnchor = levelInfo.GetChild(3);
                startButtonAnchor.SetLocalPositionAndRotation(new Vector3(startButtonAnchor.localPosition.x, startButtonAnchor.localPosition.y - 38f, startButtonAnchor.localPosition.z), startButtonAnchor.rotation);

                once = true;
            }

            var two = levelInfo.GetChild(2);
            Mod.Logger.LogInfo(" c " + levelInfo.GetChild(2).localPosition.y);
            two.SetLocalPositionAndRotation(new Vector3(two.localPosition.x, two.localPosition.y + 20f, two.localPosition.z), two.rotation);
            Mod.Logger.LogInfo(" c " + levelInfo.GetChild(2).localPosition.y);
            Mod.Logger.LogInfo(" a " + levelInfo.GetChild(2).localPosition.y);

            if (attemptAnchor != null)
            {
                GameObject.Destroy(attemptAnchor);
                attemptAnchor = null;
            }

            attemptAnchor = UnityEngine.Object.Instantiate(__instance.bestTimeAnchor, __instance.bestTimeAnchor.transform.parent);
            attemptAnchor.transform.SetSiblingIndex(1);
            __instance.bestTimeAnchor.transform.GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;

            //var layoutGroup = levelInfo.AddComponentIfMissing<UnityEngine.UI.VerticalLayoutGroup>();

            GameObject headerAttemptAnchor = attemptAnchor.transform.GetChild(0).gameObject;
            headerAttemptAnchor.GetComponent<FleeceTextSetter>().passage = attemptCountJumper;
            TMPro.TextMeshProUGUI headerAttemptTextGui = headerAttemptAnchor.GetComponent<TMPro.TextMeshProUGUI>();
            headerAttemptTextGui.fontSize = 24;
            headerAttemptTextGui.color = Color.white;

            LevelData levelData = null;
            if (info is LevelInformation)
            {
                levelData = GameManager.instance.progressManager.GetLevelData(info as LevelInformation);
            }

            if (levelData != null)
            {
                TMPro.TextMeshProUGUI textGui = attemptAnchor.transform.GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
                textGui.text = levelData.GetNumberOfAttempts().ToString();
                textGui.fontSize = 24;
                textGui.color = Color.white;
                textGui.alignment = TMPro.TextAlignmentOptions.Right;
            }
        }
    }
}