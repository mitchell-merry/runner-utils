using Enemy;
using RunnerUtils.Components.UI;
using System;
using System.Collections.Generic;

namespace RunnerUtils.Components;

public class RUInputManager
{
    public struct BindingInfo(string identifier, Action action, string guidKbm, string guidGamepad, string defaultKeyPath = Rebinding.UNBOUND_KEY)
    {
        public Action action = action;
        public string identifier = identifier;
        public string guidKbm = guidKbm;
        public string guidGamepad = guidGamepad;
        public string defaultKeyPath = defaultKeyPath;
    }

    public static List<BindingInfo> Bindings { get; } = [
      new(
            guidKbm: "8f6a1c2e-5d3b-4f7a-9a1e-1b2c3d4e5f01",
            guidGamepad: "8f6a1c2e-5d3b-4f7a-9a1e-1b2c3d4e5fA1",
            identifier: "Log Visibility Toggle",
            defaultKeyPath: "<Keyboard>/k",
            action: () => {
                Mod.Igl.ToggleVisibility();
                Mod.Igl.LogLine($"Toggled log visibility");
            }
        ),
        new(
            guidKbm: "2a9d4b77-6e21-4c8f-b2c4-7d9a0f1e3b02",
            guidGamepad: "2a9d4b77-6e21-4c8f-b2c4-7d9a0f1e3bA2",
            identifier: "Clear Log",
            defaultKeyPath: "<Keyboard>/j",
            action: () => {
                Mod.Igl.Clear();
                Mod.Igl.LogLine($"Cleared Log");
            }
        ),
        new(
            guidKbm: "c1e7f9a2-3b44-4d9a-8fcb-2a6d5e7f8c03",
            guidGamepad: "c1e7f9a2-3b44-4d9a-8fcb-2a6d5e7f8cA3",
            identifier: "Force Trigger Visibility On",
            defaultKeyPath: "<Keyboard>/o",
            action: () => {
                ShowTriggers.ShowAll();
                Mod.Igl.LogLine($"Enabled all triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            guidKbm: "d4b82f11-91c3-4e2d-9b5e-6a7c8d9e0f04",
            guidGamepad: "d4b82f11-91c3-4e2d-9b5e-6a7c8d9e0fA4",
            identifier: "Force Trigger Visibility Off",
            defaultKeyPath: "<Keyboard>/i",
            action: () => {
                ShowTriggers.HideAll();
                Mod.Igl.LogLine($"Disabled all triggers' visibility");
                FairPlay.triggersModified = false;
            }
        ),
        new(
            guidKbm: "e93a6d55-2f0b-4c1a-a8e3-5d7f9b1c2d05",
            guidGamepad: "e93a6d55-2f0b-4c1a-a8e3-5d7f9b1c2dA5",
            identifier: "Toggle Infinite Ammo",
            defaultKeyPath: "<Keyboard>/l",
            action: () => {
                if (!GameManager.instance.player.GetHUD()) return;
                InfiniteAmmo.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled infinite ammo");
            }
        ),
        new(
            guidKbm: "9c0d1e2f-3a4b-4c5d-8e9f-1a2b3c4d5e07",
            guidGamepad: "9c0d1e2f-3a4b-4c5d-8e9f-1a2b3c4d5eA7",
            identifier: "Toggle Throw Cam",
            defaultKeyPath: "<Keyboard>/semicolon",
            action: () => {
                if (ThrowCam.cameraAvailable) {
                    ThrowCam.ToggleCam();
                    Mod.Igl.LogLine($"Toggled throw cam");
                } else {
                    Mod.Igl.LogLine($"Unable to switch to throw cam ~ no thrown weapons are in the air");
                }
            }
        ),
        new(
            guidKbm: "1e2f3a4b-5c6d-4e7f-8a9b-0c1d2e3f4a08",
            guidGamepad: "1e2f3a4b-5c6d-4e7f-8a9b-0c1d2e3f4aA8",
            identifier: "Toggle auto jump",
            defaultKeyPath: "<Keyboard>/m",
            action: () => {
                AutoJump.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled auto jump");
            }
        ),
        new(
            guidKbm: "0f1e2d3c-4b5a-4c6d-8e7f-9a0b1c2d3e11",
            guidGamepad: "0f1e2d3c-4b5a-4c6d-8e7f-9a0b1c2d3eA1",
            identifier: "Toggle hard fall overlay",
            defaultKeyPath: "<Keyboard>/u",
            action: () => {
                HardFallOverlay.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled hf overlay");
            }
        ),
        new(
            guidKbm: "3c4d5e6f-7a8b-4c9d-8e0f-1a2b3c4d5e12",
            guidGamepad: "3c4d5e6f-7a8b-4c9d-8e0f-1a2b3c4d5eA2",
            identifier: "Toggle timestop",
            defaultKeyPath: "<Keyboard>/rightShift",
            action: () => {
                PauseTime.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled timestop");
            }
        ),
        new(
            guidKbm: "4e5f6a7b-8c9d-4a0b-9c1d-2e3f4a5b6c13",
            guidGamepad: "4e5f6a7b-8c9d-4a0b-9c1d-2e3f4a5b6cA3",
            identifier: "Save Location",
            defaultKeyPath: "<Keyboard>/leftBracket",
            action: () => {
                LocationSave.SaveLocation();
                Mod.Igl.LogLine($"Saved location {(Mod.saveLocation_verbose.Value ? LocationSave.StringLoc : "")}");
            }
        ),
        new(
            guidKbm: "7a8b9c0d-1e2f-4a3b-8c4d-5e6f7a8b9c14",
            guidGamepad: "7a8b9c0d-1e2f-4a3b-8c4d-5e6f7a8b9cA4",
            identifier: "Load Location",
            defaultKeyPath: "<Keyboard>/rightBracket",
            action: () => {
                if (LocationSave.savedPosition is not null) {
                    LocationSave.RestoreLocation();
                    Mod.Igl.LogLine($"Loaded previous location {(Mod.saveLocation_verbose.Value ? LocationSave.StringLoc : "")}");
                } else {
                    Mod.Igl.LogLine("No location saved!");
                }
            }
        ),
        new(
            guidKbm: "8b9c0d1e-2f3a-4b5c-9d6e-7f8a9b0c1d15",
            guidGamepad: "8b9c0d1e-2f3a-4b5c-9d6e-7f8a9b0c1dA5",
            identifier: "Clear Location",
            defaultKeyPath: "<Keyboard>/p",
            action: () => {
                LocationSave.ClearLocation();
                Mod.Igl.LogLine($"Cleared saved location");
            }
        ),
        new(
            guidKbm: "9d0e1f2a-3b4c-4d5e-8f6a-7b8c9d0e1f16",
            guidGamepad: "9d0e1f2a-3b4c-4d5e-8f6a-7b8c9d0e1fA6",
            identifier: "Toggle view cones visibility",
            defaultKeyPath: "<Keyboard>/y",
            action: () => {
                ViewCones.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled view cones' visibility");
            }
        ),
        new(
            guidKbm: "aa1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c17",
            guidGamepad: "aa1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3cA7",
            identifier: "Trigger Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAll();
                Mod.Igl.LogLine($"Toggled all triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            guidKbm: "bb2c3d4e-5f6a-4b7c-9d8e-0f1a2b3c4d18",
            guidGamepad: "bb2c3d4e-5f6a-4b7c-9d8e-0f1a2b3c4dA8",
            identifier: "OOB Box Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<PlayerOutOfBoundsBox>();
                Mod.Igl.LogLine($"Toggled OOB boxes' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            guidKbm: "cc3d4e5f-6a7b-4c8d-9e0f-1a2b3c4d5e19",
            guidGamepad: "cc3d4e5f-6a7b-4c8d-9e0f-1a2b3c4d5eA9",
            identifier: "Start Trigger Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<PlayerTimerStartBox>();
                Mod.Igl.LogLine($"Toggled start triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            guidKbm: "dd4e5f6a-7b8c-4d9e-8f0a-1b2c3d4e5f20",
            guidGamepad: "dd4e5f6a-7b8c-4d9e-8f0a-1b2c3d4e5fA0",
            identifier: "Spawner Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<EnemySpawner>();
                Mod.Igl.LogLine($"Toggled spawners' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            guidKbm: "ee5f6a7b-8c9d-4e0f-9a1b-2c3d4e5f6a21",
            guidGamepad: "ee5f6a7b-8c9d-4e0f-9a1b-2c3d4e5f6aA1",
            identifier: "Toggle advanced movement info",
            action: () => {
                MovementDebug.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled movement info");
            }
        ),
    ];
}
