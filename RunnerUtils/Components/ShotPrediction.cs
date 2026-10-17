using AimAssist;
using Enemy;
using HarmonyLib;
using Projectiles;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace RunnerUtils.Components;

public class ShotPrediction : ComponentBase<ShotPrediction>
{
    private const int PhysicalPredictionSteps = 40;
    private const float PhysicalPredictionStep = 0.04f;
    private static Material impactMaterial;
    private static GameObject impactMarker;

    public override string Identifier => "Shot Prediction";
    public override bool ShowOnFairPlay => true;

    public override void Enable() {
        base.Enable();
        EnsureVisuals();
    }

    public override void Disable() {
        base.Disable();
        HideVisuals();
    }

    private static void EnsureVisuals() {
        if (!impactMaterial) {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (!shader) return;

            impactMaterial = CreateOverlayMaterial(shader);
        }

        if (!impactMarker) {
            impactMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(impactMarker.GetComponent<Collider>());
            impactMarker.name = "RunnerUtils~ShotPredictionImpact";
            impactMarker.hideFlags = HideFlags.HideAndDontSave;
            impactMarker.transform.localScale = Vector3.one * 0.18f;
            var renderer = impactMarker.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = impactMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            impactMarker.SetActive(false);
        }
    }

    private static Material CreateOverlayMaterial(Shader shader) {
        var material = new Material(shader) {
            hideFlags = HideFlags.HideAndDontSave,
            renderQueue = 5000
        };
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_Cull", (int)CullMode.Off);
        material.SetInt("_ZTest", (int)CompareFunction.Always);
        material.SetInt("_ZWrite", 0);
        return material;
    }

    private static void UpdatePrediction(PlayerAimAssistManager aimManager) {
        if (!Instance.enabled) {
            HideVisuals();
            return;
        }

        EnsureVisuals();
        if (!impactMarker || !GameManager.instance || !GameManager.instance.player) {
            HideVisuals();
            return;
        }

        var player = GameManager.instance.player;
        var armSet = player.GetArmScript().GetCurrentArms();
        var weapon = armSet ? armSet.GetComponentInChildren<PlayerGunArmed>(true) : null;
        var prefab = weapon ? weapon.GetProjectilePrefab() : null;
        var projectile = prefab ? prefab.GetComponent<Projectile>() : null;
        if (!projectile) {
            HideVisuals();
            return;
        }

        var origin = player.GetLookScript().transform.position;
        var rotation = GetShotRotation(aimManager, origin);
        var direction = rotation * Vector3.forward;
        var layerMask = GameManager.instance.layermasks.playerProjectileColliders;

        if (projectile is HitscanProjectile hitscan) {
            PredictHitscan(origin, direction, hitscan.range, layerMask);
        } else if (projectile is SpherecastProjectile spherecast) {
            PredictSpherecast(origin, direction, spherecast, layerMask);
        } else if (projectile is PhysicalProjectile physical) {
            PredictPhysical(origin, rotation, direction, prefab, physical, layerMask);
        } else {
            HideVisuals();
        }
    }

    private static Quaternion GetShotRotation(PlayerAimAssistManager aimManager, Vector3 origin) {
        var cameraManager = GameManager.instance.cameraManager;
        var target = aimManager.GetHighlightedTarget();
        var rotation = cameraManager.GetForwardAnchor().rotation;
        if (!target) return rotation;

        var targetRotation = Quaternion.LookRotation(target.GetCenter() - origin, Vector3.up);
        if (aimManager.ShouldUseAutoAim()) rotation = targetRotation;

        var angle = Quaternion.Angle(cameraManager.GetAimAnchor().rotation, targetRotation);
        var range = aimManager.GetMagnetismRange();
        var magnetism = range > 0f ? 1f - Mathf.Clamp01(angle / range) : 0f;
        magnetism = 1f - Mathf.Pow(1f - magnetism, 2f);
        return Quaternion.Lerp(rotation, targetRotation, magnetism);
    }

    private static void PredictHitscan(Vector3 origin, Vector3 direction, float range, LayerMask layerMask) {
        var hitSomething = Physics.Raycast(origin, direction, out var hit, range, layerMask, QueryTriggerInteraction.Collide);
        if (hitSomething) ShowImpact(hit);
        else HideImpact();
    }

    private static void PredictSpherecast(Vector3 origin, Vector3 direction, SpherecastProjectile projectile, LayerMask layerMask) {
        var distance = projectile.range;
        if (Physics.Raycast(origin, direction, out var geometryHit, projectile.range, GameManager.instance.layermasks.levelGeometry, QueryTriggerInteraction.Ignore)) {
            distance = geometryHit.distance;
        }

        var hits = Physics.SphereCastAll(origin, projectile.radius, direction, distance, layerMask, QueryTriggerInteraction.Collide);
        var closestHitDistance = float.MaxValue;
        var hasHit = false;
        var closestHit = default(RaycastHit);
        foreach (var hit in hits) {
            if (hit.distance >= closestHitDistance) continue;
            closestHitDistance = hit.distance;
            closestHit = hit;
            hasHit = true;
        }

        if (hasHit) ShowImpact(closestHit);
        else HideImpact();
    }

    private static void PredictPhysical(Vector3 origin, Quaternion rotation, Vector3 direction, GameObject prefab, PhysicalProjectile projectile, LayerMask layerMask) {
        var start = origin + rotation * Vector3.Scale(prefab.transform.localScale, new Vector3(0.25f, 0f, 1f)) * 0.65f;
        var velocity = direction * projectile.forwardVelocity;
        var rigidbody = prefab.GetComponent<Rigidbody>();
        var gravity = rigidbody && rigidbody.useGravity ? Physics.gravity : Vector3.zero;
        var collider = prefab.GetComponent<Collider>();
        var radius = collider ? Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.y, collider.bounds.extents.z) : 0f;
        var position = start;

        for (var i = 1; i <= PhysicalPredictionSteps; i++) {
            velocity += gravity * PhysicalPredictionStep;
            var next = position + velocity * PhysicalPredictionStep;
            var segment = next - position;
            if (Physics.SphereCast(position, radius, segment.normalized, out var hit, segment.magnitude, layerMask, QueryTriggerInteraction.Ignore)) {
                ShowImpact(hit);
                return;
            }

            position = next;
        }

        HideImpact();
    }

    private static void ShowImpact(RaycastHit hit) {
        if (!impactMarker) return;
        impactMarker.transform.position = hit.point;
        var receiver = hit.collider.GetComponentInParent<DamageReceiver>();
        var isEnemy = receiver && receiver.GetEnemy();
        impactMaterial.SetColor("_Color", isEnemy ? Color.red : Color.yellow);
        impactMarker.SetActive(true);
    }

    private static void HideImpact() {
        if (impactMarker) impactMarker.SetActive(false);
    }

    private static void HideVisuals() {
        HideImpact();
    }

    [HarmonyPatch(typeof(PlayerAimAssistManager), nameof(PlayerAimAssistManager.Update))]
    public static class PlayerAimAssistManagerPatch
    {
        [HarmonyPostfix]
        public static void UpdateShotPrediction(PlayerAimAssistManager __instance) {
            UpdatePrediction(__instance);
        }
    }
}