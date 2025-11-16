using UnityEngine;
using Fleece;

namespace RunnerUtils.Components.UI
{
    internal class RunnerUtilsSettings
    {
        public class UISettingsSubMenuCustom : UISettingsSubMenu
        {
            private UISettingsOptionToggle showAttemptCount;

            private static Jumper attemptShowToggleText = Text.MakeJumper("SHOW ATTEMPT COUNT");

            public override void Start()
            {
                base.Start();

                // clear out the children and component
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
                Destroy(GetComponent<UISettingsSubMenuVisual>());

                // setup our settings
                showAttemptCount = Base.MakeToggleOption(transform, attemptShowToggleText);
            }

            public override void SaveSettings()
            {
                base.SaveSettings();

                if (showAttemptCount != null)
                    Mod.Logger.LogInfo(showAttemptCount.GetToggled());

                // TODO save the settings
                // TODO reference the settings
                // To be done in a later PR
            }
        }
    }
}
