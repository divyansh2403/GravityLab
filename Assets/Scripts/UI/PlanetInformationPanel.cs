using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GravityLab.Core;
using GravityLab.Physics;
using GravityLab.Utilities;

namespace GravityLab.UI
{
    /// <summary>
    /// Displays scientific telemetry and orbital dynamics for the currently selected celestial body.
    /// Updates in real-time as the simulation advances.
    /// </summary>
    public class PlanetInformationPanel : MonoBehaviour
    {
        [Header("UI Text Displays")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _bodyTypeText;
        [SerializeField] private Text _massText;
        [SerializeField] private Text _radiusText;
        [SerializeField] private Text _positionText;
        [SerializeField] private Text _velocityText;
        [SerializeField] private Text _accelerationText;
        [SerializeField] private Text _distanceToSunText;
        [SerializeField] private Text _gravitationalForceText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _astronomicalFactsText;

        [Header("Action Buttons")]
        [SerializeField] private Button _focusButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Text _deleteButtonText;

        [Header("Quick Select Dropdown")]
        [SerializeField] private Dropdown _bodyDropdown;

        [Header("Empty State")]
        [SerializeField] private GameObject _contentContainer;
        [SerializeField] private GameObject _noSelectionPrompt;

        private CelestialBody _currentBody;

        private void Start()
        {
            if (_focusButton != null) _focusButton.onClick.AddListener(OnFocusClicked);
            if (_deleteButton != null) _deleteButton.onClick.AddListener(OnDeleteClicked);
            if (_bodyDropdown != null) _bodyDropdown.onValueChanged.AddListener(OnDropdownSelected);

            ApplyDropdownStyling();

            if (SimulationManager.Instance != null)
            {
                SimulationManager.Instance.OnBodySelectionChanged += SetSelectedBody;
                SimulationManager.Instance.OnModeChanged += HandleModeChanged;

                if (SimulationManager.Instance.Simulator != null)
                {
                    SimulationManager.Instance.Simulator.OnBodyAdded += HandleBodiesChanged;
                    SimulationManager.Instance.Simulator.OnBodyRemoved += HandleBodiesChanged;
                    PopulateDropdown(SimulationManager.Instance.Simulator.ActiveBodies);
                }
                else
                {
                    PopulateDropdown(null);
                }

                SetSelectedBody(SimulationManager.Instance.SelectedBody);
            }
            else
            {
                PopulateDropdown(null);
            }
        }

        private void OnDestroy()
        {
            if (_focusButton != null) _focusButton.onClick.RemoveListener(OnFocusClicked);
            if (_deleteButton != null) _deleteButton.onClick.RemoveListener(OnDeleteClicked);
            if (_bodyDropdown != null) _bodyDropdown.onValueChanged.RemoveListener(OnDropdownSelected);

            if (SimulationManager.Instance != null)
            {
                SimulationManager.Instance.OnBodySelectionChanged -= SetSelectedBody;
                SimulationManager.Instance.OnModeChanged -= HandleModeChanged;

                if (SimulationManager.Instance.Simulator != null)
                {
                    SimulationManager.Instance.Simulator.OnBodyAdded -= HandleBodiesChanged;
                    SimulationManager.Instance.Simulator.OnBodyRemoved -= HandleBodiesChanged;
                }
            }
        }

        private void HandleBodiesChanged(CelestialBody body)
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                PopulateDropdown(SimulationManager.Instance.Simulator.ActiveBodies);
            }
        }

