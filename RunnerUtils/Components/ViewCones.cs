using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Enemy;
using HarmonyLib;
using TMPro;
using System.Collections;

namespace RunnerUtils.Components;

public class ViewCones : ComponentBase<ViewCones>
{
    public override string Identifier => "View Cones";
    public override bool ShowOnFairPlay => true;

    public override void Enable() {
        base.Enable();
        foreach (var cone in cones) {
            cone.ForceDisable = false;
        }
    }

    public override void Disable() {
        base.Disable();
        foreach (var cone in cones) {
            cone.ForceDisable = true;
        }
    }

    private static List<ViewCone> cones = [];

    public static void OnSceneLoad() => cones.Clear();

    private static Color seenColor = new Color(1, 0, 0, 0.25f);
    private static Color defaultColor = new Color(0.5f, 0, 0.5f, 0.4f);
    private static Color noSightlineColor = new Color(0, 0.5f, 0.5f, 0.25f);


    [HarmonyPatch(typeof(EnemyHuman))]
    public class EnemyHumanPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void CreateViewCone(EnemyHuman __instance) {
            if (__instance.GetSniperScanZone() is not null) return;
            var obj = new GameObject();
            var mr = obj.AddComponent<MeshRenderer>();
            mr.material = ShowTriggers.mat;
            mr.material.color = defaultColor;
            obj.name = "View Cone";
            obj.AddComponent<MeshFilter>();
            obj.transform.up = __instance.GetHeadAnchor().forward;
            obj.transform.position = __instance.GetHeadAnchor().position;
            obj.transform.parent = __instance.gameObject.transform;
            var cone = obj.AddComponent<ViewCone>();
        }

        [HarmonyPatch("OnPlayerSeen")]
        [HarmonyPostfix]
        public static void RecolorConeOnSeen(EnemyHuman __instance) {
            if (__instance.GetSniperScanZone() is not null) return;

            var cone = __instance.transform.Find("View Cone");
            cone?.GetComponent<ViewCone>().SetColor(seenColor);
        }

        [HarmonyPatch("RefreshPlayerInView")]
        [HarmonyPrefix]
        public static void UpdateViewConeVisibility(EnemyHuman __instance) {
            if (__instance.GetSniperScanZone() is not null) return;

            var coneObj = __instance.transform.Find("View Cone");
            if (coneObj is null) return;
            var cone = coneObj.GetComponent<ViewCone>();
            if (cone.ForceDisable) return;

            var targetPosition = GameManager.instance.player.GetTargetCenter(0f);
            bool inRange = Vector3.Distance(targetPosition, __instance.GetHeadAnchor().position) < __instance.GetDetectionRadius() * 2f;
            cone.SetVisibility(inRange);

            if (__instance.GetHasPersonallySeenPlayer()) return;

            cone.SetColor(!__instance.PositionSightlineClear(targetPosition) ? noSightlineColor : defaultColor);
        }
    }

    [HarmonyPatch(typeof(Enemy.Enemy))]
    public class EnemyPatch
    {
        [HarmonyPatch("Kill")]
        [HarmonyPostfix]
        public static void DestroyViewCone(Enemy.Enemy __instance) {
            if (__instance is not EnemyHuman) return;
            var coneTransform = __instance.transform.Find("View Cone");
            if (coneTransform is null) return;
            UnityEngine.Object.Destroy(coneTransform.gameObject);
        }
    }

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class ViewCone : MonoBehaviour
    {
        private static Dictionary<(float, float), Mesh> cachedMeshes = new();

        private const int defaultSubdivisions = 25;
        public bool ForceDisable { get => m_forceDisable; set {
                m_forceDisable = value;
                m_renderer.enabled = !value;
            }
        }
        private MeshRenderer m_renderer;

        private bool m_forceDisable;

        public void SetVisibility(bool value) {
            if (!ForceDisable) {
                GetComponent<MeshRenderer>().enabled = value;
            }
        }

        public void Start() {
            var enemy = transform.parent.gameObject.GetComponent<EnemyHuman>();

            Mesh mesh;
            float range = enemy.GetDetectionRadius();
            float angle = enemy.GetDetectionAngle();

            if (cachedMeshes.ContainsKey((range, angle))) {
                mesh = cachedMeshes[(range, angle)];
            } else {
                mesh = CreateConeMesh(defaultSubdivisions, range, angle);
                cachedMeshes[(range, angle)] = mesh;
            }

            GetComponent<MeshFilter>().sharedMesh = mesh;
            m_renderer = GetComponent<MeshRenderer>();
            cones.Add(this);
            ForceDisable = !Instance.enabled;
        }

        public void SetColor(Color col) {
            m_renderer.material.color = col;
        }

        public void OnDestroy() {
            cones.Remove(this);
        }

        private static Mesh CreateConeMesh(int subdivisions, float height, float angle) {
            Mesh mesh = new Mesh();

            var radius = height * Mathf.Tan(0.5f * angle * Mathf.Deg2Rad);

            Vector3[] vertices = new Vector3[subdivisions + 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(subdivisions * 2) * 3];

            vertices[0] = new Vector3(0f, height, 0f);
            uv[0] = new Vector2(0.5f, 0f);
            for (int i = 0; i < subdivisions; i++) {
                float ratio = (float)i / (subdivisions-1);
                float r = ratio * (Mathf.PI * 2f);
                float x = Mathf.Cos(r) * radius;
                float z = Mathf.Sin(r) * radius;
                vertices[i + 1] = new Vector3(x, height, z);

                uv[i + 1] = new Vector2(ratio, 0f);
            }
            vertices[subdivisions + 1] = Vector3.zero;
            uv[subdivisions + 1] = new Vector2(0.5f, 1f);

            for (int i = 0; i < subdivisions - 1; i++) {
                int offset = i * 3;
                triangles[offset] = 0;
                triangles[offset + 1] = i + 1;
                triangles[offset + 2] = i + 2;
            }

            // construct sides
            int bottomOffset = subdivisions * 3;
            for (int i = 0; i < subdivisions - 1; i++) {
                int offset = i * 3 + bottomOffset;
                triangles[offset] = i + 1;
                triangles[offset + 1] = subdivisions + 1;
                triangles[offset + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

    }
}