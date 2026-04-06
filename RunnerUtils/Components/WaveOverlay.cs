using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CurvedUI;
using Enemy;
using HarmonyLib;
using Objectives;
using Progress;
using UnityEngine;

namespace RunnerUtils.Components;
public class WaveOverlay : ComponentBase<WaveOverlay>
{
    public override string Identifier => "Advanced Movement Info";
    public override bool ShowOnFairPlay => false;

    private static InGameLog igl = new InGameLog($"{Mod.pluginName}~Wave Overlay", 150);
    private static InGameLog spawnIgl = new InGameLog($"{Mod.pluginName}~Wave Spawns", 15);


    private static List<Color> colours = new List<Color>
    {
        new Color(1f, 0f, 0f),        // Red
        //new Color(1f, 0.25f, 0f),     // Red-Orange
        new Color(1f, 0.5f, 0f),      // Orange
        new Color(1f, 0.75f, 0f),     // Yellow-Orange
        new Color(1f, 1f, 0f),        // Yellow

        new Color(0.75f, 1f, 0f),     // Yellow-Green
        new Color(0.5f, 1f, 0f),      // Lime
        //new Color(0.25f, 1f, 0f),     // Light Green
        new Color(0f, 1f, 0f),        // Green

        //new Color(0f, 1f, 0.25f),     // Green-Cyan
        new Color(0f, 1f, 0.5f),      // Sea Green
        new Color(0f, 1f, 0.75f),     // Turquoise
        new Color(0f, 1f, 1f),        // Cyan

        new Color(0f, 0.75f, 1f),     // Sky Blue
        new Color(0f, 0.5f, 1f),      // Light Blue
        new Color(0f, 0.25f, 1f),     // Blue
        //new Color(0f, 0f, 1f),        // Deep Blue

        //new Color(0.25f, 0f, 1f),     // Indigo
        new Color(0.5f, 0f, 1f),      // Violet
        new Color(0.75f, 0f, 1f),     // Purple

        new Color(1f, 0f, 1f),        // Magenta
        //new Color(1f, 0f, 0.75f),     // Pink-Magenta
        new Color(1f, 0f, 0.5f),      // Pink
        new Color(1f, 0f, 0.25f),     // Hot Pink

        new Color(0.75f, 0f, 0f)      // Dark Red (loops back nicely)
    };

    public void Init()
    {
        igl.anchoredPos = new Vector2(-425, 535);
        igl.Setup();

        spawnIgl.anchoredPos = new Vector2(725, -100);
        spawnIgl.Setup();
        if (!enabled) {
            igl.Hide();
            spawnIgl.Hide();
        } else {
            igl.Show();
            spawnIgl.Show();
        }
    }

    static GameObject preferredSpawn = null;
    static GameObject lastCylinder = null;

    public override void Enable()
    {
        base.Enable();
        igl.Show();
        spawnIgl.Show();

        if (preferredSpawn != null)
        {
            preferredSpawn.SetActive(true);
        }

        if (lastCylinder != null)
        {
            lastCylinder.SetActive(true);
        }
    }

    public override void Disable()
    {
        base.Disable();
        igl.Hide();
        spawnIgl.Hide();

        if (preferredSpawn != null)
        {
            preferredSpawn.SetActive(false);
        }

        if (lastCylinder != null)
        {
            lastCylinder.SetActive(false);
        }
    }

    public static GameObject SpawnCylinder(
        Vector3 position,
        Color color,
        float height = 100f,
        float radius = 0.5f)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        Collider col = cylinder.GetComponent<Collider>();
        if (col != null)
        {
            UnityEngine.Object.Destroy(col);
        }

        TweakCylinder(ref cylinder, position, color, height, radius);