        private void HandleModeChanged(GravityLab.Core.SimulationMode mode)
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                PopulateDropdown(SimulationManager.Instance.Simulator.ActiveBodies);
            }
        }

        public void SetSelectedBody(CelestialBody body)
        {
            _currentBody = body;

            if (_currentBody == null)
            {
                if (_contentContainer != null) _contentContainer.SetActive(false);
                if (_noSelectionPrompt != null) _noSelectionPrompt.SetActive(true);
                return;
            }

            if (_contentContainer != null) _contentContainer.SetActive(true);
            if (_noSelectionPrompt != null) _noSelectionPrompt.SetActive(false);

            if (_titleText != null) _titleText.text = _currentBody.BodyName;
            if (_bodyTypeText != null) _bodyTypeText.text = $"Type: {_currentBody.Type}";
            if (_descriptionText != null) _descriptionText.text = _currentBody.Description;

            if (_deleteButton != null)
            {
                _deleteButton.interactable = !_currentBody.IsProtected;
                if (_deleteButtonText != null)
                {
                    _deleteButtonText.text = _currentBody.IsProtected ? "Protected (Default)" : "Delete Body";
                }
            }

            SyncDropdownSelection();
            PopulateAstronomicalFacts();
            RefreshTelemetry();
        }

        /// <summary>
        /// Synchronizes the dropdown's selected value and caption text with the current celestial body.
        /// </summary>
        private void SyncDropdownSelection()
        {
            if (_bodyDropdown == null || _currentBody == null) return;

            for (int i = 0; i < _bodyDropdown.options.Count; i++)
            {
                if (string.Equals(_bodyDropdown.options[i].text, _currentBody.BodyName, System.StringComparison.OrdinalIgnoreCase))
                {
                    _bodyDropdown.SetValueWithoutNotify(i);
                    _bodyDropdown.RefreshShownValue();
                    return;
                }
            }
        }

        private void Update()
        {
            if (_currentBody != null && _contentContainer != null && _contentContainer.activeSelf)
            {
                RefreshTelemetry();
            }
        }

        private void RefreshTelemetry()
        {
            if (_currentBody == null) return;

            if (_massText != null)
            {
                _massText.text = UnitConverter.FormatMass(_currentBody.Mass);
            }

            if (_radiusText != null)
            {
                _radiusText.text = $"{_currentBody.VisualRadius:F2} SU (Visual Scale)";
            }

            if (_positionText != null)
            {
                Vector3 p = _currentBody.PhysicsPosition;
                _positionText.text = $"({p.x:F1}, {p.y:F1}, {p.z:F1}) SU";
            }

            if (_velocityText != null)
            {
                _velocityText.text = UnitConverter.FormatVelocity(_currentBody.PhysicsVelocity);
            }

            if (_accelerationText != null)
            {
                _accelerationText.text = UnitConverter.FormatAcceleration(_currentBody.PhysicsAcceleration);
            }

            if (_distanceToSunText != null)
            {
                float dist = _currentBody.PhysicsPosition.magnitude;
                _distanceToSunText.text = UnitConverter.FormatDistance(dist);
            }

            if (_gravitationalForceText != null)
            {
                float forceMag = _currentBody.NetGravitationalForce.magnitude;
                _gravitationalForceText.text = UnitConverter.FormatGravitationalForce(forceMag);
            }
        }

        private void PopulateAstronomicalFacts()
        {
            if (_astronomicalFactsText == null || _currentBody == null) return;

            if (_currentBody.realMassKg > 0)
            {
                _astronomicalFactsText.text =
                    $"• Real Mass: {_currentBody.realMassKg:E2} kg\n" +
                    $"• Mean Radius: {_currentBody.realRadiusKm:N0} km\n" +
                    $"• Semi-Major Axis: {_currentBody.realSemiMajorAxisAu:F3} AU\n" +
                    $"• Orbital Period: {_currentBody.realOrbitalPeriodDays:N1} Earth Days";
            }
            else
            {
                _astronomicalFactsText.text = "Custom user-generated simulation entity.";
            }
        }

        private void OnFocusClicked()
        {
            if (SimulationManager.Instance != null && _currentBody != null)
            {
                SimulationManager.Instance.FocusCameraOnSelected();
            }
        }

        private void OnDeleteClicked()
        {
            if (SimulationManager.Instance != null && _currentBody != null)
            {
                SimulationManager.Instance.DeleteBody(_currentBody);
            }
        }

        public void PopulateDropdown(IReadOnlyList<CelestialBody> bodies)
        {
            if (_bodyDropdown == null) return;

            _bodyDropdown.ClearOptions();
            var options = new List<string>();
            int selectedIdx = 0;

            if (bodies != null && bodies.Count > 0)
            {
                for (int i = 0; i < bodies.Count; i++)
                {
                    if (bodies[i] != null)
                    {
                        options.Add(bodies[i].BodyName);
                        if (_currentBody == bodies[i])
                        {
                            selectedIdx = options.Count - 1;
                        }
                    }
                }
            }

            // Fallback: if no active bodies are loaded yet, display canonical 9 solar system bodies
            if (options.Count == 0)
            {
                string[] defaultBodies = { "Sun", "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune" };
                options.AddRange(defaultBodies);
                selectedIdx = 3; // Earth default
            }

            _bodyDropdown.AddOptions(options);
            _bodyDropdown.SetValueWithoutNotify(selectedIdx);
            _bodyDropdown.RefreshShownValue();
        }

        private void OnDropdownSelected(int index)
        {
            if (SimulationManager.Instance == null || SimulationManager.Instance.Simulator == null) return;
            var bodies = SimulationManager.Instance.Simulator.ActiveBodies;
            if (index >= 0 && index < bodies.Count && bodies[index] != null)
            {
                SimulationManager.Instance.SelectBody(bodies[index]);
            }
            else if (_bodyDropdown != null && index >= 0 && index < _bodyDropdown.options.Count)
            {
                string selectedName = _bodyDropdown.options[index].text;
                SimulationManager.Instance.SelectBodyByName(selectedName);
            }
        }

        /// <summary>
        /// Configures the dropdown's template, background, and item text styling to ensure
        /// maximum contrast and legibility (dark navy text on light grey/white background).
        /// </summary>
        public void ApplyDropdownStyling()
        {
            if (_bodyDropdown == null) return;

            // Caption Text (closed state): readable light text against the dark dropdown button
            if (_bodyDropdown.captionText != null)
            {
                _bodyDropdown.captionText.color = new Color(0.92f, 0.96f, 1.0f, 1.0f);
            }

            RectTransform template = _bodyDropdown.template;
            if (template == null) return;

            // Template container border/background (dark slate border)
            Image templateImg = template.GetComponent<Image>();
            if (templateImg != null)
            {
                templateImg.color = new Color(0.12f, 0.16f, 0.24f, 1.0f);
            }

            Transform viewport = template.Find("Viewport");
            if (viewport != null)
            {
                // Mask: disable showMaskGraphic so it does not render a solid opaque white rectangle
                Mask mask = viewport.GetComponent<Mask>();
                if (mask != null)
                {
                    mask.showMaskGraphic = false;
                }

                Image vpImg = viewport.GetComponent<Image>();
                if (vpImg != null)
                {
                    vpImg.color = new Color(0.95f, 0.96f, 0.98f, 1.0f);
                }

                Transform content = viewport.Find("Content");
                if (content != null)
                {
                    Transform item = content.Find("Item");
                    if (item != null)
                    {
                        // Ensure Item has an Image background component (light grey/white)
                        Image itemBg = item.GetComponent<Image>();
                        if (itemBg == null)
                        {
                            itemBg = item.gameObject.AddComponent<Image>();
                        }
                        itemBg.color = new Color(0.95f, 0.96f, 0.98f, 1.0f);

                        // Configure Toggle transition colors
                        Toggle itemToggle = item.GetComponent<Toggle>();
                        if (itemToggle != null)
                        {
                            itemToggle.targetGraphic = itemBg;
                            ColorBlock cb = itemToggle.colors;
                            cb.normalColor = new Color(0.95f, 0.96f, 0.98f, 1.0f);      // Light grey
                            cb.highlightedColor = new Color(0.82f, 0.88f, 0.96f, 1.0f); // Soft light blue on hover
                            cb.pressedColor = new Color(0.72f, 0.80f, 0.92f, 1.0f);     // Darker blue on press
                            cb.selectedColor = new Color(0.78f, 0.86f, 0.98f, 1.0f);    // Clear selected highlight
                            cb.disabledColor = new Color(0.85f, 0.85f, 0.85f, 0.5f);
                            cb.colorMultiplier = 1.0f;
                            cb.fadeDuration = 0.1f;
                            itemToggle.colors = cb;
                        }

                        // Ensure Item Text is dark navy and clearly legible
                        if (_bodyDropdown.itemText != null)
                        {
                            _bodyDropdown.itemText.color = new Color(0.06f, 0.09f, 0.16f, 1.0f); // Dark navy
                        }
                        else
                        {
                            Text itemText = item.GetComponentInChildren<Text>();
                            if (itemText != null)
                            {
                                itemText.color = new Color(0.06f, 0.09f, 0.16f, 1.0f);
                                _bodyDropdown.itemText = itemText;
                            }
                        }
                    }
                }
            }
        }
    }
}
