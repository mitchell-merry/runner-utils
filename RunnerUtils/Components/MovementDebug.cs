using HarmonyLib;
using TMPro;
using UnityEngine;

namespace RunnerUtils.Components;

public class MovementDebug : ComponentBase<MovementDebug>
{
    public override string Identifier => "Advanced Movement Info";
    public override bool ShowOnFairPlay => false;

    private static InGameLog igl = new InGameLog($"{Mod.pluginName}~Movement Info", 10);

    public void Init() {
        igl.anchoredPos = new Vector2(-425, 535);
        igl.Setup();
        if (!enabled) igl.Hide();
    }

    public override void Enable() {
        base.Enable();
        igl.Show();
    }

    public override void Disable() {
        base.Disable();
        igl.Hide();
    }
    
    [HarmonyPatch(typeof(PlayerMovement), "Update")]
    public class LogMovement
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerMovement __instance, ref int ___dropOnEnemyFrames, ref int ___fallLevel, ref float ___coyoteTimer, ref float ___vMomentum) {
            if (!Instance.enabled) return;
            igl.SetBufferLine(0, $"Hard falling: {__instance.IsHardFalling()}");
            igl.SetBufferLine(1, $"Drop frames: {___dropOnEnemyFrames}");
            igl.SetBufferLine(2, $"Fall level: {___fallLevel}");
            igl.SetBufferLine(3, $"Coyote: {___coyoteTimer:0.00}");
            igl.SetBufferLine(4, $"vMomentum: {___vMomentum:0.00}");
            igl.FlushBuffer();
        }
    }
}