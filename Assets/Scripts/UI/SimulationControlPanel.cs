using UnityEngine;
using UnityEngine.UI;
using GravityLab.Core;
using SimulationMode = GravityLab.Core.SimulationMode;

namespace GravityLab.UI
{
    /// <summary>
    /// Controls simulation execution state, temporal speed, gravitational constant G,
    /// orbit trail toggles, mode switching, and modal dialog invocation.
    /// </summary>
    public class SimulationControlPanel : MonoBehaviour
    {
        [Header("Playback Buttons")]
        [SerializeField] private Button _playPauseButton;
        [SerializeField] private Text _playPauseText;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _clearTrailsButton;
        [SerializeField] private Button _resetCameraButton;

        [Header("Simulation Sliders")]
        [SerializeField] private Slider _speedSlider;
        [SerializeField] private Text _speedValueText;
        [SerializeField] private Slider _gravitySlider;
        [SerializeField] private Text _gravityValueText;
        [SerializeField] private Slider _softeningSlider;
        [SerializeField] private Text _softeningValueText;

        [Header("Toggles & View Options")]
        [SerializeField] private Toggle _trailsToggle;
        [SerializeField] private Toggle _cameraFollowToggle;

        [Header("Mode & Preset Controls")]
        [SerializeField] private Dropdown _modeDropdown;
        [SerializeField] private Dropdown _experimentPresetDropdown;
        [SerializeField] private GameObject _experimentPresetGroup;

        [Header("Modal Triggers")]
        [SerializeField] private Button _openCreateBodyButton;
        [SerializeField] private Button _openPhysicsGuideButton;

        private void Start()
        {
            ApplyDropdownStyling(_modeDropdown);
            ApplyDropdownStyling(_experimentPresetDropdown);
            InitializeListeners();
            SyncUIWithSimulationSettings();
        }

        private void ApplyDropdownStyling(Dropdown dd)
        {
            if (dd == null) return;
            if (dd.captionText != null) dd.captionText.color = new Color(0.92f, 0.96f, 1.0f, 1f);

            RectTransform template = dd.template;
            if (template == null) return;

            Image templateImg = template.GetComponent<Image>();
            if (templateImg != null) templateImg.color = new Color(0.12f, 0.16f, 0.24f, 1f);

            Transform viewport = template.Find("Viewport");
            if (viewport != null)
            {
                Mask mask = viewport.GetComponent<Mask>();
                if (mask != null) mask.showMaskGraphic = false;

                Image vpImg = viewport.GetComponent<Image>();
                if (vpImg != null) vpImg.color = new Color(0.95f, 0.96f, 0.98f, 1f);

                Transform content = viewport.Find("Content");
                if (content != null)
                {
                    Transform item = content.Find("Item");
                    if (item != null)
                    {
                        Image itemBg = item.GetComponent<Image>();
                        if (itemBg == null) itemBg = item.gameObject.AddComponent<Image>();
                        itemBg.color = new Color(0.95f, 0.96f, 0.98f, 1f);

                        Toggle itemToggle = item.GetComponent<Toggle>();
                        if (itemToggle != null)
                        {
                            itemToggle.targetGraphic = itemBg;
                            ColorBlock cb = itemToggle.colors;
                            cb.normalColor = new Color(0.95f, 0.96f, 0.98f, 1f);
                            cb.highlightedColor = new Color(0.82f, 0.88f, 0.96f, 1f);
                            cb.pressedColor = new Color(0.72f, 0.80f, 0.92f, 1f);
                            cb.selectedColor = new Color(0.78f, 0.86f, 0.98f, 1f);
                            itemToggle.colors = cb;
                        }

                        if (dd.itemText != null)
                        {
                            dd.itemText.color = new Color(0.06f, 0.09f, 0.16f, 1f);
                        }
                    }
                }
            }
        }

        private void InitializeListeners()
        {
            if (_playPauseButton != null) _playPauseButton.onClick.AddListener(OnPlayPauseClicked);
            if (_resetButton != null) _resetButton.onClick.AddListener(OnResetClicked);
            if (_clearTrailsButton != null) _clearTrailsButton.onClick.AddListener(OnClearTrailsClicked);
            if (_resetCameraButton != null) _resetCameraButton.onClick.AddListener(OnResetCameraClicked);

            if (_speedSlider != null) _speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            if (_gravitySlider != null) _gravitySlider.onValueChanged.AddListener(OnGravityChanged);
            if (_softeningSlider != null) _softeningSlider.onValueChanged.AddListener(OnSofteningChanged);

            if (_trailsToggle != null) _trailsToggle.onValueChanged.AddListener(OnTrailsToggled);
            if (_cameraFollowToggle != null) _cameraFollowToggle.onValueChanged.AddListener(OnCameraFollowToggled);

            if (_modeDropdown != null) _modeDropdown.onValueChanged.AddListener(OnModeChanged);
            if (_experimentPresetDropdown != null) _experimentPresetDropdown.onValueChanged.AddListener(OnExperimentPresetChanged);

            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                SimulationManager.Instance.Simulator.Clock.OnPauseStateChanged += UpdatePlayPauseButtonUI;
            }
        }

