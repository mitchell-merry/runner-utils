using UnityEngine;
using HarmonyLib;

namespace RunnerUtils.Components;

public class PauseTime : ComponentBase<PauseTime>
{
    public override string Identifier => "Timestop";
    public override bool ShowOnFairPlay => true;
    
    private static TimeScale m_timeScale;
    
    public override void Enable() {
        base.Enable();
        m_timeScale.SetScale(0f);
    }

    public override void Disable() {
        base.Disable();
        m_timeScale.SetScale(1f);
    }
    
    // TimeManager is most likely initialized once LevelController.Initialize is called
    // So it's reasonable to init timescales here
    [HarmonyPatch(typeof(LevelController))]
    public class LevelControllerPatch
    {
        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        public static void InitScale() {
            m_timeScale = GameManager.instance.timeManager.CreateTimeScale(Instance.enabled?0:1);
        }
    }
}