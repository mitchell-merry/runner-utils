using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using RunnerUtils.Components;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RunnerUtils;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Mod : BaseUnityPlugin
{

    public static Mod Instance { get; private set; }
    private static RUInputManager InputManager { get; set; } = new();
    public static InGameLog Igl { get; private set; } = new InGameLog($"{PluginInfo.PLUGIN_NAME}~Ingame Log (v{PluginInfo.PLUGIN_VERSION})");
    internal static new ManualLogSource Logger;

    private static string loadBearingColonThree = ":3";

    public void Awake() {
        if (loadBearingColonThree != ":3") Application.Quit();

        gameObject.hideFlags = HideFlags.HideAndDontSave; //fuck you unity
        Instance = this;
        Logger = base.Logger;

        Configs.Init(Config);
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Logger.LogInfo("Hiiiiiiiiiiii :3");
    }
    
    private void Update() {
        if (GameManager.instance.player is not null) {
            FairPlay.Update();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Additive) {
            ShowTriggers.ExtendRegistry();
            ClimbSnapOverlay.ExtendRegistry();
        }
        ViewCones.OnSceneLoad();
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Initialize))]
    public class PlayerPatch
    {
        [HarmonyPostfix]
        public static void PlayerInitPostfix() {
            ShowTriggers.RegisterAll();
            ClimbSnapOverlay.RegisterAll();
            Igl.Setup();
            ThrowCam.Reset();
            FairPlay.Init();
            HardFallOverlay.Instance.SetupIndicator();
            MovementDebug.Instance.Init();
        }
    }

    [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.Initialize))]
    public class PlayerMovementPatch
    {
        [HarmonyPostfix]
        public static void InitWalkabilityOverlay(CharacterController ___controller) {
            if (Configs.WalkabilityOverlayEnabled) {
                foreach (var t in Terrain.activeTerrains) {
                    var terrainData = t.terrainData;
                    WalkabilityOverlay.MakeWalkabilityTex(ref  terrainData, ___controller.slopeLimit);
                    t.terrainData =  terrainData;
                }
            }
        }
    }

    [HarmonyPatch(typeof(HUDLevelTimer), nameof(HUDLevelTimer.Update))]
    public class The
    {
        [HarmonyPostfix]
        public static void Postfix(ref TMP_Text ___gradeText) {
            ___gradeText.text = $" {___gradeText.text}";
        }
    }
}