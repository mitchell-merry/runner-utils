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
    private static readonly Dictionary<PlayerClimbPointSurface, LineRenderer> predictedDirections = [];
    private static Material arrowMaterial;
    private static LineRenderer skidDirection;

    public override void Enable() {
        base.Enable();
        if (markers.Count == 0) RegisterAll();
        EnsureSkidDirection();
        SetVisible(true);
    }

    public override void Disable() {
        base.Disable();
        SetVisible(false);
    }

    public static void RegisterAll() {
        ClearMarkers();
        EnsureSkidDirection();
        foreach (var surface in Object.FindObjectsOfType<PlayerClimbPointSurface>(true)) {
            Register(surface);
        }
        SetVisible(Instance.enabled);
    }

    public static void ExtendRegistry() {
        EnsureSkidDirection();
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

        var arrowObject = new GameObject("RunnerUtils~ClimbDirection") {
            hideFlags = HideFlags.HideAndDontSave
        };
        arrowObject.transform.SetParent(surface.transform, worldPositionStays: true);
        var arrow = ConfigureArrow(arrowObject.AddComponent<LineRenderer>(), Color.cyan);
        arrow.enabled = Instance.enabled;

        UpdateMarkerPosition(surface, sphere, arrow);
        markers[surface] = sphere;
        predictedDirections[surface] = arrow;
    }

    private static void UpdateMarkerPosition(PlayerClimbPointSurface surface, GameObject marker, LineRenderer arrow) {
        if (!surface || !marker || !arrow) return;

        var landingPosition = surface.transform.position;
        marker.transform.position = landingPosition;

        if (!GameManager.instance || !GameManager.instance.player) {
            arrow.enabled = false;
            return;
        }

        var direction = landingPosition - GameManager.instance.player.GetPosition();
        direction.y = 0f;
        SetArrow(arrow, landingPosition + Vector3.up * 0.25f, direction);
    }

    private static LineRenderer ConfigureArrow(LineRenderer arrow, Color color) {
        if (!arrowMaterial) {
            arrowMaterial = new Material(Shader.Find("Hidden/Internal-Colored")) {
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = 5000
            };
            arrowMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            arrowMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            arrowMaterial.SetInt("_Cull", (int)CullMode.Off);
            arrowMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
            arrowMaterial.SetInt("_ZWrite", 0);
        }

        arrow.sharedMaterial = arrowMaterial;
        arrow.useWorldSpace = true;
        arrow.positionCount = 5;
        arrow.startWidth = 0.04f;
        arrow.endWidth = 0.04f;
        arrow.startColor = color;
        arrow.endColor = color;
        return arrow;
    }

    private static void EnsureSkidDirection() {
        if (skidDirection) return;

        var arrowObject = new GameObject("RunnerUtils~PostClimbSkidDirection") {
            hideFlags = HideFlags.HideAndDontSave
        };
        skidDirection = ConfigureArrow(arrowObject.AddComponent<LineRenderer>(), Color.yellow);
        skidDirection.enabled = Instance.enabled;
    }

    private static void SetArrow(LineRenderer arrow, Vector3 start, Vector3 direction) {
        if (direction.sqrMagnitude < 0.001f) {
            arrow.enabled = false;
            return;
        }

        direction.Normalize();
        var end = start + direction * 2.5f;
        var arrowSide = Vector3.Cross(direction, Vector3.up).normalized * 0.18f;
        var arrowBack = direction * 0.3f;
        arrow.SetPosition(0, start);
        arrow.SetPosition(1, end);
        arrow.SetPosition(2, end - arrowBack + arrowSide);
        arrow.SetPosition(3, end);
        arrow.SetPosition(4, end - arrowBack - arrowSide);
        arrow.enabled = Instance.enabled;
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
            if (predictedDirections.TryGetValue(kv.Key, out var arrow) && arrow) {
                arrow.enabled = visible;
                if (visible) UpdateMarkerPosition(kv.Key, kv.Value, arrow);
            }
        }
        if (skidDirection) skidDirection.enabled = visible;
        if (stale is null) return;
        foreach (var surface in stale) {
            markers.Remove(surface);
            if (predictedDirections.Remove(surface, out var arrow) && arrow) Object.Destroy(arrow.gameObject);
        }
    }

    private static void ClearMarkers() {
        foreach (var marker in markers.Values) {
            if (marker) Object.Destroy(marker);
        }
        foreach (var arrow in predictedDirections.Values) {
            if (arrow) Object.Destroy(arrow.gameObject);
        }
        markers.Clear();
        predictedDirections.Clear();
    }

    [HarmonyPatch(typeof(PlayerClimbPointSurface), "LateUpdate")]
    public class PlayerClimbPointSurfacePatch
    {
        [HarmonyPostfix]
        public static void UpdateLandingVisuals(PlayerClimbPointSurface __instance) {
            if (!Instance.enabled) return;
            if (markers.TryGetValue(__instance, out var marker) && predictedDirections.TryGetValue(__instance, out var arrow)) {
                UpdateMarkerPosition(__instance, marker, arrow);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.Update))]
    public class PlayerMovementPatch
    {
        [HarmonyPostfix]
        public static void FollowSkidDirection(PlayerMovement __instance, ref PlayerMovement.MoveState ___moveState) {
            if (!skidDirection) return;
            if (!Instance.enabled || ___moveState != PlayerMovement.MoveState.Skidding) {
                skidDirection.enabled = false;
                return;
            }

            SetArrow(skidDirection, __instance.transform.position + Vector3.up * 0.25f, __instance.GetSkidDirection());
        }
    }
}
