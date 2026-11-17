using System;
using System.Collections.Generic;
using UnityEngine;
using SimulationMode = GravityLab.Core.SimulationMode;
using GravityLab.Data;
using GravityLab.Physics;
using GravityLab.Visualization;
using GravityLab.Utilities;

namespace GravityLab.Core
{
    public enum SimulationMode
    {
        SolarSystem,
        Experiment
    }

    public enum ExperimentPreset
    {
        FigureEightThreeBody,
        BinaryStarSystem,
        TrojanAsteroids,
        BlankSandbox
    }

    /// <summary>
    /// Central coordinator for the GravityLab application.
    /// Manages high-level modes (Solar System vs Experiment), celestial body selection,
    /// preset instantiation, procedural body creation, and UI notification events.
    /// </summary>
    [DisallowMultipleComponent]
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance { get; private set; }

        [Header("Subsystems")]
        [SerializeField] private GravitySimulator _simulator;
        [SerializeField] private CameraController _cameraController;

        [Header("Current Mode & Selection")]
        [SerializeField] private SimulationMode _currentMode = SimulationMode.SolarSystem;
        [SerializeField] private CelestialBody _selectedBody;

        // Events
        public event Action<CelestialBody> OnBodySelectionChanged;
        public event Action<SimulationMode> OnModeChanged;
        public event Action<string> OnNotification;

