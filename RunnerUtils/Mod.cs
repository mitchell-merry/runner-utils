using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RunnerUtils.Components;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RunnerUtils;

[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
public class Mod : BaseUnityPlugin
{
    public const string pluginGuid = "kestrel.iamyourbeast.runnerutils";
    public const string pluginName = "Runner Utils";
    public const string pluginVersion = "2.4.2";

    public static Mod Instance { get; private set; }
    private static RUInputManager InputManager { get; set; } = new();
    public static InGameLog Igl { get; private set; } = new InGameLog($"{pluginName}~Ingame Log (v{pluginVersion})");
    internal static new ManualLogSource Logger;
    
    public static ConfigEntry<float> throwCam_rangeScalar;
    public static ConfigEntry<bool> throwCam_unlockCamera;
    public static ConfigEntry<bool> throwCam_autoSwitch;
    public static ConfigEntry<bool> walkabilityOverlay;

    public static ConfigEntry<bool> saveLocation_verbose;

    public static ConfigEntry<bool> snowmanPercent;

    private static string loadBearingColonThree = ":3";

    public void Awake() {
        if (loadBearingColonThree != ":3") Application.Quit();

        gameObject.hideFlags = HideFlags.HideAndDontSave; //fuck you unity
        Instance = this;
        Logger = base.Logger;

        // TODO: menu-ify
        throwCam_unlockCamera = Config.Bind("Throw Cam", "Unlock Camera", false, "Unlock the camera when in throw cam");
        throwCam_rangeScalar = Config.Bind("Throw Cam", "Camera Range", 0.2f, new ConfigDescription("Follow range of the throw cam", new AcceptableValueRange<float>(0.01f, 3.0f)));
        throwCam_autoSwitch = Config.Bind("Throw Cam", "Auto Switch", false, "Automatically switch to throw cam when a projectile is thrown");
        walkabilityOverlay = Config.Bind("Walkability Overlay", "Enable", false, "Makes all walkable surfaces appear snowy, and all unwalkable surfaces appear black");

        saveLocation_verbose = Config.Bind("Location Save", "Verbose", false, "Log the exact location and rotation when a save or load is performed");

        snowmanPercent = Config.Bind("Snowman%", "Enable", true, "Displays your time upon destroying a snowman, to time the (silly) category snowman%");

        new Harmony(pluginGuid).PatchAll();
        Logger.LogInfo("Hiiiiiiiiiiii :3");
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update() {
        if (GameManager.instance.player is not null) {
            FairPlay.Update();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Additive) ShowTriggers.ExtendRegistry();
        ViewCones.OnSceneLoad();
    }

    [HarmonyPatch(typeof(Player))]
    public class PatchPlayer
    {
        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        public static void PlayerInitPostfix() {
            ShowTriggers.RegisterAll();
            Igl.Setup();
            ThrowCam.Reset();
            FairPlay.Init();
            HardFallOverlay.Instance.SetupIndicator();
            MovementDebug.Instance.Init();
        }
    }

    [HarmonyPatch(typeof(PlayerMovement))]
    public class PatchPlayerMovement
    {
        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        public static void MovementInitPostfix(CharacterController ___controller) {
            if (walkabilityOverlay.Value) {
                foreach (Terrain t in Terrain.activeTerrains) {
                    TerrainData td = t.terrainData;
                    WalkabilityOverlay.MakeWalkabilityTex(ref td, ___controller.slopeLimit);
                    t.terrainData = td;
                }
            }
        }
    }

    [HarmonyPatch(typeof(HUDLevelTimer), "Update")]
    public class The
    {
        [HarmonyPostfix]
        public static void Postfix(ref TMP_Text ___gradeText) {
            ___gradeText.text = $" {___gradeText.text}";
        }
    }
}