        return cylinder;
    }

    static void TweakCylinder(
        ref GameObject cylinder,
        Vector3 position,
        Color color,
        float height = 100f,
        float radius = 0.5f)
    {
        cylinder.transform.position = position;
        cylinder.transform.localScale = new Vector3(radius, height / 2f, radius);

        Renderer renderer = cylinder.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            renderer.material = mat;
        }
    }

    // remove initial spawn lines
    // cylinder height based on weapon
    // colour based on order in wave

    static bool showCylinders = false;

    [HarmonyPatch(typeof(LevelObjectiveSurviveWaves), "Start")]
    public class OnHordeLoadIn
    {
        [HarmonyPostfix]
        public static void Prefix(LevelObjectiveSurviveWaves __instance)
        {
            showCylinders = false;
            spawnIgl.Clear();
        }
    }

    [HarmonyPatch(typeof(LevelObjectiveSurviveWaves), "StartSpawningWaves")]
    public class OnHordeStartSpawningWaves
    {
        [HarmonyPostfix]
        public static void Prefix(LevelObjectiveSurviveWaves __instance)
        {
            showCylinders = true;
        }
    }


    [HarmonyPatch(typeof(LevelHordeManager), "Update")]
    public class LogHordeInformation
    {
        [HarmonyPostfix]
        public static void Postfix(LevelHordeManager __instance)
        {
            if (!Instance.enabled) return;

            List<string> lines = new List<string>();

            lines.Add($"Begun: {__instance.objective.wavesBegun}");
            lines.Add($"Current wave: {__instance.objective.waveIndex}");
            lines.Add($"Total enemies to kill: {__instance.objective.GetTotalEnemiesToKill()}");
            lines.Add($"# of dead enemies: {__instance.objective.GetKilledEnemies()}");
            lines.Add($"Ideal distance: {__instance.objective.idealDistance}");
            lines.Add($"Minimum spawn distance: {__instance.objective.minimumSpawnDistance}");
            lines.Add($"Spawner index: {__instance.objective.spawnerIndex}");
            lines.Add($"Spawn type: {__instance.objective.spawnType}");

            for (int i = 0; i < __instance.objective.waves.Length; i++)
            {
                var wave = __instance.objective.waves[i];

                var aliveEnemies = 0;
                foreach (EnemyHuman enemyHuman in wave.spawnedEnemies)
                {
                    if (enemyHuman && enemyHuman.IsAlive())
                    {
                        aliveEnemies++;
                    }
                }

                var spawnedEnemies = 0;
                foreach (var spawner in wave.enemyOrder)
                {
                    if (spawner.spawned)
                    {
                        spawnedEnemies++;
                    }
                }

                lines.Add($"Wave {i} (\"{wave.name}\"):");
                lines.Add($"  Is active: {wave.active}");
                lines.Add($"  # of enemies to spawn: {wave.enemyOrder.Count}");
                lines.Add($"  # of alive enemies: {aliveEnemies}");
                lines.Add($"  # of enemies spawned: {spawnedEnemies}");
                lines.Add($"  # of dead enemies: {wave.GetKilledEnemies()}");
                lines.Add($"  Ememies remaining before wave continues: {wave.enemiesRemainingBeforeWaveContinues}");
                lines.Add($"  Spawn index: {wave.spawnIndex}");
                lines.Add($"  Spawn interval: {wave.spawnIntervalInSeconds} seconds");
                lines.Add($"  Spawn countdown: {wave.spawnCountdown} seconds");

                //lines.Add($"  Spawn loops: {wave.spawnLoops}");

                // Red herrings: 
                // - maximumEnemiesAtATime (unused, all enemies are spawned)

                //lines.Add($"  Max # of alive enemies: {wave.maximumEnemiesAtATime}");
            }

            for (int i = 0; i < lines.Count; i++)
            {
                igl.SetBufferLine(i, lines[i]);
            }

            igl.FlushBuffer();
        }
    }


    [HarmonyPatch(typeof(EnemyWaveBlueprint), "FixedUpdate")]
    public class WaveUpdate
    {
        [HarmonyPostfix]
        public static void Prefix(EnemyWaveBlueprint __instance)
        {
            //if (!Instance.enabled) return;
            //if (!__instance.active || __instance.spawnIndex >= __instance.enemyOrder.Count)
            //{
            //    return;
            //}

            //var
            //   enemyBlueprint = __instance.enemyOrder[__instance.spawnIndex];
            //LevelObjectiveSurviveWaves.SpawnerRating spawnerRating = null;
            //foreach (LevelObjectiveSurviveWaves.SpawnerRating spawnerRating2 in __instance.root.spawnerRatings)
            //{
            //    if (spawnerRating2.GetRating(false, 20f) <= 0f)
            //    {
            //        continue;
            //    }

            //    if (spawnerRating == null)
            //    {
            //        spawnerRating = spawnerRating2;
            //        continue;
            //    }

            //    float num = 20f;
            //    if (enemyBlueprint.GetUninstantiatedStartingWeapon())
            //    {
            //        WeaponInformation weaponDetails = enemyBlueprint.GetUninstantiatedStartingWeapon().GetWeaponDetails();
            //        if (weaponDetails)
            //        {
            //            Debug.Log("weapon " + weaponDetails.GetDisplayName() + " - " + weaponDetails.GetEnemyMaximumRange() + " - " + weaponDetails.GetEnemyIdealRange());
            //            num = (weaponDetails.GetEnemyMaximumRange() + weaponDetails.GetEnemyIdealRange()) * 0.5f;
            //        }
            //    }
            //    if (spawnerRating2.GetRating(true, num) > spawnerRating.GetRating(true, num))
            //    {
            //        spawnerRating = spawnerRating2;
            //    }
            //}

            //if (spawnerRating != null)
            //{
            //    var position = spawnerRating.GetSpawner().transform.position;

            //    if (preferredSpawn == null)
            //    {
            //        preferredSpawn = SpawnCylinder(position, Color.red);
            //    }
            //    else
            //    {
            //        TweakCylinder(ref preferredSpawn, position, Color.red);
            //    }
            //}
        }
    }

    [HarmonyPatch(typeof(EnemyWaveBlueprint), "AddEnemyForTracking")]
    public class OnSpawn
    {
        [HarmonyPostfix]
        public static void Postfix(EnemyWaveBlueprint __instance, EnemyHuman enemy)
        {
            if (!Instance.enabled || !showCylinders) return;

            var waveIndex = 0;
            try
            {
                for (int i = 0; i < __instance.root.waves.Length; i++)
                {
                    if (__instance == __instance.root.waves[i])
                    {
                        waveIndex = i;
                        break;
                    }
                }
            } catch {
                Debug.Log("FAILED DURING HSGHSLDHLKJSFHJKSLDFH");
                return;
            }

            // Wave 1 Spawn 2: Weapon - Combat Knife
            var weaponName = "unknown weapon";
            try
            {
                weaponName = enemy.startingWeaponPickupPrefab.name;
            } catch { }

            var colourHex = ColorUtility.ToHtmlStringRGB(colours[__instance.spawnIndex]);
            spawnIgl.LogLine($"<color=#{colourHex}>Wave {waveIndex} Spawn {__instance.spawnIndex}: {weaponName}</color>");

            var cylinder = SpawnCylinder(enemy.transform.position, colours[__instance.spawnIndex]);

            lastCylinder = cylinder;

            // Destroy after delay
            UnityEngine.Object.Destroy(cylinder, 15f);
        }
    }
}