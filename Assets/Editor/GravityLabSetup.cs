using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using GravityLab.Core;
using GravityLab.Physics;
using GravityLab.Visualization;
using GravityLab.UI;
using GravityLab.Data;

namespace GravityLab.Editor
{
    /// <summary>
    /// Automated setup wizard and scene generator for the GravityLab simulator.
    /// Provides one-click procedural scene generation via menu item 'GravityLab -> Generate Demo Scene'.
    /// Creates the complete hierarchy, camera, lighting, 9-body solar system, starfield,
    /// and full scientific dashboard UI, and wires all serialized references.
    /// </summary>
    public static class GravityLabSetup
    {
        private static Font _defaultFont;

        [MenuItem("GravityLab/Generate Demo Scene", false, 1)]
        public static void GenerateDemoScene()
        {
            try
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayProgressBar("GravityLab Setup", "Creating simulation scene...", 0.1f);
                }

                // Step 1: Create or open MainSimulation.unity
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                string sceneDirectory = "Assets/Scenes";
                if (!AssetDatabase.IsValidFolder(sceneDirectory))
                {
                    AssetDatabase.CreateFolder("Assets", "Scenes");
                }
                string scenePath = $"{sceneDirectory}/MainSimulation.unity";

                // Resolve built-in font
                _defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_defaultFont == null) _defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

                if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("GravityLab Setup", "Setting up Environment & Lighting...", 0.2f);

                // Step 2: Camera & Controller
                GameObject cameraObj = new GameObject("Main Camera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.015f, 0.02f, 0.035f, 1f); // Deep cosmological dark
                cam.fieldOfView = 60f;
                cam.nearClipPlane = 0.3f;
                cam.farClipPlane = 3000f;
                cameraObj.tag = "MainCamera";
                cameraObj.AddComponent<AudioListener>();
                CameraController cameraController = cameraObj.AddComponent<CameraController>();

                // Step 3: Subtle Ambient Lighting
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.18f, 1.0f);

                GameObject dirLightObj = new GameObject("Subtle Ambient Light");
                Light dirLight = dirLightObj.AddComponent<Light>();
                dirLight.type = LightType.Directional;
                dirLight.color = new Color(0.35f, 0.38f, 0.45f);
                dirLight.intensity = 0.3f;
                dirLightObj.transform.rotation = Quaternion.Euler(45f, 45f, 0f);

                if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("GravityLab Setup", "Generating Starfield Generator...", 0.35f);

                // Step 4: Starfield
                GameObject starfieldObj = new GameObject("StarfieldGenerator");
                starfieldObj.AddComponent<StarFieldGenerator>();

                if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("GravityLab Setup", "Initializing Simulation Core...", 0.45f);

                // Step 5: Simulation Core Manager & Simulator
                GameObject coreObj = new GameObject("SimulationCore");
                GravitySimulator simulator = coreObj.AddComponent<GravitySimulator>();
                SimulationManager simManager = coreObj.AddComponent<SimulationManager>();

                // Step 6: Spawn 9 Solar System Bodies
                if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("GravityLab Setup", "Instantiating 9-Body Solar System Preset...", 0.6f);
                GameObject celestialSystemRoot = new GameObject("CelestialBodies");

                var presetList = SolarSystemPreset.GetSolarSystemPreset();
                List<CelestialBody> spawnedBodies = new List<CelestialBody>();

                for (int i = 0; i < presetList.Count; i++)
                {
                    var data = presetList[i];
                    GameObject bodyObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    bodyObj.name = data.name;
                    bodyObj.transform.SetParent(celestialSystemRoot.transform, false);

                    CelestialBody body = bodyObj.AddComponent<CelestialBody>();
                    body.InitializeFromData(data);
                    simulator.RegisterBody(body);
                    spawnedBodies.Add(body);
                }

                if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("GravityLab Setup", "Constructing Scientific UI Dashboard...", 0.75f);

                // Step 7: Build UI Hierarchy
                GameObject canvasObj = CreateCanvasWithEventSystem();
                Canvas canvas = canvasObj.GetComponent<Canvas>();

                // Construct UI components
                MainUIController mainUI = canvasObj.AddComponent<MainUIController>();
                BuildUIHierarchy(canvasObj, simManager, simulator, cameraController, mainUI, spawnedBodies);

                // Link Manager and Camera
                SerializedObject simManagerSo = new SerializedObject(simManager);
                simManagerSo.FindProperty("_simulator").objectReferenceValue = simulator;
                simManagerSo.FindProperty("_cameraController").objectReferenceValue = cameraController;
                simManagerSo.ApplyModifiedProperties();

                // Step 8: Save Scene & Register in Build Settings
                if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("GravityLab Setup", "Saving Scene & Registering Build Settings...", 0.95f);
                EditorSceneManager.SaveScene(scene, scenePath);

