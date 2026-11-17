using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using GravityLab.Core;
using GravityLab.Physics;

namespace GravityLab.UI
{
    /// <summary>
    /// Interactive dialog allowing students and researchers to spawn custom celestial bodies
    /// with strict input validation, orbital velocity calculation helpers, and color selection.
    /// </summary>
    public class BodyCreationPanel : MonoBehaviour
    {
        [Header("Dialog Container")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Text Inputs")]
        [SerializeField] private InputField _nameInput;
        [SerializeField] private InputField _massInput;
        [SerializeField] private InputField _radiusInput;

        [Header("Position Inputs (SU)")]
        [SerializeField] private InputField _posXInput;
        [SerializeField] private InputField _posYInput;
        [SerializeField] private InputField _posZInput;

        [Header("Velocity Inputs (SU/s)")]
        [SerializeField] private InputField _velXInput;
        [SerializeField] private InputField _velYInput;
        [SerializeField] private InputField _velZInput;

        [Header("Visual Color Selection")]
        [SerializeField] private Image _colorPreviewImage;
        [SerializeField] private Button _colorCyanBtn;
        [SerializeField] private Button _colorGreenBtn;
        [SerializeField] private Button _colorRedBtn;
        [SerializeField] private Button _colorGoldBtn;
        [SerializeField] private Button _colorPurpleBtn;
        [SerializeField] private Button _colorWhiteBtn;

        [Header("Action Buttons & Feedback")]
        [SerializeField] private Button _autoOrbitButton;
        [SerializeField] private Button _createButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Text _feedbackText;

        private Color _selectedColor = new Color(0.25f, 0.85f, 1.0f); // Default bright cyan

        private void Start()
        {
            if (_createButton != null) _createButton.onClick.AddListener(OnCreateClicked);
            if (_closeButton != null) _closeButton.onClick.AddListener(ClosePanel);
            if (_autoOrbitButton != null) _autoOrbitButton.onClick.AddListener(OnAutoOrbitClicked);

            SetupColorButtons();
            SetDefaultValues();
            UpdateColorPreview();
        }

        private void SetupColorButtons()
        {
            if (_colorCyanBtn != null) _colorCyanBtn.onClick.AddListener(() => SetColor(new Color(0.2f, 0.85f, 1.0f)));
            if (_colorGreenBtn != null) _colorGreenBtn.onClick.AddListener(() => SetColor(new Color(0.3f, 0.95f, 0.45f)));
            if (_colorRedBtn != null) _colorRedBtn.onClick.AddListener(() => SetColor(new Color(1.0f, 0.35f, 0.25f)));
            if (_colorGoldBtn != null) _colorGoldBtn.onClick.AddListener(() => SetColor(new Color(1.0f, 0.85f, 0.2f)));
            if (_colorPurpleBtn != null) _colorPurpleBtn.onClick.AddListener(() => SetColor(new Color(0.85f, 0.45f, 1.0f)));
            if (_colorWhiteBtn != null) _colorWhiteBtn.onClick.AddListener(() => SetColor(new Color(0.95f, 0.95f, 0.95f)));
        }

        public void SetColor(Color col)
        {
            _selectedColor = col;
            UpdateColorPreview();
        }

        private void UpdateColorPreview()
        {
            if (_colorPreviewImage != null)
            {
                _colorPreviewImage.color = _selectedColor;
            }
        }

        public void OpenPanel()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            SetDefaultValues();
            ClearFeedback();
        }

