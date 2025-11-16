using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AimAssist;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace RunnerUtils.Components
{
    internal class MagnetismOverlay : ComponentBase<MagnetismOverlay>
    {
        public override string Identifier => "Magnetism Overlay";
        public override bool ShowOnFairPlay => true;

        private static string HIGHLIGHTED_TARGET_OBJECT_NAME = "RunnerUtils-MagnetismOverlay-HighlightedTarget";
        private static List<GameObject> targetsInConeIndicators = new List<GameObject>();
        private static GameObject highlightedTargetIndicator;

        public override void Disable()
        {
            base.Disable();

            GameObject.Destroy(highlightedTargetIndicator);
            highlightedTargetIndicator = null;

            ClearTargetsInConeIndicators();
        }

        private static GameObject MakeTargetIndicator(Color color)
        {

            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.name = HIGHLIGHTED_TARGET_OBJECT_NAME;

            //float scaleFactor = 2f;
            //indicator.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            indicator.GetComponent<Collider>().enabled = false;

            Renderer sphereRenderer = indicator.GetComponent<Renderer>();
            sphereRenderer.material = new Material(Shader.Find("Sprites/Default"))
            {
                color = color
            };

            return indicator;
        }

        private static void ClearTargetsInConeIndicators()
        {
            foreach (GameObject target in targetsInConeIndicators)
            {
                GameObject.Destroy(target);
            }

            targetsInConeIndicators.Clear();
        }

        [HarmonyPatch(typeof(PlayerAimAssistManager), "Update")]
        public class MagnetismOverlayPatch
        {

            [HarmonyPostfix]
            public static void Postfix(ref PlayerAimAssistManager __instance)
            {
                if (!Instance.enabled) return;

                if (highlightedTargetIndicator == null) {
                    highlightedTargetIndicator = MakeTargetIndicator(Color.red);
                }

                if (__instance.highlightedTarget != null)
                {
                    highlightedTargetIndicator.GetComponent<Renderer>().enabled = true;
                    highlightedTargetIndicator.transform.position = __instance.highlightedTarget.transform.position;

                }
                else
                {
                    highlightedTargetIndicator.GetComponent<Renderer>().enabled = false;
                }

                ClearTargetsInConeIndicators();
                foreach (AimTarget target in __instance.targetsInCone)
                {
                    if (target == __instance.highlightedTarget) continue;

                    GameObject targetIndicator = MakeTargetIndicator(Color.blue);
                    targetIndicator.transform.position = target.transform.position;
                    targetIndicator.GetComponent<Renderer>().enabled = true;

                    targetsInConeIndicators.Add(targetIndicator);
                }


                //BoxCollider box = GameManager.instance.player.aimManager.highlightedTarget.collider as BoxCollider;

                //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //GameObject.Destroy(cube.GetComponent<Collider>());
                //cube.transform.SetParent(box.transform);
                //cube.transform.localPosition = box.transform.localPosition;
                //cube.transform.localRotation = Quaternion.identity;
                //cube.transform.localScale = box.size;
                //cube.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"))
                //{
                //    color = Color.gray
                //};
                //cube.name = "BoxColliderVisual";

            }
        }
    }
}

////GameObject[] allObjects = FindObjectsOfType<GameObject>();

//// 2. Loop through every object found
////foreach (GameObject obj in allObjects)
////{
////    // 3. Check if the current object's name matches the target name
////    if (obj.name == "BigBlueSphere")
////    {
////       // 4. Destroy the object
////       // Note: Actual destruction happens at the end of the current frame
////       Destroy(obj);
////   }
////}
//GameObject blueSphere = GameObject.Find("BigBlueSphere");
//if (blueSphere == null)
//{
//    blueSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//}

//blueSphere.name = "BigBlueSphere";

//blueSphere.transform.position = GameManager.instance.player.aimManager.highlightedTarget.transform.position;

//float scaleFactor = 2f;
//blueSphere.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

//Renderer sphereRenderer = blueSphere.GetComponent<Renderer>();

//sphereRenderer.enabled = true;
//sphereRenderer.material = new Material(Shader.Find("Sprites/Default")); // Assign a material
//sphereRenderer.material.color = Color.blue;
