using Fleece;
using RunnerUtils.Components;
using UnityEngine.UI;

namespace RunnerUtils.UI;

 // Custom settings window to change these settings
 public class UISettingsSubMenuRunnerUtils : UISettingsSubMenu
 {
     private UISettingsOptionToggle m_skipSplashCardsToggle;
     private UISettingsOptionToggle m_walkabilityOverlayToggle;
     private UISettingsOptionToggle m_verboseLocationSaveToggle;
     private UISettingsOptionToggle m_snowmanPercentToggle;
     private UISettingsOptionToggle m_throwCamUnlockCameraToggle;
     private UISettingsOptionToggle m_throwCamAutoSwitchToggle;
    private static SliderOption m_timeScaleSlider;

     private static Jumper m_splashCardSkipText = FleeceUtil.MakeJumper("Skip splash cards");
     private static Jumper m_walkabilityOverlayText = FleeceUtil.MakeJumper("Walkability Overlay");
     private static Jumper m_verboseLocationSaveText = FleeceUtil.MakeJumper("Log exact location on save/load");
     private static Jumper m_snowmanPercentText = FleeceUtil.MakeJumper("Enable Snowman% Timer");
     private static Jumper m_throwCamUnlockCameraText = FleeceUtil.MakeJumper("Unlock Camera");
     private static Jumper m_throwCamAutoSwitchText = FleeceUtil.MakeJumper("Auto Switch");
     private static Jumper m_timeScaleText = FleeceUtil.MakeJumper("Timescale");

     public static void SetTimeScaleSlider(float value) {
         m_timeScaleSlider?.SetValueWithoutNotify(value);
     }

     public void Awake() {
         // TODO: once this gets too big for the window
         // (if it's smaller than the window it's buggy for some reason, TODO fix that)
         var content = Base.MakeMenuScrollable(this);

         // setup our settings
         var heading = Base.MakeHeading(
             content.transform,
             "RunnerUtils Settings",
             "Some of these options are banned from regular play.\nPlease show the top right corner during all recordings with this mod."
         );
         // heading's default padding is for if it's in the middle of a page
         heading.GetComponent<VerticalLayoutGroup>().padding.top = 0;
         heading.GetComponent<VerticalLayoutGroup>().padding.bottom = 10;

         m_skipSplashCardsToggle = Base.MakeToggleOption(content.transform, m_splashCardSkipText, Configs.SkipSplashCardsEnabled);
         m_walkabilityOverlayToggle = Base.MakeToggleOption(content.transform, m_walkabilityOverlayText, Configs.WalkabilityOverlayEnabled);
         m_verboseLocationSaveToggle = Base.MakeToggleOption(content.transform, m_verboseLocationSaveText, Configs.SaveLocationVerboseEnabled);
         m_snowmanPercentToggle = Base.MakeToggleOption(content.transform, m_snowmanPercentText, Configs.SnowmanPercentEnabled);

         var timeScaleHeading = Base.MakeHeading(content.transform, "Time Scale");
         timeScaleHeading.GetComponent<VerticalLayoutGroup>().padding.top = 10;
         timeScaleHeading.GetComponent<VerticalLayoutGroup>().padding.bottom = 0;
         m_timeScaleSlider = Base.MakeSliderOption(content.transform, m_timeScaleText, 0f, 1f, Configs.TimeScale, TimeScaleControl.SetScale);

         var throwCamHeading = Base.MakeHeading(content.transform, "Throw Cam");
         throwCamHeading.GetComponent<VerticalLayoutGroup>().padding.top = 10;
         throwCamHeading.GetComponent<VerticalLayoutGroup>().padding.bottom = 0;

         m_throwCamUnlockCameraToggle = Base.MakeToggleOption(content.transform, m_throwCamUnlockCameraText, Configs.ThrowCamUnlockCameraEnabled);
         m_throwCamAutoSwitchToggle = Base.MakeToggleOption(content.transform, m_throwCamAutoSwitchText, Configs.ThrowCamAutoSwitchEnabled);
         // TODO: slider for throw cam camera range
     }

     public override void SaveSettings() {
         base.SaveSettings();
         Configs.SkipSplashCardsEnabled = m_skipSplashCardsToggle.GetToggled();
         Configs.WalkabilityOverlayEnabled = m_walkabilityOverlayToggle.GetToggled();
         Configs.SaveLocationVerboseEnabled = m_verboseLocationSaveToggle.GetToggled();
         Configs.SnowmanPercentEnabled = m_snowmanPercentToggle.GetToggled();

         Configs.ThrowCamUnlockCameraEnabled = m_throwCamUnlockCameraToggle.GetToggled();
         Configs.ThrowCamAutoSwitchEnabled = m_throwCamAutoSwitchToggle.GetToggled();
     }
 }