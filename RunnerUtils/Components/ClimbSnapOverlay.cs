using System.Collections.Generic;
using HarmonyLib;
using PlayerClimbing;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace RunnerUtils.Components;

public class ClimbSnapOverlay : ComponentBase<ClimbSnapOverlay>
{
    public override string Identifier => "Climb Snap Points";
    public override bool ShowOnFairPlay => true;

    private static readonly Dictionary<PlayerClimbPointSurface, GameObject> markers = [];

    public override void Enable() {
        base.Enable();
        if (markers.Count == 0) RegisterAll();
        SetVisible(true);
    }

    public override void Disable() {
        base.Disable();
        SetVisible(false);
    }

    public static void RegisterAll() {
        ClearMarkers();
        foreach (var surface in Object.FindObjectsOfType<PlayerClimbPointSurface>(true)) {
            Register(surface);
        }
        SetVisible(Instance.enabled);
    }

    public static void ExtendRegistry() {
        foreach (var surface in Object.FindObjectsOfType<PlayerClimbPointSurface>(true)) {
            if (!markers.ContainsKey(surface)) Register(surface);
        }
        SetVisible(Instance.enabled);
    }

    private static void Register(PlayerClimbPointSurface surface) {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.Destroy(sphere.GetComponent<Collider>());
        sphere.name = "RunnerUtils~ClimbSnap";
        sphere.transform.SetParent(surface.transform, worldPositionStays: true);
        sphere.transform.localScale = Vector3.one * 0.2f;

        var renderer = sphere.GetComponent<MeshRenderer>();
        renderer.material = ShowTriggers.mat;
        renderer.material.color = Color.white;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.enabled = Instance.enabled;

        UpdateMarkerPosition(surface, sphere);
        markers[surface] = sphere;
    }

    private static void UpdateMarkerPosition(PlayerClimbPointSurface surface, GameObject marker) {
        if (!surface || !marker) return;

        var pos = surface.interactionGoalPosition;
        if (pos == Vector3.zero) {
            pos = surface.GetPositionOnPerch(0.5f);
        }
        marker.transform.position = pos;
    }

    private static void SetVisible(bool visible) {
        List<PlayerClimbPointSurface> stale = null;
        foreach (var kv in markers) {
            if (!kv.Key || !kv.Value) {
                stale ??= [];
                stale.Add(kv.Key);
                if (kv.Value) Object.Destroy(kv.Value);
                continue;
            }
            kv.Value.GetComponent<MeshRenderer>().enabled = visible;
            if (visible) UpdateMarkerPosition(kv.Key, kv.Value);
        }
        if (stale is null) return;
        foreach (var surface in stale) markers.Remove(surface);
    }

    private static void ClearMarkers() {
        foreach (var marker in markers.Values) {
            if (marker) Object.Destroy(marker);
        }
        markers.Clear();
    }

    [HarmonyPatch(typeof(PlayerClimbPointSurface), "LateUpdate")]
    public class PlayerClimbPointSurfacePatch
    {
        [HarmonyPostfix]
        public static void FollowInteractionGoal(PlayerClimbPointSurface __instance) {
            if (!Instance.enabled) return;
            if (markers.TryGetValue(__instance, out var marker)) {
                UpdateMarkerPosition(__instance, marker);
            }
        }
    }
}