        private void OnDestroy()
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                SimulationManager.Instance.Simulator.Clock.OnPauseStateChanged -= UpdatePlayPauseButtonUI;
            }
        }

        public void SyncUIWithSimulationSettings()
        {
            if (SimulationManager.Instance == null || SimulationManager.Instance.Simulator == null) return;
            var settings = SimulationManager.Instance.Simulator.Settings;

            if (_speedSlider != null)
            {
                _speedSlider.minValue = 0.1f;
                _speedSlider.maxValue = 10.0f;
                _speedSlider.SetValueWithoutNotify(settings.timeScale);
                if (_speedValueText != null) _speedValueText.text = $"{settings.timeScale:F1}x";
            }

            if (_gravitySlider != null)
            {
                _gravitySlider.minValue = 0.1f;
                _gravitySlider.maxValue = 5.0f;
                _gravitySlider.SetValueWithoutNotify(settings.gravitationalConstant);
                if (_gravityValueText != null) _gravityValueText.text = $"{settings.gravitationalConstant:F2} G";
            }

            if (_softeningSlider != null)
            {
                _softeningSlider.minValue = 0.05f;
                _softeningSlider.maxValue = 3.0f;
                _softeningSlider.SetValueWithoutNotify(settings.softeningFactor);
                if (_softeningValueText != null) _softeningValueText.text = $"{settings.softeningFactor:F2} SU";
            }

            if (_trailsToggle != null)
            {
                _trailsToggle.SetIsOnWithoutNotify(settings.showOrbitTrails);
            }

            if (_cameraFollowToggle != null && SimulationManager.Instance.Camera != null)
            {
                _cameraFollowToggle.SetIsOnWithoutNotify(SimulationManager.Instance.Camera.FollowTrackedBody);
            }

            UpdatePlayPauseButtonUI(SimulationManager.Instance.Simulator.Clock.IsPaused);
        }

        private void OnPlayPauseClicked()
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                SimulationManager.Instance.Simulator.Clock.TogglePause();
            }
        }

        private void UpdatePlayPauseButtonUI(bool isPaused)
        {
            if (_playPauseText != null)
            {
                _playPauseText.text = isPaused ? "▶ Resume" : "⏸ Pause";
            }
        }

        private void OnResetClicked()
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                SimulationManager.Instance.Simulator.ResetSimulation();
            }
        }

        private void OnClearTrailsClicked()
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                SimulationManager.Instance.Simulator.ClearAllTrails();
            }
        }

        private void OnResetCameraClicked()
        {
            if (SimulationManager.Instance != null)
            {
                SimulationManager.Instance.ResetCameraView();
            }
        }

        private void OnSpeedChanged(float val)
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                SimulationManager.Instance.Simulator.Settings.timeScale = val;
                if (_speedValueText != null) _speedValueText.text = $"{val:F1}x";
            }
        }

        private void OnGravityChanged(float val)
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                SimulationManager.Instance.Simulator.Settings.gravitationalConstant = val;
                if (_gravityValueText != null) _gravityValueText.text = $"{val:F2} G";
            }
        }

        private void OnSofteningChanged(float val)
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                SimulationManager.Instance.Simulator.Settings.softeningFactor = val;
                if (_softeningValueText != null) _softeningValueText.text = $"{val:F2} SU";
            }
        }

        private void OnTrailsToggled(bool isOn)
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                SimulationManager.Instance.Simulator.SetTrailsVisibility(isOn);
            }
        }

        private void OnCameraFollowToggled(bool isOn)
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Camera != null)
            {
                SimulationManager.Instance.Camera.FollowTrackedBody = isOn;
            }
        }

        private void OnModeChanged(int modeIndex)
        {
            if (SimulationManager.Instance == null) return;

            SimulationMode mode = (SimulationMode)modeIndex;
            if (_experimentPresetGroup != null)
            {
                _experimentPresetGroup.SetActive(mode == SimulationMode.Experiment);
            }

            SimulationManager.Instance.SwitchMode(mode);
        }

        private void OnExperimentPresetChanged(int presetIndex)
        {
            if (SimulationManager.Instance == null) return;
            ExperimentPreset preset = (ExperimentPreset)presetIndex;
            SimulationManager.Instance.LoadExperimentPreset(preset);
        }
    }
}
