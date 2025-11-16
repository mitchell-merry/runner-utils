using BepInEx.Configuration;
using Enemy;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RunnerUtils.Components;

public class RUInputManager
{
    public struct DefaultBindingInfo(string identifier, Action action, string description = "", KeyCode key = KeyCode.None)
    {
        public Action action = action;
        public string identifier = identifier;
        public string description = description;
        public KeyCode key = key;
    }

    private Dictionary<ConfigEntry<KeyCode>, Action> bindings = [];

    // Bind the default config values to a specified config file
    public void BindToConfig(ConfigFile config) {
        foreach (var defaultBinding in DefaultBindings) {
            var configEntry = config.Bind(
                defaultBinding.key != KeyCode.None ? "Keybinds" : "Keybinds.Optional",
                defaultBinding.identifier,
                defaultBinding.key,
                defaultBinding.description
            );

            bindings[configEntry] = defaultBinding.action;
        }
    }

    public void Update() {
        if (GameManager.instance.levelController is null || GameManager.instance.levelController.IsLevelPaused()) return;
        foreach (var binding in bindings) {
            if (Input.GetKeyDown(binding.Key.Value)) { // lol
                binding.Value?.Invoke();
            }
        }
    }

    private static List<DefaultBindingInfo> DefaultBindings { get; } = [
        new(
            identifier: "Log Visibility Toggle",
            key: KeyCode.K,
            action: () => {
                Mod.Igl.ToggleVisibility();
                Mod.Igl.LogLine($"Toggled log visibility");
            }
        ),
        new(
            identifier: "Clear Log",
            key: KeyCode.J,
            action: () => {
                Mod.Igl.Clear();
                Mod.Igl.LogLine($"Cleared Log");
            }
        ),
        new(
            identifier: "Force Trigger Visibility On",
            key: KeyCode.O,
            action: () => {
                ShowTriggers.ShowAll();
                Mod.Igl.LogLine($"Enabled all triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            identifier: "Force Trigger Visibility Off",
            key: KeyCode.I,
            action: () => {
                ShowTriggers.HideAll();
                Mod.Igl.LogLine($"Disabled all triggers' visibility");
                FairPlay.triggersModified = false;
            }
        ),
        new(
            identifier: "Toggle Infinite Ammo",
            key: KeyCode.L,
            action: () => {
                if (!GameManager.instance.player.GetHUD()) return;
                InfiniteAmmo.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled infinite ammo");
            }
        ),
        new(
            identifier: "Toggle Throw Cam",
            key: KeyCode.Semicolon,
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
            identifier: "Toggle auto jump",
            key: KeyCode.M,
            action: () => {
                AutoJump.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled auto jump");
            }
        ),
        new(
            identifier: "Toggle magnetism overlay",
            key: KeyCode.Quote,
            action: () => {
                MagnetismOverlay.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled magnetism overlay");
            }
        ),
        new(
            identifier: "Toggle hard fall overlay",
            key: KeyCode.U,
            action: () => {
                HardFallOverlay.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled hf overlay");
            }
        ),
        new(
            identifier: "Toggle timestop",
            key: KeyCode.RightShift,
            action: () => {
                PauseTime.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled timestop");
            }
        ),
        new(
            identifier: "Save Location",
            key: KeyCode.LeftBracket,
            action: () => {
                LocationSave.SaveLocation();
                Mod.Igl.LogLine($"Saved location {(Mod.saveLocation_verbose.Value ? LocationSave.StringLoc : "")}");
            }
        ),
        new(
            identifier: "Load Location",
            key: KeyCode.RightBracket,
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
            identifier: "Clear Location",
            key: KeyCode.P,
            action: () => {
                LocationSave.ClearLocation();
                Mod.Igl.LogLine($"Cleared saved location");
            }
        ),
        new(
            identifier: "Toggle view cones visibility",
            key: KeyCode.Y,
            action: () => {
                ViewCones.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled view cones' visibility");
            }
        ),

        // OPTIONAL SETTINGS

        new(
            identifier: "Trigger Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAll();
                Mod.Igl.LogLine($"Toggled all triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            identifier: "OOB Box Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<PlayerOutOfBoundsBox>();
                Mod.Igl.LogLine($"Toggled OOB boxes' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            identifier: "Start Trigger Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<PlayerTimerStartBox>();
                Mod.Igl.LogLine($"Toggled start triggers' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            identifier: "Spawner Visibility Toggle",
            action: () => {
                ShowTriggers.ToggleAllOf<EnemySpawner>();
                Mod.Igl.LogLine($"Toggled spawners' visibility");
                FairPlay.triggersModified = true;
            }
        ),
        new(
            identifier: "Toggle advanced movement info",
            action: () => {
                MovementDebug.Instance.Toggle();
                Mod.Igl.LogLine($"Toggled movement info");
            }
        ),
    ];
}
