using BepInEx.Configuration;

namespace RunnerUtils;

internal static class Configs
{
    private static ConfigEntry<bool> m_skipSplashCardsEntry;
    private static ConfigEntry<float> m_throwCamRangeScaleEntry;
    private static ConfigEntry<bool> m_throwCamUnlockCameraEntry;
    private static ConfigEntry<bool> m_throwCamAutoSwitchEntry;
    private static ConfigEntry<bool> m_walkabilityOverlayEntry;
    private static ConfigEntry<bool> m_wallsVisibleEntry;
    private static ConfigEntry<bool> m_saveLocationVerboseEntry;
    private static ConfigEntry<float> m_timeScaleEntry;

    private static ConfigEntry<bool> m_snowmanPercentEntry;

    public static void Init(ConfigFile config) {
        m_skipSplashCardsEntry = config.Bind("Options", "Skip Splash Cards", false, "Skip the splash cards on boot");

        m_throwCamRangeScaleEntry = config.Bind("Throw Cam", "Camera Range", 0.2f, new ConfigDescription("Follow range of the throw cam", new AcceptableValueRange<float>(0.01f, 3.0f)));
        m_throwCamUnlockCameraEntry = config.Bind("Throw Cam", "Unlock Camera", false, "Unlock the camera when in throw cam");
        m_throwCamAutoSwitchEntry = config.Bind("Throw Cam", "Auto Switch", false, "Automatically switch to throw cam when a projectile is thrown");
        m_walkabilityOverlayEntry = config.Bind("Walkability Overlay", "Enable", false, "Makes all walkable surfaces appear snowy, and all unwalkable surfaces appear black");
        m_wallsVisibleEntry = config.Bind("Options", "Show Walls", true, "Show level geometry");
        m_timeScaleEntry = config.Bind("Time Scale", "Scale", 1f, new ConfigDescription("Game timescale", new AcceptableValueRange<float>(0f, 1f)));

        m_saveLocationVerboseEntry = config.Bind("Location Save", "Verbose", false, "Log the exact location and rotation when a save or load is performed");

        m_snowmanPercentEntry = config.Bind("Snowman%", "Enable", true, "Displays your time upon destroying a snowman, to time the (silly) category snowman%");
    }

    public static bool SkipSplashCardsEnabled {
        get => m_skipSplashCardsEntry.Value;
        set => m_skipSplashCardsEntry.Value = value;
    }
    
    public static float ThrowCamRangeScale {
        get => m_throwCamRangeScaleEntry.Value;
        set => m_throwCamRangeScaleEntry.Value = value;
    }
    
    public static bool ThrowCamUnlockCameraEnabled {
        get => m_throwCamUnlockCameraEntry.Value;
        set =>  m_throwCamUnlockCameraEntry.Value = value;
    }
    
    public static bool ThrowCamAutoSwitchEnabled {
        get => m_throwCamAutoSwitchEntry.Value;
        set => m_throwCamAutoSwitchEntry.Value = value;
    }

    public static bool WalkabilityOverlayEnabled {
        get => m_walkabilityOverlayEntry.Value;
        set => m_walkabilityOverlayEntry.Value = value;
    }

    public static bool WallsVisible {
        get => m_wallsVisibleEntry.Value;
        set => m_wallsVisibleEntry.Value = value;
    }

    public static bool SaveLocationVerboseEnabled {
        get => m_saveLocationVerboseEntry.Value;
        set => m_saveLocationVerboseEntry.Value = value;
    }

    public static float TimeScale {
        get => m_timeScaleEntry.Value;
        set => m_timeScaleEntry.Value = value;
    }

    public static bool SnowmanPercentEnabled {
        get => m_snowmanPercentEntry.Value;
        set => m_snowmanPercentEntry.Value = value;
    }
}