        public void ClosePanel()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void SetDefaultValues()
        {
            int nextId = (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
                ? SimulationManager.Instance.Simulator.BodyCount + 1
                : 1;

            if (_nameInput != null) _nameInput.text = $"Planet_{nextId}";
            if (_massInput != null) _massInput.text = "1.0";
            if (_radiusInput != null) _radiusInput.text = "0.8";

            if (_posXInput != null) _posXInput.text = "38.0";
            if (_posYInput != null) _posYInput.text = "0.0";
            if (_posZInput != null) _posZInput.text = "0.0";

            if (_velXInput != null) _velXInput.text = "0.0";
            if (_velYInput != null) _velYInput.text = "0.0";
            if (_velZInput != null) _velZInput.text = "16.2";
        }

        private void OnAutoOrbitClicked()
        {
            // Automatically calculate tangential circular orbit velocity around the heaviest central mass (or origin)
            if (!TryParseVector(_posXInput, _posYInput, _posZInput, out Vector3 pos, out string posErr))
            {
                ShowFeedback($"Position Error: {posErr}", true);
                return;
            }

            float dist = pos.magnitude;
            if (dist < 1.0f)
            {
                ShowFeedback("Body is too close to origin (r < 1 SU) to calculate stable orbit.", true);
                return;
            }

            // Find central mass
            float centralMass = 10000f; // Default solar mass
            float G = 1.0f;
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                G = SimulationManager.Instance.Simulator.Settings.gravitationalConstant;
                var bodies = SimulationManager.Instance.Simulator.ActiveBodies;
                for (int i = 0; i < bodies.Count; i++)
                {
                    if (bodies[i] != null && bodies[i].Mass > centralMass)
                    {
                        centralMass = bodies[i].Mass;
                    }
                }
            }

            // v = sqrt(G * M / r)
            float speed = Mathf.Sqrt((G * centralMass) / dist);

            // Compute perpendicular orbital velocity in XZ plane
            Vector3 tangentDir = Vector3.Cross(Vector3.up, pos.normalized);
            Vector3 orbitalVel = tangentDir * speed;

            if (_velXInput != null) _velXInput.text = orbitalVel.x.ToString("F2", CultureInfo.InvariantCulture);
            if (_velYInput != null) _velYInput.text = "0.0";
            if (_velZInput != null) _velZInput.text = orbitalVel.z.ToString("F2", CultureInfo.InvariantCulture);

            ShowFeedback($"Calculated circular orbital speed: {speed:F2} SU/s", false);
        }

        private void OnCreateClicked()
        {
            // Validate Name
            string name = _nameInput != null ? _nameInput.text.Trim() : "";
            if (string.IsNullOrEmpty(name))
            {
                ShowFeedback("Error: Body name cannot be empty.", true);
                return;
            }

            // Validate Mass
            if (!float.TryParse(_massInput.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float mass) || mass <= 0f)
            {
                ShowFeedback("Error: Mass must be a positive number (> 0).", true);
                return;
            }

            // Validate Radius
            if (!float.TryParse(_radiusInput.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float radius) || radius <= 0f)
            {
                ShowFeedback("Error: Visual radius must be a positive number (> 0).", true);
                return;
            }

            // Validate Position
            if (!TryParseVector(_posXInput, _posYInput, _posZInput, out Vector3 pos, out string posErr))
            {
                ShowFeedback($"Invalid Position: {posErr}", true);
                return;
            }

            // Validate Velocity
            if (!TryParseVector(_velXInput, _velYInput, _velZInput, out Vector3 vel, out string velErr))
            {
                ShowFeedback($"Invalid Velocity: {velErr}", true);
                return;
            }

            // Proximity Collision Check
            if (SimulationManager.Instance != null && SimulationManager.Instance.Simulator != null)
            {
                var bodies = SimulationManager.Instance.Simulator.ActiveBodies;
                for (int i = 0; i < bodies.Count; i++)
                {
                    if (bodies[i] != null)
                    {
                        float dist = Vector3.Distance(pos, bodies[i].PhysicsPosition);
                        float minSafeDist = radius + bodies[i].VisualRadius;
                        if (dist < minSafeDist)
                        {
                            ShowFeedback($"Collision Warning: Too close to {bodies[i].BodyName} (Dist: {dist:F2} SU < {minSafeDist:F2} SU).", true);
                            return;
                        }
                    }
                }
            }

            // Validation successful -> Spawn Body
            if (SimulationManager.Instance != null)
            {
                SimulationManager.Instance.CreateCustomBody(name, mass, radius, pos, vel, _selectedColor);
                ShowFeedback($"Successfully created '{name}'!", false);
                ClosePanel();
            }
        }

        private bool TryParseVector(InputField inX, InputField inY, InputField inZ, out Vector3 result, out string error)
        {
            result = Vector3.zero;
            error = "";

            if (!float.TryParse(inX.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
            {
                error = "X component must be a valid number.";
                return false;
            }
            if (!float.TryParse(inY.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                error = "Y component must be a valid number.";
                return false;
            }
            if (!float.TryParse(inZ.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
            {
                error = "Z component must be a valid number.";
                return false;
            }

            result = new Vector3(x, y, z);
            return true;
        }

        private void ShowFeedback(string msg, bool isError)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = msg;
                _feedbackText.color = isError ? new Color(1.0f, 0.4f, 0.4f) : new Color(0.4f, 1.0f, 0.5f);
            }
        }

        private void ClearFeedback()
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = "";
            }
        }
    }
}
