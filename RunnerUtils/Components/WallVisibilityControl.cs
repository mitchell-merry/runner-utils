using Enemy;
using HarmonyLib;
using UnityEngine;

namespace RunnerUtils.Components;

public class WallVisibilityControl : ComponentBase<WallVisibilityControl>
{
    public override string Identifier => "Walls Hidden";
    public override bool ShowOnFairPlay => true;

    public static void SetWallsVisible(bool visible) {
        if (visible) Instance.Disable();
        else Instance.Enable();
    }

    public override void Enable() {
        var changed = !enabled;
        base.Enable();
        Configs.WallsVisible = false;
        ApplyToCurrentCamera();
        if (changed && Mod.Igl.TextComponent) Mod.Igl.LogLine("Walls hidden");
    }

    public override void Disable() {
        var changed = enabled;
        base.Disable();
        Configs.WallsVisible = true;
        ApplyToCurrentCamera();
        if (changed && Mod.Igl.TextComponent) Mod.Igl.LogLine("Walls shown");
    }

    private static void ApplyToCurrentCamera() {
        var cameraManager = GameManager.instance ? GameManager.instance.cameraManager : null;
        ApplyToCamera(cameraManager ? cameraManager.GetManagersCamera() : null);
    }

    private static void ApplyToCamera(Camera camera) {
        if (!camera || !GameManager.instance) return;

        var geometryMask = GameManager.instance.layermasks.levelGeometry.value;
        camera.cullingMask = Configs.WallsVisible
            ? camera.cullingMask | geometryMask
            : camera.cullingMask & ~geometryMask;
    }

    [HarmonyPatch(typeof(CameraManager), "Start")]
    public static class CameraManagerPatch
    {
        [HarmonyPrefix]
        public static void SetVisibilityOnInit(ref Camera ___myCamera) {
            Instance.enabled = !Configs.WallsVisible;
            ApplyToCamera(___myCamera);
        }
    }

    [HarmonyPatch(typeof(EnemyHuman), "Start")]
    public static class EnemyHumanPatch
    {
        [HarmonyPrefix]
        public static void KeepEnemyMeshVisible(EnemyHuman __instance) {
            var enemyMesh = __instance.transform.Find("Animator/Enemy:Enemy_mesh");
            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyMesh && enemyLayer >= 0) enemyMesh.gameObject.layer = enemyLayer;
        }
    }
}