        public GravitySimulator Simulator => _simulator;
        public CameraController Camera => _cameraController;
        public SimulationMode CurrentMode => _currentMode;
        public CelestialBody SelectedBody => _selectedBody;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_simulator == null) _simulator = GetComponent<GravitySimulator>();
            if (_cameraController == null) _cameraController = FindFirstObjectByType<CameraController>();
        }

        private void Start()
        {
            // Subscribe to raycast clicks
            RegisterBodyClickEvents();

            // Auto-select Earth or Sun by default
            if (_selectedBody == null && _simulator != null && _simulator.ActiveBodies.Count > 0)
            {
                SelectBodyByName("Earth");
                if (_selectedBody == null) SelectBody(_simulator.ActiveBodies[0]);
            }
        }

        private void Update()
        {
            // Raycast selection when clicking into 3D view
            if (Input.GetMouseButtonDown(0))
            {
                CheckRaycastSelection();
            }

            // Keyboard shortcut 'F' to focus camera
            if (Input.GetKeyDown(KeyCode.F))
            {
                FocusCameraOnSelected();
            }

            // Spacebar toggles pause
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_simulator != null && _simulator.Clock != null)
                {
                    _simulator.Clock.TogglePause();
                }
            }
        }

        private void CheckRaycastSelection()
        {
            // If pointer is over UI, do not raycast 3D scene
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (UnityEngine.Camera.main == null) return;

            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, 2000f))
            {
                CelestialBody body = hit.collider.GetComponentInParent<CelestialBody>();
                if (body != null)
                {
                    SelectBody(body);
                }
            }
        }

        private void RegisterBodyClickEvents()
        {
            if (_simulator == null) return;
            for (int i = 0; i < _simulator.ActiveBodies.Count; i++)
            {
                var body = _simulator.ActiveBodies[i];
                if (body != null)
                {
                    body.OnBodyClicked += SelectBody;
                }
            }

            _simulator.OnBodyAdded += b => { if (b != null) b.OnBodyClicked += SelectBody; };
        }

        /// <summary>
        /// Selects a celestial body, highlights it, and notifies the UI.
        /// </summary>
        public void SelectBody(CelestialBody body)
        {
            if (_selectedBody == body && body != null) return;

            // Remove highlight on previously selected body
            if (_selectedBody != null && _selectedBody.Visuals != null)
            {
                _selectedBody.Visuals.SetSelected(false);
            }

            _selectedBody = body;

            // Apply highlight to new selection
            if (_selectedBody != null && _selectedBody.Visuals != null)
            {
                _selectedBody.Visuals.SetSelected(true);
            }

            OnBodySelectionChanged?.Invoke(_selectedBody);
        }

        /// <summary>
        /// Selects a celestial body by case-insensitive name.
        /// </summary>
        public bool SelectBodyByName(string name)
        {
            if (_simulator == null) return false;
            for (int i = 0; i < _simulator.ActiveBodies.Count; i++)
            {
                var b = _simulator.ActiveBodies[i];
                if (b != null && string.Equals(b.BodyName, name, StringComparison.OrdinalIgnoreCase))
                {
                    SelectBody(b);
                    return true;
                }
            }
            return false;
        }

        public void DeselectBody()
        {
            SelectBody(null);
        }

        public void FocusCameraOnSelected()
        {
            if (_cameraController != null && _selectedBody != null)
            {
                _cameraController.FocusOnBody(_selectedBody);
            }
        }

        public void ResetCameraView()
        {
            if (_cameraController != null)
            {
                _cameraController.ResetView();
            }
        }

        /// <summary>
        /// Switches between Solar System Mode and Experiment Mode.
        /// </summary>
        public void SwitchMode(SimulationMode newMode)
        {
            _currentMode = newMode;
            OnModeChanged?.Invoke(newMode);

            if (newMode == SimulationMode.SolarSystem)
            {
                LoadSolarSystemMode();
            }
            else
            {
                LoadExperimentPreset(ExperimentPreset.FigureEightThreeBody);
            }
        }

        /// <summary>
        /// Loads the standard 9-body Solar System preset.
        /// </summary>
        public void LoadSolarSystemMode()
        {
            _simulator.ClearAllBodies();
            var presetData = SolarSystemPreset.GetSolarSystemPreset();
            SpawnPresetBodies(presetData);

            _cameraController.ResetView();
            SelectBodyByName("Earth");
            OnNotification?.Invoke("Loaded Solar System Mode (9 planetary bodies).");
        }

        /// <summary>
        /// Loads an experimental gravitational choreography preset.
        /// </summary>
        public void LoadExperimentPreset(ExperimentPreset preset)
        {
            _simulator.ClearAllBodies();
            List<CelestialBodyData> dataList;

            switch (preset)
            {
                case ExperimentPreset.FigureEightThreeBody:
                    dataList = SolarSystemPreset.GetFigureEightPreset();
                    OnNotification?.Invoke("Loaded Chenciner-Montgomery Figure-8 3-Body Choreography.");
                    break;
                case ExperimentPreset.BinaryStarSystem:
                    dataList = SolarSystemPreset.GetBinaryStarPreset();
                    OnNotification?.Invoke("Loaded Binary Star System with Circumbinary Planet.");
                    break;
                case ExperimentPreset.TrojanAsteroids:
                    dataList = SolarSystemPreset.GetTrojanAsteroidsPreset();
                    OnNotification?.Invoke("Loaded Sun-Jupiter Lagrange L4/L5 Trojan Asteroids.");
                    break;
                case ExperimentPreset.BlankSandbox:
                default:
                    // Spawn central reference star
                    dataList = new List<CelestialBodyData>
                    {
                        new CelestialBodyData("Central Star", BodyType.Star, 8000f, 3.2f, Vector3.zero, Vector3.zero, new Color(1f, 0.85f, 0.25f), "Central gravitational reference anchor.", isProtected: true)
                    };
                    OnNotification?.Invoke("Loaded Empty Sandbox. Add custom celestial bodies!");
                    break;
            }

            SpawnPresetBodies(dataList);
            _cameraController.ResetView();
            if (_simulator.ActiveBodies.Count > 0)
            {
                SelectBody(_simulator.ActiveBodies[0]);
            }
        }

        private void SpawnPresetBodies(List<CelestialBodyData> dataList)
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                SpawnBodyFromData(dataList[i]);
            }
        }

        /// <summary>
        /// Instantiates a GameObject, attaches required physics/visualization components,
        /// initializes from CelestialBodyData, and registers it with the simulator.
        /// </summary>
        public CelestialBody SpawnBodyFromData(CelestialBodyData data)
        {
            GameObject bodyObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bodyObj.name = data.name;
            bodyObj.layer = 0;

            CelestialBody body = bodyObj.AddComponent<CelestialBody>();
            body.InitializeFromData(data);

            _simulator.RegisterBody(body);
            return body;
        }

        /// <summary>
        /// Spawns a user-created custom body into the simulation and selects it.
        /// </summary>
        public CelestialBody CreateCustomBody(
            string name,
            float mass,
            float radius,
            Vector3 position,
            Vector3 velocity,
            Color color)
        {
            // Input Validation
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"Body_{_simulator.BodyCount + 1}";
            }

            mass = Mathf.Max(0.001f, mass);
            radius = Mathf.Max(0.1f, radius);

            CelestialBodyData data = new CelestialBodyData(
                name: name.Trim(),
                type: BodyType.Custom,
                mass: mass,
                visualRadius: radius,
                position: position,
                velocity: velocity,
                color: color,
                description: "Custom user-generated celestial body.",
                isProtected: false
            );

            CelestialBody newBody = SpawnBodyFromData(data);
            SelectBody(newBody);
            _cameraController.FocusOnBody(newBody);

            OnNotification?.Invoke($"Created celestial body: {newBody.BodyName}");
            return newBody;
        }

        /// <summary>
        /// Deletes a celestial body from the simulation if it is not protected.
        /// </summary>
        public bool DeleteBody(CelestialBody body)
        {
            if (body == null) return false;

            if (body.IsProtected)
            {
                OnNotification?.Invoke($"Cannot delete default solar system body: {body.BodyName}");
                return false;
            }

            if (_selectedBody == body)
            {
                SelectBody(null);
            }

            string bName = body.BodyName;
            bool removed = _simulator.UnregisterBody(body);
            if (removed)
            {
                Destroy(body.gameObject);
                OnNotification?.Invoke($"Deleted celestial body: {bName}");
            }
            return removed;
        }
    }
}