                List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                bool existsInBuild = false;
                for (int i = 0; i < buildScenes.Count; i++)
                {
                    if (buildScenes[i].path == scenePath)
                    {
                        existsInBuild = true;
                        buildScenes[i].enabled = true;
                        break;
                    }
                }
                if (!existsInBuild)
                {
                    buildScenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
                    EditorBuildSettings.scenes = buildScenes.ToArray();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!Application.isBatchMode)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog(
                        "GravityLab Setup Complete",
                        "The GravityLab demo scene has been generated successfully!\n\n" +
                        "Scene saved to: Assets/Scenes/MainSimulation.unity\n\n" +
                        "Press the ▶ Play button in Unity to run the simulation.",
                        "Start Exploring"
                    );
                }

                Debug.Log("[GravityLab::SETUP] Successfully generated MainSimulation.unity with full UI, 9-body physics, starfield, and camera controllers.");
            }
            catch (Exception ex)
            {
                if (!Application.isBatchMode) EditorUtility.ClearProgressBar();
                Debug.LogError($"[GravityLab::SETUP ERROR] Scene generation failed: {ex}");
            }
        }

        private static GameObject CreateCanvasWithEventSystem()
        {
            GameObject canvasObj = new GameObject("GravityLabCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Event System
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventObj = new GameObject("EventSystem");
                eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            return canvasObj;
        }

        private static void BuildUIHierarchy(
            GameObject canvasObj,
            SimulationManager simManager,
            GravitySimulator simulator,
            CameraController cameraController,
            MainUIController mainUI,
            List<CelestialBody> initialBodies)
        {
            Color panelBgColor = new Color(0.06f, 0.08f, 0.12f, 0.88f); // Translucent slate
            Color borderAccentColor = new Color(0.22f, 0.40f, 0.65f, 0.5f);
            Color textBrightColor = new Color(0.92f, 0.95f, 1.0f);
            Color textMutedColor = new Color(0.65f, 0.72f, 0.82f);
            Color cyanAccent = new Color(0.2f, 0.75f, 1.0f);

            // ==================== A. TOP BAR ====================
            GameObject topBar = CreateUIPanel(canvasObj, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(0f, 60f), new Color(0.04f, 0.06f, 0.09f, 0.95f));

            Text titleTxt = CreateUIText(topBar, "Title", "GRAVITYLAB", 26, FontStyle.Bold, cyanAccent, TextAnchor.MiddleLeft);
            SetRect(titleTxt.gameObject, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(25f, 0f), new Vector2(180f, 0f));

            Text subTxt = CreateUIText(topBar, "Subtitle", "Interactive Space & N-Body Gravity Simulator", 15, FontStyle.Normal, textMutedColor, TextAnchor.MiddleLeft);
            SetRect(subTxt.gameObject, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(215f, 0f), new Vector2(350f, 0f));

            // Status Badge
            GameObject statusBadge = CreateUIPanel(topBar, "StatusBadge", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-60f, 0f), new Vector2(140f, 36f), new Color(0.15f, 0.75f, 0.35f, 0.85f));
            Text statusTxt = CreateUIText(statusBadge, "StatusText", "● RUNNING", 16, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            StretchFull(statusTxt.gameObject);

            // Speed Badge
            Text speedTxt = CreateUIText(topBar, "SpeedBadge", "Speed: 1.0x", 16, FontStyle.Bold, textBrightColor, TextAnchor.MiddleCenter);
            SetRect(speedTxt.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(75f, 0f), new Vector2(120f, 36f));

            // Top Buttons
            Button addBodyTopBtn = CreateUIButton(topBar, "AddBodyTopBtn", "+ Add Body", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-205f, 0f), new Vector2(150f, 42f), new Color(0.15f, 0.45f, 0.75f, 0.9f));
            Button physicsTopBtn = CreateUIButton(topBar, "PhysicsTopBtn", "📖 Physics Theory", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-25f, 0f), new Vector2(165f, 42f), new Color(0.2f, 0.3f, 0.45f, 0.9f));

            // ==================== B. LEFT CONTROL PANEL ====================
            GameObject leftPanelObj = CreateUIPanel(canvasObj, "LeftControlPanel", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(15f, -5f), new Vector2(340f, -110f), panelBgColor);
            SimulationControlPanel controlPanel = leftPanelObj.AddComponent<SimulationControlPanel>();

            Text controlHeader = CreateUIText(leftPanelObj, "Header", "SIMULATION CONTROLS", 20, FontStyle.Bold, cyanAccent, TextAnchor.MiddleCenter);
            SetRect(controlHeader.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(0f, 36f));

            // Playback buttons
            Button playPauseBtn = CreateUIButton(leftPanelObj, "PlayPauseBtn", "⏸ Pause", new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(10f, -70f), new Vector2(-15f, 42f), new Color(0.18f, 0.5f, 0.8f));
            Text playPauseTxt = playPauseBtn.GetComponentInChildren<Text>();

            Button resetBtn = CreateUIButton(leftPanelObj, "ResetBtn", "↺ Reset", new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(5f, -70f), new Vector2(-15f, 42f), new Color(0.6f, 0.25f, 0.25f));

            // Sliders
            Slider speedSlider = CreateUISlider(leftPanelObj, "SpeedSlider", "Simulation Speed", 0.1f, 10.0f, 1.0f, -128f, out Text speedValTxt);
            Slider gravitySlider = CreateUISlider(leftPanelObj, "GravitySlider", "Gravity Strength (G)", 0.1f, 5.0f, 1.0f, -198f, out Text gravValTxt);
            Slider softeningSlider = CreateUISlider(leftPanelObj, "SofteningSlider", "Plummer Softening (ε)", 0.05f, 3.0f, 0.5f, -268f, out Text softValTxt);

            // Toggles
            Toggle trailsToggle = CreateUIToggle(leftPanelObj, "TrailsToggle", "Show Orbit Trails", true, -340f);
            Toggle camFollowToggle = CreateUIToggle(leftPanelObj, "CamFollowToggle", "Camera Track Planet", true, -378f);

            Button clearTrailsBtn = CreateUIButton(leftPanelObj, "ClearTrailsBtn", "Clear Orbit Trails", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -424f), new Vector2(-30f, 38f), new Color(0.2f, 0.25f, 0.35f));
            Button resetCamBtn = CreateUIButton(leftPanelObj, "ResetCamBtn", "Reset Camera View", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -472f), new Vector2(-30f, 38f), new Color(0.2f, 0.25f, 0.35f));

            // Mode switcher
            Text modeLabel = CreateUIText(leftPanelObj, "ModeLabel", "SIMULATION MODE", 16, FontStyle.Bold, textBrightColor, TextAnchor.MiddleLeft);
            SetRect(modeLabel.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(15f, -528f), new Vector2(-30f, 24f));

            Dropdown modeDropdown = CreateUIDropdown(leftPanelObj, "ModeDropdown", new string[] { "Solar System Mode", "Experiment Sandbox" }, -558f);

            GameObject expGroup = new GameObject("ExperimentPresetGroup");
            expGroup.transform.SetParent(leftPanelObj.transform, false);
            RectTransform expRt = expGroup.AddComponent<RectTransform>();
            expRt.anchorMin = new Vector2(0f, 1f);
            expRt.anchorMax = new Vector2(1f, 1f);
            expRt.pivot = new Vector2(0.5f, 1f);
            expRt.anchoredPosition = new Vector2(0f, -612f);
            expRt.sizeDelta = new Vector2(0f, 74f);
            expGroup.SetActive(false);

            Text expLabel = CreateUIText(expGroup, "ExpLabel", "EXPERIMENT PRESET", 15, FontStyle.Bold, textMutedColor, TextAnchor.MiddleLeft);
            SetRect(expLabel.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(15f, 0f), new Vector2(-30f, 20f));

            Dropdown expDropdown = CreateUIDropdown(expGroup, "ExpDropdown", new string[] { "Figure-8 (3-Body)", "Binary Star System", "Trojan Asteroids", "Blank Canvas" }, -26f);

            // ==================== C. RIGHT INFORMATION PANEL ====================
            GameObject rightPanelObj = CreateUIPanel(canvasObj, "RightInfoPanel", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-15f, -5f), new Vector2(400f, -110f), panelBgColor);
            PlanetInformationPanel infoPanel = rightPanelObj.AddComponent<PlanetInformationPanel>();

            Text infoHeader = CreateUIText(rightPanelObj, "Header", "CELESTIAL TELEMETRY", 20, FontStyle.Bold, cyanAccent, TextAnchor.MiddleCenter);
            SetRect(infoHeader.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(0f, 36f));

            // Quick select dropdown
            Dropdown bodyDropdown = CreateUIDropdown(rightPanelObj, "BodySelectDropdown", new string[] { "Select Body..." }, -60f);

            GameObject infoContent = new GameObject("TelemetryContent");
            infoContent.transform.SetParent(rightPanelObj.transform, false);
            RectTransform icRt = infoContent.AddComponent<RectTransform>();
            StretchFull(infoContent);
            icRt.offsetMax = new Vector2(-12f, -90f);
            icRt.offsetMin = new Vector2(12f, 12f);

            Text bodyTitle = CreateUIText(infoContent, "BodyTitle", "Earth", 24, FontStyle.Bold, textBrightColor, TextAnchor.MiddleLeft);
            SetRect(bodyTitle.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(0f, 32f));

            Text bodyType = CreateUIText(infoContent, "BodyType", "Type: Terrestrial Planet", 16, FontStyle.Italic, cyanAccent, TextAnchor.MiddleLeft);
            SetRect(bodyType.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(0f, 22f));

            Text descText = CreateUIText(infoContent, "Description", "Our home planet harboring life and oceans.", 15, FontStyle.Normal, textMutedColor, TextAnchor.UpperLeft);
            SetRect(descText.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(0f, 56f));

            // Telemetry Fields
            Text massTxt = CreateTelemetryRow(infoContent, "MassRow", "Mass:", "1.00 MU", -136f);
            Text radTxt = CreateTelemetryRow(infoContent, "RadiusRow", "Radius:", "0.95 SU", -178f);
            Text posTxt = CreateTelemetryRow(infoContent, "PosRow", "Position:", "(22.0, 0.0, 0.0) SU", -220f);
            Text velTxt = CreateTelemetryRow(infoContent, "VelRow", "Velocity:", "(0.0, 0.0, 21.3) SU/s", -262f);
            Text accTxt = CreateTelemetryRow(infoContent, "AccRow", "Accel:", "(0.0, 0.0, 0.0) SU/s²", -304f);
            Text sunDistTxt = CreateTelemetryRow(infoContent, "DistRow", "Distance to Sun:", "22.00 SU", -346f);
            Text gravForceTxt = CreateTelemetryRow(infoContent, "ForceRow", "Grav Force:", "0.00 N", -388f);

            Text factsHeader = CreateUIText(infoContent, "FactsHeader", "ASTRONOMICAL DATA", 16, FontStyle.Bold, textBrightColor, TextAnchor.MiddleLeft);
            SetRect(factsHeader.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -436f), new Vector2(0f, 28f));

            Text factsTxt = CreateUIText(infoContent, "FactsBody", "Real Mass: 5.97e24 kg\nReal Radius: 6371 km", 15, FontStyle.Normal, textMutedColor, TextAnchor.UpperLeft);
            SetRect(factsTxt.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -470f), new Vector2(0f, 95f));

            // Action Buttons
            Button focusBtn = CreateUIButton(infoContent, "FocusBtn", "🎯 Focus Camera (F)", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(0f, 38f), new Color(0.18f, 0.5f, 0.8f));
            Button deleteBtn = CreateUIButton(infoContent, "DeleteBtn", "Protected (Default)", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(0f, 38f), new Color(0.45f, 0.2f, 0.2f));
            Text deleteBtnTxt = deleteBtn.GetComponentInChildren<Text>();

            // Empty state placeholder
            GameObject noSelectObj = new GameObject("NoSelectionPrompt");
            noSelectObj.transform.SetParent(rightPanelObj.transform, false);
            StretchFull(noSelectObj);
            Text noSelectTxt = CreateUIText(noSelectObj, "NoSelectTxt", "Click any celestial body\nto view scientific telemetry.", 16, FontStyle.Italic, textMutedColor, TextAnchor.MiddleCenter);
            StretchFull(noSelectTxt.gameObject);
            noSelectObj.SetActive(false);

            // ==================== D. BOTTOM INFORMATION BAR ====================
            GameObject bottomBar = CreateUIPanel(canvasObj, "BottomBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(0f, 44f), new Color(0.04f, 0.06f, 0.09f, 0.95f));

            Text bodyCountTxt = CreateUIText(bottomBar, "BodyCount", "Bodies: 9", 15, FontStyle.Normal, textBrightColor, TextAnchor.MiddleLeft);
            SetRect(bodyCountTxt.gameObject, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(25f, 0f), new Vector2(140f, 0f));

            Text simTimeTxt = CreateUIText(bottomBar, "SimTime", "Sim Time: 0.0 Days", 15, FontStyle.Normal, textBrightColor, TextAnchor.MiddleLeft);
            SetRect(simTimeTxt.gameObject, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(180f, 0f), new Vector2(280f, 0f));

            Text timestepTxt = CreateUIText(bottomBar, "Timestep", "Δt: 0.016s", 15, FontStyle.Normal, textBrightColor, TextAnchor.MiddleLeft);
            SetRect(timestepTxt.gameObject, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(475f, 0f), new Vector2(140f, 0f));

            Text fpsTxt = CreateUIText(bottomBar, "FPS", "FPS: 60", 15, FontStyle.Normal, cyanAccent, TextAnchor.MiddleLeft);
            SetRect(fpsTxt.gameObject, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(630f, 0f), new Vector2(110f, 0f));

            Text energyTxt = CreateUIText(bottomBar, "Energy", "Energy: -4.52E+04 EU", 15, FontStyle.Normal, textMutedColor, TextAnchor.MiddleRight);
            SetRect(energyTxt.gameObject, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-25f, 0f), new Vector2(480f, 0f));

            // ==================== E. TOAST NOTIFICATION BANNER ====================
            GameObject toastRoot = CreateUIPanel(canvasObj, "ToastNotification", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -75f), new Vector2(560f, 44f), new Color(0.12f, 0.35f, 0.65f, 0.95f));
            Text toastTxt = CreateUIText(toastRoot, "ToastText", "Notification message", 16, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter);
            StretchFull(toastTxt.gameObject);
            toastRoot.SetActive(false);

            // ==================== F. BODY CREATION MODAL ====================
            GameObject creationModalObj = CreateUIPanel(canvasObj, "BodyCreationModal", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 680f), new Color(0.08f, 0.11f, 0.16f, 0.98f));
            BodyCreationPanel creationPanel = creationModalObj.AddComponent<BodyCreationPanel>();

            Text creationHeader = CreateUIText(creationModalObj, "Header", "CREATE CUSTOM CELESTIAL BODY", 22, FontStyle.Bold, cyanAccent, TextAnchor.MiddleCenter);
            SetRect(creationHeader.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(0f, 38f));

            InputField nameIn = CreateUIInputField(creationModalObj, "NameIn", "Body Name", "Planet_X", -75f);
            InputField massIn = CreateUIInputField(creationModalObj, "MassIn", "Mass (MU)", "1.0", -125f);
            InputField radIn = CreateUIInputField(creationModalObj, "RadIn", "Visual Radius (SU)", "0.8", -175f);

            Text posLabel = CreateUIText(creationModalObj, "PosLabel", "Initial Position (X, Y, Z in SU)", 15, FontStyle.Bold, textBrightColor, TextAnchor.MiddleLeft);
            SetRect(posLabel.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(35f, -220f), new Vector2(-70f, 22f));

            CreateVector3Inputs(creationModalObj, "Pos", "38.0", "0.0", "0.0", -255f, out InputField posXIn, out InputField posYIn, out InputField posZIn);

            Text velLabel = CreateUIText(creationModalObj, "VelLabel", "Initial Velocity (Vx, Vy, Vz in SU/s)", 15, FontStyle.Bold, textBrightColor, TextAnchor.MiddleLeft);
            SetRect(velLabel.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(35f, -295f), new Vector2(-70f, 22f));

            CreateVector3Inputs(creationModalObj, "Vel", "0.0", "0.0", "16.2", -330f, out InputField velXIn, out InputField velYIn, out InputField velZIn);

            Button autoOrbitBtn = CreateUIButton(creationModalObj, "AutoOrbitBtn", "⚡ Auto-Calculate Circular Orbital Velocity", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -385f), new Vector2(-70f, 36f), new Color(0.2f, 0.45f, 0.7f));

            // Color Selection
            Text colLabel = CreateUIText(creationModalObj, "ColLabel", "Display Color", 15, FontStyle.Bold, textBrightColor, TextAnchor.MiddleLeft);
            SetRect(colLabel.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(35f, -425f), new Vector2(-70f, 22f));

            GameObject colPreview = CreateUIPanel(creationModalObj, "ColorPreview", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(35f, -450f), new Vector2(36f, 36f), new Color(0.25f, 0.85f, 1.0f));
            Image colPreviewImg = colPreview.GetComponent<Image>();

            Button cCyan = CreateColorButton(creationModalObj, "CyanBtn", new Color(0.2f, 0.85f, 1.0f), 85f, -450f);
            Button cGreen = CreateColorButton(creationModalObj, "GreenBtn", new Color(0.3f, 0.95f, 0.45f), 130f, -450f);
            Button cRed = CreateColorButton(creationModalObj, "RedBtn", new Color(1.0f, 0.35f, 0.25f), 175f, -450f);
            Button cGold = CreateColorButton(creationModalObj, "GoldBtn", new Color(1.0f, 0.85f, 0.2f), 220f, -450f);
            Button cPurple = CreateColorButton(creationModalObj, "PurpleBtn", new Color(0.85f, 0.45f, 1.0f), 265f, -450f);
            Button cWhite = CreateColorButton(creationModalObj, "WhiteBtn", new Color(0.95f, 0.95f, 0.95f), 310f, -450f);

            Text feedbackTxt = CreateUIText(creationModalObj, "FeedbackTxt", "", 15, FontStyle.Normal, Color.yellow, TextAnchor.MiddleCenter);
            SetRect(feedbackTxt.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -505f), new Vector2(-70f, 26f));

            Button createBodyBtn = CreateUIButton(creationModalObj, "CreateBodyBtn", "✓ Add Celestial Body", new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(25f, 25f), new Vector2(-40f, 42f), new Color(0.15f, 0.65f, 0.35f));
            Button closeCreationBtn = CreateUIButton(creationModalObj, "CloseCreationBtn", "✕ Cancel", new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(15f, 25f), new Vector2(-40f, 42f), new Color(0.3f, 0.35f, 0.4f));

            creationModalObj.SetActive(false);

            // ==================== G. EDUCATIONAL PHYSICS MODAL ====================
            GameObject physicsModalObj = CreateUIPanel(canvasObj, "PhysicsEducationModal", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 720f), new Color(0.08f, 0.11f, 0.16f, 0.98f));
            PhysicsEducationPanel physicsPanel = physicsModalObj.AddComponent<PhysicsEducationPanel>();

            Text physicsHeader = CreateUIText(physicsModalObj, "Header", "THE PHYSICS BEHIND GRAVITYLAB", 22, FontStyle.Bold, cyanAccent, TextAnchor.MiddleCenter);
            SetRect(physicsHeader.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(0f, 38f));

            // Formula banner
            GameObject formulaBanner = CreateUIPanel(physicsModalObj, "FormulaBanner", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -75f), new Vector2(-60f, 70f), new Color(0.12f, 0.18f, 0.28f, 0.95f));
            Text formulaTxt = CreateUIText(formulaBanner, "FormulaText", "F = G · (m₁ · m₂) / r²", 17, FontStyle.Bold, cyanAccent, TextAnchor.MiddleCenter);
            StretchFull(formulaTxt.gameObject);

            // Variables legend
            GameObject legendBox = CreateUIPanel(physicsModalObj, "LegendBox", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(-60f, 90f), new Color(0.06f, 0.09f, 0.14f, 0.8f));
            Text legendTxt = CreateUIText(legendBox, "LegendText", "Variables Legend", 15, FontStyle.Normal, textBrightColor, TextAnchor.MiddleLeft);
            StretchFull(legendTxt.gameObject);
            RectTransform legRt = legendTxt.GetComponent<RectTransform>();
            legRt.offsetMin = new Vector2(15f, 5f);
            legRt.offsetMax = new Vector2(-15f, -5f);

            // Scrollable explanation area
            GameObject scrollRoot = new GameObject("ScrollArea");
            scrollRoot.transform.SetParent(physicsModalObj.transform, false);
            RectTransform scrollRt = scrollRoot.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(30f, 65f);
            scrollRt.offsetMax = new Vector2(-30f, -230f);

            ScrollRect scrollRect = scrollRoot.AddComponent<ScrollRect>();
            scrollRoot.AddComponent<RectMask2D>();

            GameObject scrollContent = new GameObject("Content");
            scrollContent.transform.SetParent(scrollRoot.transform, false);
            RectTransform sContentRt = scrollContent.AddComponent<RectTransform>();
            sContentRt.anchorMin = new Vector2(0f, 1f);
            sContentRt.anchorMax = new Vector2(1f, 1f);
            sContentRt.pivot = new Vector2(0.5f, 1f);
            sContentRt.sizeDelta = new Vector2(0f, 1200f);
            scrollRect.content = sContentRt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 25f;

            Text topicsTxt = CreateUIText(scrollContent, "TopicsText", "Physics Topics", 15, FontStyle.Normal, textBrightColor, TextAnchor.UpperLeft);
            StretchFull(topicsTxt.gameObject);

            Button closePhysicsBtn = CreateUIButton(physicsModalObj, "ClosePhysicsBtn", "✕ Close Physics Guide", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(-60f, 42f), new Color(0.2f, 0.4f, 0.65f));

            physicsModalObj.SetActive(false);

            // ==================== SERIALIZED PROPERTY WIRING ====================
            WireMainUIController(mainUI, titleTxt, subTxt, statusTxt, statusBadge.GetComponent<Image>(), speedTxt,
                bodyCountTxt, simTimeTxt, timestepTxt, fpsTxt, energyTxt,
                toastRoot, toastTxt, controlPanel, infoPanel, creationPanel, physicsPanel,
                addBodyTopBtn, physicsTopBtn);

            WireControlPanel(controlPanel, playPauseBtn, playPauseTxt, resetBtn, clearTrailsBtn, resetCamBtn,
                speedSlider, speedValTxt, gravitySlider, gravValTxt, softeningSlider, softValTxt,
                trailsToggle, camFollowToggle, modeDropdown, expDropdown, expGroup,
                addBodyTopBtn, physicsTopBtn);

            WireInfoPanel(infoPanel, bodyTitle, bodyType, massTxt, radTxt, posTxt, velTxt, accTxt,
                sunDistTxt, gravForceTxt, descText, factsTxt, focusBtn, deleteBtn, deleteBtnTxt,
                bodyDropdown, infoContent, noSelectObj);

            WireCreationPanel(creationPanel, creationModalObj, nameIn, massIn, radIn,
                posXIn, posYIn, posZIn, velXIn, velYIn, velZIn,
                colPreviewImg, cCyan, cGreen, cRed, cGold, cPurple, cWhite,
                autoOrbitBtn, createBodyBtn, closeCreationBtn, feedbackTxt);

            WirePhysicsPanel(physicsPanel, physicsModalObj, closePhysicsBtn, formulaTxt, legendTxt, topicsTxt);

            // Populate initial bodies in dropdown
            infoPanel.PopulateDropdown(initialBodies);
        }

        #region Serialized Wiring Helpers

        private static void WireMainUIController(
            MainUIController ui,
            Text title, Text sub, Text status, Image statusBg, Text speedBadge,
            Text bodyCount, Text simTime, Text timestep, Text fps, Text energy,
            GameObject toastRoot, Text toastTxt,
            SimulationControlPanel control, PlanetInformationPanel info,
            BodyCreationPanel creation, PhysicsEducationPanel physics,
            Button addTopBtn, Button physTopBtn)
        {
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("_titleText").objectReferenceValue = title;
            so.FindProperty("_subtitleText").objectReferenceValue = sub;
            so.FindProperty("_statusBadgeText").objectReferenceValue = status;
            so.FindProperty("_statusBadgeBg").objectReferenceValue = statusBg;
            so.FindProperty("_speedBadgeText").objectReferenceValue = speedBadge;

            so.FindProperty("_bodyCountText").objectReferenceValue = bodyCount;
            so.FindProperty("_simTimeText").objectReferenceValue = simTime;
            so.FindProperty("_timestepText").objectReferenceValue = timestep;
            so.FindProperty("_fpsText").objectReferenceValue = fps;
            so.FindProperty("_energyConservationText").objectReferenceValue = energy;

            so.FindProperty("_notificationRoot").objectReferenceValue = toastRoot;
            so.FindProperty("_notificationText").objectReferenceValue = toastTxt;

            so.FindProperty("_controlPanel").objectReferenceValue = control;
            so.FindProperty("_infoPanel").objectReferenceValue = info;
            so.FindProperty("_creationPanel").objectReferenceValue = creation;
            so.FindProperty("_physicsPanel").objectReferenceValue = physics;

            so.FindProperty("_openCreateBodyTopBtn").objectReferenceValue = addTopBtn;
            so.FindProperty("_openPhysicsGuideTopBtn").objectReferenceValue = physTopBtn;
            so.ApplyModifiedProperties();
        }

        private static void WireControlPanel(
            SimulationControlPanel cp,
            Button playPause, Text playPauseTxt, Button reset, Button clearTrails, Button resetCam,
            Slider speed, Text speedTxt, Slider grav, Text gravTxt, Slider soft, Text softTxt,
            Toggle trails, Toggle camFollow, Dropdown mode, Dropdown expPreset, GameObject expGroup,
            Button addBody, Button physGuide)
        {
            SerializedObject so = new SerializedObject(cp);
            so.FindProperty("_playPauseButton").objectReferenceValue = playPause;
            so.FindProperty("_playPauseText").objectReferenceValue = playPauseTxt;
            so.FindProperty("_resetButton").objectReferenceValue = reset;
            so.FindProperty("_clearTrailsButton").objectReferenceValue = clearTrails;
            so.FindProperty("_resetCameraButton").objectReferenceValue = resetCam;

            so.FindProperty("_speedSlider").objectReferenceValue = speed;
            so.FindProperty("_speedValueText").objectReferenceValue = speedTxt;
            so.FindProperty("_gravitySlider").objectReferenceValue = grav;
            so.FindProperty("_gravityValueText").objectReferenceValue = gravTxt;
            so.FindProperty("_softeningSlider").objectReferenceValue = soft;
            so.FindProperty("_softeningValueText").objectReferenceValue = softTxt;

            so.FindProperty("_trailsToggle").objectReferenceValue = trails;
            so.FindProperty("_cameraFollowToggle").objectReferenceValue = camFollow;

            so.FindProperty("_modeDropdown").objectReferenceValue = mode;
            so.FindProperty("_experimentPresetDropdown").objectReferenceValue = expPreset;
            so.FindProperty("_experimentPresetGroup").objectReferenceValue = expGroup;

            so.FindProperty("_openCreateBodyButton").objectReferenceValue = addBody;
            so.FindProperty("_openPhysicsGuideButton").objectReferenceValue = physGuide;
            so.ApplyModifiedProperties();
        }

        private static void WireInfoPanel(
            PlanetInformationPanel ip,
            Text title, Text type, Text mass, Text radius, Text pos, Text vel, Text acc,
            Text sunDist, Text force, Text desc, Text facts,
            Button focus, Button del, Text delTxt,
            Dropdown dropdown, GameObject content, GameObject noSelect)
        {
            SerializedObject so = new SerializedObject(ip);
            so.FindProperty("_titleText").objectReferenceValue = title;
            so.FindProperty("_bodyTypeText").objectReferenceValue = type;
            so.FindProperty("_massText").objectReferenceValue = mass;
            so.FindProperty("_radiusText").objectReferenceValue = radius;
            so.FindProperty("_positionText").objectReferenceValue = pos;
            so.FindProperty("_velocityText").objectReferenceValue = vel;
            so.FindProperty("_accelerationText").objectReferenceValue = acc;
            so.FindProperty("_distanceToSunText").objectReferenceValue = sunDist;
            so.FindProperty("_gravitationalForceText").objectReferenceValue = force;
            so.FindProperty("_descriptionText").objectReferenceValue = desc;
            so.FindProperty("_astronomicalFactsText").objectReferenceValue = facts;

            so.FindProperty("_focusButton").objectReferenceValue = focus;
            so.FindProperty("_deleteButton").objectReferenceValue = del;
            so.FindProperty("_deleteButtonText").objectReferenceValue = delTxt;

            so.FindProperty("_bodyDropdown").objectReferenceValue = dropdown;
            so.FindProperty("_contentContainer").objectReferenceValue = content;
            so.FindProperty("_noSelectionPrompt").objectReferenceValue = noSelect;
            so.ApplyModifiedProperties();
        }

        private static void WireCreationPanel(
            BodyCreationPanel bcp,
            GameObject root, InputField name, InputField mass, InputField rad,
            InputField px, InputField py, InputField pz,
            InputField vx, InputField vy, InputField vz,
            Image preview, Button c1, Button c2, Button c3, Button c4, Button c5, Button c6,
            Button autoOrbit, Button create, Button close, Text feedback)
        {
            SerializedObject so = new SerializedObject(bcp);
            so.FindProperty("_panelRoot").objectReferenceValue = root;
            so.FindProperty("_nameInput").objectReferenceValue = name;
            so.FindProperty("_massInput").objectReferenceValue = mass;
            so.FindProperty("_radiusInput").objectReferenceValue = rad;

            so.FindProperty("_posXInput").objectReferenceValue = px;
            so.FindProperty("_posYInput").objectReferenceValue = py;
            so.FindProperty("_posZInput").objectReferenceValue = pz;

            so.FindProperty("_velXInput").objectReferenceValue = vx;
            so.FindProperty("_velYInput").objectReferenceValue = vy;
            so.FindProperty("_velZInput").objectReferenceValue = vz;

            so.FindProperty("_colorPreviewImage").objectReferenceValue = preview;
            so.FindProperty("_colorCyanBtn").objectReferenceValue = c1;
            so.FindProperty("_colorGreenBtn").objectReferenceValue = c2;
            so.FindProperty("_colorRedBtn").objectReferenceValue = c3;
            so.FindProperty("_colorGoldBtn").objectReferenceValue = c4;
            so.FindProperty("_colorPurpleBtn").objectReferenceValue = c5;
            so.FindProperty("_colorWhiteBtn").objectReferenceValue = c6;

            so.FindProperty("_autoOrbitButton").objectReferenceValue = autoOrbit;
            so.FindProperty("_createButton").objectReferenceValue = create;
            so.FindProperty("_closeButton").objectReferenceValue = close;
            so.FindProperty("_feedbackText").objectReferenceValue = feedback;
            so.ApplyModifiedProperties();
        }

        private static void WirePhysicsPanel(
            PhysicsEducationPanel pep,
            GameObject root, Button close, Text formula, Text legend, Text topics)
        {
            SerializedObject so = new SerializedObject(pep);
            so.FindProperty("_panelRoot").objectReferenceValue = root;
            so.FindProperty("_closeButton").objectReferenceValue = close;
            so.FindProperty("_formulaBannerText").objectReferenceValue = formula;
            so.FindProperty("_variablesLegendText").objectReferenceValue = legend;
            so.FindProperty("_topicsBodyText").objectReferenceValue = topics;
            so.ApplyModifiedProperties();
        }

        #endregion

        #region Procedural UI Primitives Generators

        private static GameObject CreateUIPanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = obj.AddComponent<Image>();
            img.color = color;
            return obj;
        }

        private static Text CreateUIText(GameObject parent, string name, string content, int fontSize, FontStyle style, Color color, TextAnchor alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            Text text = obj.AddComponent<Text>();
            text.font = _defaultFont;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateUIButton(GameObject parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = obj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = obj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = bgColor * 1.25f;
            cb.pressedColor = bgColor * 0.8f;
            btn.colors = cb;

            Text txt = CreateUIText(obj, "Label", label, 16, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            StretchFull(txt.gameObject);

            return btn;
        }

        private static Slider CreateUISlider(GameObject parent, string name, string label, float min, float max, float def, float yOffset, out Text valueTxt)
        {
            GameObject group = new GameObject(name + "_Group");
            group.transform.SetParent(parent.transform, false);
            RectTransform grpRt = group.AddComponent<RectTransform>();
            grpRt.anchorMin = new Vector2(0f, 1f);
            grpRt.anchorMax = new Vector2(1f, 1f);
            grpRt.pivot = new Vector2(0.5f, 1f);
            grpRt.anchoredPosition = new Vector2(0f, yOffset);
            grpRt.sizeDelta = new Vector2(-30f, 56f);

            Text lbl = CreateUIText(group, "Label", label, 16, FontStyle.Normal, new Color(0.85f, 0.9f, 0.95f), TextAnchor.MiddleLeft);
            SetRect(lbl.gameObject, new Vector2(0f, 1f), new Vector2(0.7f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(0f, 24f));

            valueTxt = CreateUIText(group, "Value", $"{def:F1}", 16, FontStyle.Bold, new Color(0.35f, 0.8f, 1.0f), TextAnchor.MiddleRight);
            SetRect(valueTxt.gameObject, new Vector2(0.7f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 24f));

            // Slider Background track
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(group.transform, false);
            RectTransform sRt = sliderObj.AddComponent<RectTransform>();
            SetRect(sliderObj, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(0f, 18f));

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = def;

            GameObject bg = CreateUIPanel(sliderObj, "Background", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.15f, 0.2f, 0.28f));

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0f, 0.25f);
            faRt.anchorMax = new Vector2(1f, 0.75f);
            faRt.sizeDelta = new Vector2(-10f, 0f);

            GameObject fill = CreateUIPanel(fillArea, "Fill", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.2f, 0.65f, 0.95f));

            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform haRt = handleArea.AddComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = new Vector2(-10f, 0f);

            GameObject handle = CreateUIPanel(handleArea, "Handle", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(16f, 22f), Color.white);

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();

            return slider;
        }

        private static Toggle CreateUIToggle(GameObject parent, string name, string label, bool defVal, float yOffset)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, yOffset);
            rt.sizeDelta = new Vector2(-30f, 30f);

            Toggle toggle = obj.AddComponent<Toggle>();
            toggle.isOn = defVal;

            GameObject bg = CreateUIPanel(obj, "Background", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(20f, 20f), new Color(0.18f, 0.24f, 0.32f));

            GameObject check = CreateUIPanel(bg, "Checkmark", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(12f, 12f), new Color(0.25f, 0.85f, 1.0f));

            Text lbl = CreateUIText(obj, "Label", label, 16, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            SetRect(lbl.gameObject, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(-28f, 0f));

            toggle.graphic = check.GetComponent<Image>();
            toggle.targetGraphic = bg.GetComponent<Image>();

            return toggle;
        }

        private static Dropdown CreateUIDropdown(GameObject parent, string name, string[] options, float yOffset)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, yOffset);
            rt.sizeDelta = new Vector2(-30f, 38f);

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.14f, 0.2f, 0.3f);

            Dropdown dd = obj.AddComponent<Dropdown>();

            Text label = CreateUIText(obj, "Label", options.Length > 0 ? options[0] : "", 16, FontStyle.Normal, new Color(0.92f, 0.96f, 1.0f, 1.0f), TextAnchor.MiddleLeft);
            SetRect(label.gameObject, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(-25f, 0f));

            Text arrow = CreateUIText(obj, "Arrow", "▼", 14, FontStyle.Normal, new Color(0.7f, 0.8f, 0.9f), TextAnchor.MiddleCenter);
            SetRect(arrow.gameObject, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-10f, 0f), new Vector2(18f, 18f));

            // Template container (dark slate border)
            GameObject template = CreateUIPanel(obj, "Template", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 160f), new Color(0.12f, 0.16f, 0.24f, 1.0f));
            ScrollRect sr = template.AddComponent<ScrollRect>();

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            StretchFull(viewport);
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false; // Do not render mask graphic as a solid white box
            Image vpImg = viewport.AddComponent<Image>();
            vpImg.color = new Color(0.95f, 0.96f, 0.98f, 1.0f);

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform cRt = content.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.sizeDelta = new Vector2(0f, 160f);

            GameObject item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            RectTransform itemRt = item.AddComponent<RectTransform>();
            itemRt.anchorMin = new Vector2(0f, 1f);
            itemRt.anchorMax = new Vector2(1f, 1f);
            itemRt.pivot = new Vector2(0.5f, 1f);
            itemRt.sizeDelta = new Vector2(0f, 32f);

            // Item background (contrasting light grey/white)
            Image itemImg = item.AddComponent<Image>();
            itemImg.color = new Color(0.95f, 0.96f, 0.98f, 1.0f);

            Toggle itemToggle = item.AddComponent<Toggle>();
            itemToggle.targetGraphic = itemImg;
            ColorBlock cb = itemToggle.colors;
            cb.normalColor = new Color(0.95f, 0.96f, 0.98f, 1.0f);      // Light grey
            cb.highlightedColor = new Color(0.82f, 0.88f, 0.96f, 1.0f); // Soft light blue hover
            cb.pressedColor = new Color(0.72f, 0.80f, 0.92f, 1.0f);     // Darker blue on press
            cb.selectedColor = new Color(0.78f, 0.86f, 0.98f, 1.0f);    // Selected item accent
            cb.disabledColor = new Color(0.85f, 0.85f, 0.85f, 0.5f);
            cb.colorMultiplier = 1.0f;
            cb.fadeDuration = 0.1f;
            itemToggle.colors = cb;

            // Item text (dark navy on light grey background)
            Text itemText = CreateUIText(item, "Item Label", "Option", 16, FontStyle.Normal, new Color(0.06f, 0.09f, 0.16f, 1.0f), TextAnchor.MiddleLeft);
            SetRect(itemText.gameObject, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(-12f, 0f));

            template.SetActive(false);

            dd.template = template.GetComponent<RectTransform>();
            dd.captionText = label;
            dd.itemText = itemText;

            dd.ClearOptions();
            dd.AddOptions(new List<string>(options));

            return dd;
        }

        private static InputField CreateUIInputField(GameObject parent, string name, string label, string def, float yOffset)
        {
            GameObject group = new GameObject(name + "_Group");
            group.transform.SetParent(parent.transform, false);
            RectTransform grpRt = group.AddComponent<RectTransform>();
            grpRt.anchorMin = new Vector2(0f, 1f);
            grpRt.anchorMax = new Vector2(1f, 1f);
            grpRt.pivot = new Vector2(0.5f, 1f);
            grpRt.anchoredPosition = new Vector2(0f, yOffset);
            grpRt.sizeDelta = new Vector2(-70f, 36f);

            Text lbl = CreateUIText(group, "Label", label, 16, FontStyle.Normal, new Color(0.85f, 0.9f, 0.95f), TextAnchor.MiddleLeft);
            SetRect(lbl.gameObject, new Vector2(0f, 0f), new Vector2(0.45f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);

            GameObject inputObj = CreateUIPanel(group, name, new Vector2(0.48f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.14f, 0.18f, 0.25f));
            InputField input = inputObj.AddComponent<InputField>();

            Text txt = CreateUIText(inputObj, "Text", def, 15, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            StretchFull(txt.gameObject);
            RectTransform txtRt = txt.GetComponent<RectTransform>();
            txtRt.offsetMin = new Vector2(8f, 2f);
            txtRt.offsetMax = new Vector2(-8f, -2f);

            input.textComponent = txt;
            input.text = def;

            return input;
        }

        private static void CreateVector3Inputs(GameObject parent, string prefix, string defX, string defY, string defZ, float yOffset, out InputField inX, out InputField inY, out InputField inZ)
        {
            GameObject row = new GameObject(prefix + "_Row");
            row.transform.SetParent(parent.transform, false);
            RectTransform rowRt = row.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.anchoredPosition = new Vector2(0f, yOffset);
            rowRt.sizeDelta = new Vector2(-70f, 36f);

            inX = CreateSingleVectorInput(row, prefix + "X", "X:", defX, 0.0f, 0.31f);
            inY = CreateSingleVectorInput(row, prefix + "Y", "Y:", defY, 0.34f, 0.65f);
            inZ = CreateSingleVectorInput(row, prefix + "Z", "Z:", defZ, 0.68f, 1.0f);
        }

        private static InputField CreateSingleVectorInput(GameObject parent, string name, string label, string def, float minAnchor, float maxAnchor)
        {
            GameObject obj = CreateUIPanel(parent, name, new Vector2(minAnchor, 0f), new Vector2(maxAnchor, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.14f, 0.18f, 0.25f));

            Text lbl = CreateUIText(obj, "Label", label, 15, FontStyle.Bold, new Color(0.5f, 0.7f, 0.9f), TextAnchor.MiddleLeft);
            SetRect(lbl.gameObject, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(22f, 0f));

            InputField input = obj.AddComponent<InputField>();
            Text txt = CreateUIText(obj, "Text", def, 15, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            StretchFull(txt.gameObject);
            RectTransform txtRt = txt.GetComponent<RectTransform>();
            txtRt.offsetMin = new Vector2(28f, 2f);
            txtRt.offsetMax = new Vector2(-4f, -2f);

            input.textComponent = txt;
            input.text = def;

            return input;
        }

        private static Button CreateColorButton(GameObject parent, string name, Color color, float posX, float posY)
        {
            GameObject obj = CreateUIPanel(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(posX, posY), new Vector2(36f, 36f), color);
            Button btn = obj.AddComponent<Button>();
            return btn;
        }

        private static Text CreateTelemetryRow(GameObject parent, string name, string label, string defValue, float yOffset)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent.transform, false);
            RectTransform rRt = row.AddComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0f, 1f);
            rRt.anchorMax = new Vector2(1f, 1f);
            rRt.pivot = new Vector2(0.5f, 1f);
            rRt.anchoredPosition = new Vector2(0f, yOffset);
            rRt.sizeDelta = new Vector2(0f, 36f);

            Text lbl = CreateUIText(row, "Label", label, 16, FontStyle.Normal, new Color(0.65f, 0.75f, 0.85f), TextAnchor.UpperLeft);
            SetRect(lbl.gameObject, new Vector2(0f, 0f), new Vector2(0.42f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);

            Text val = CreateUIText(row, "Value", defValue, 16, FontStyle.Bold, Color.white, TextAnchor.UpperLeft);
            SetRect(val.gameObject, new Vector2(0.42f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);

            return val;
        }

        private static void StretchFull(GameObject obj)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static void SetRect(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }

        #endregion
    }
}
