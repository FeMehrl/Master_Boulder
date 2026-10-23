using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Boulderhalle.EditorTools
{
    /// <summary>
    /// Menü "Boulderhalle": erzeugt das statische Modell als Szene, speichert die Materialien als Assets
    /// und platziert den Spieler (XR Origin, falls vorhanden – sonst ein Desktop-Testrig).
    /// </summary>
    public static class BoulderhalleSceneBuilder
    {
        const string RootFolder = "Assets/Boulderhalle";
        const string MaterialFolder = RootFolder + "/Generated/Materials";
        const string ScenePath = RootFolder + "/Scenes/Boulderhalle.unity";

        static readonly string[] XrOriginPrefabNames =
        {
            "Complete XR Origin Set Up", "XR Origin (XR Rig)", "XR Origin (VR)", "XR Origin"
        };

        [MenuItem("Boulderhalle/Statisches Modell erzeugen (VR)", priority = 0)]
        public static void BuildForVr() => Build(preferXr: true);

        [MenuItem("Boulderhalle/Statisches Modell erzeugen (Desktop-Test ohne VR)", priority = 1)]
        public static void BuildForDesktop() => Build(preferXr: false);

        static void Build(bool preferXr)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");

            var library = new MaterialLibrary();
            var result = BoulderhalleBuilder.Build(library);
            SaveMaterials(library);
            MarkStatic(result.Root);

            bool placedXr = preferXr && TryPlaceXrOrigin(result.SpawnPoint);
            if (placedXr) AddTeleportAreas(result.WalkableSurfaces);
            else CreateDesktopRig(result.SpawnPoint);

            EnsureFolder(Path.GetDirectoryName(ScenePath)?.Replace('\\', '/'));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();

            Selection.activeGameObject = result.Root;
            Debug.Log($"Boulderhalle erzeugt und gespeichert unter {ScenePath} " +
                      (placedXr ? "(mit XR Origin)." : "(mit Desktop-Testrig – für VR siehe README)."));
        }

        // ------------------------------------------------------------------ Spieler

        static bool TryPlaceXrOrigin(Transform spawn)
        {
            var paths = AssetDatabase.FindAssets("t:Prefab").Select(AssetDatabase.GUIDToAssetPath).ToList();
            foreach (var wanted in XrOriginPrefabNames)
            {
                var path = paths.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == wanted);
                if (path == null) continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
                Debug.Log($"XR-Rig platziert: {path}");
                return true;
            }
            Debug.LogWarning("Kein XR-Origin-Prefab gefunden. Bitte das XR Interaction Toolkit samt Sample \"Starter Assets\" " +
                             "importieren (oder das Unity-VR-Template verwenden). Es wird ein Desktop-Testrig erstellt.");
            return false;
        }

        static void CreateDesktopRig(Transform spawn)
        {
            var rig = new GameObject("Spieler_Desktop");
            rig.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            var cc = rig.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.35f; // reicht, um auf die Matte zu steigen

            var camGo = new GameObject("Kamera") { tag = "MainCamera" };
            camGo.transform.SetParent(rig.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            camGo.AddComponent<AudioListener>();

            rig.AddComponent<DesktopRigController>().cameraTransform = camGo.transform;
        }

        /// <summary>
        /// Fügt Böden und Matten als TeleportationArea hinzu (per Reflection, damit das Skript
        /// auch ohne installiertes XR Interaction Toolkit kompiliert). Unterstützt XRI 2.x und 3.x.
        /// </summary>
        static void AddTeleportAreas(IEnumerable<GameObject> surfaces)
        {
            var areaType = FindType("UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea")
                           ?? FindType("UnityEngine.XR.Interaction.Toolkit.TeleportationArea");
            if (areaType == null) return;

            object teleportLayer = null;
            var maskType = FindType("UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask");
            var getMask = maskType?.GetMethod("GetMask", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string[]) }, null);
            if (getMask != null)
            {
                var raw = getMask.Invoke(null, new object[] { new[] { "Teleport" } });
                if (raw is int bits)
                {
                    var toMask = maskType.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(m =>
                        m.Name == "op_Implicit" && m.ReturnType == maskType && m.GetParameters()[0].ParameterType == typeof(int));
                    if (bits != 0 && toMask != null) teleportLayer = toMask.Invoke(null, new object[] { bits });
                }
                else if (raw != null)
                {
                    teleportLayer = raw;
                }
            }

            var layersProperty = areaType.GetProperty("interactionLayers");
            foreach (var go in surfaces)
            {
                if (go.GetComponent(areaType) != null) continue;
                var area = go.AddComponent(areaType);
                if (teleportLayer != null && layersProperty != null) layersProperty.SetValue(area, teleportLayer);
            }
        }

        static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }

        // ------------------------------------------------------------------ Assets

        static void SaveMaterials(MaterialLibrary library)
        {
            EnsureFolder(MaterialFolder);
            foreach (var entry in library.All)
            {
                var path = $"{MaterialFolder}/{entry.Key}.mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(entry.Value, path);
            }
            AssetDatabase.SaveAssets();
        }

        static void MarkStatic(GameObject root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                // Uhrzeiger bewegen sich, TextMesh-Geometrie darf nicht statisch gebatcht werden
                if (t.GetComponentInParent<ClockHands>() != null || t.GetComponent<TextMesh>() != null) continue;
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
            }
        }

        static void AddSceneToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == ScenePath)) return;
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
