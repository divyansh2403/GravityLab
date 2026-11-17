using UnityEngine;
using GravityLab.Physics;

namespace GravityLab.Visualization
{
    /// <summary>
    /// Scientific 3D orbital camera controller.
    /// Provides smooth orbital rotation, panning, exponential zooming, planetary tracking,
    /// and keyboard navigation (WASD/QE).
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target & Tracking")]
        [SerializeField] private Transform _target;
        [SerializeField] private CelestialBody _trackedBody;
        [SerializeField] private bool _followTrackedBody = true;

        [Header("Orbit Speeds & Sensitivity")]
        [SerializeField] private float _orbitSensitivityX = 180f;
        [SerializeField] private float _orbitSensitivityY = 120f;
        [SerializeField] private float _panSpeed = 40f;
        [SerializeField] private float _keyPanSpeed = 60f;

        [Header("Zoom Constraints")]
        [SerializeField] private float _zoomSensitivity = 15f;
        [SerializeField] private float _minDistance = 3f;
        [SerializeField] private float _maxDistance = 500f;
        [SerializeField] private float _smoothDamping = 8f;

        [Header("Default Isometric Overview")]
        [SerializeField] private Vector3 _defaultPivot = Vector3.zero;
        [SerializeField] private float _defaultDistance = 90f;
        [SerializeField] private float _defaultYaw = 45f;
        [SerializeField] private float _defaultPitch = 35f;

        // Current internal angles & distance
        private float _yaw = 45f;
        private float _pitch = 35f;
        private float _distance = 90f;
        private float _targetDistance = 90f;
        private Vector3 _currentPivot = Vector3.zero;
        private Vector3 _targetPivot = Vector3.zero;

        public bool FollowTrackedBody
        {
            get => _followTrackedBody;
            set => _followTrackedBody = value;
        }

        public CelestialBody TrackedBody => _trackedBody;

        private void Start()
        {
            ResetView();
        }

        private void LateUpdate()
        {
            HandleKeyboardInput();
            HandleMouseInput();
            UpdateCameraTransform();
        }

        private void HandleKeyboardInput()
        {
            // Arrow / WASD panning
            float h = Input.GetAxis("Horizontal"); // A/D or Left/Right
            float v = Input.GetAxis("Vertical");   // W/S or Up/Down

            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                // Break tracking when user manually pans
                _trackedBody = null;

                Vector3 forward = transform.forward;
                forward.y = 0f;
                forward.Normalize();
                Vector3 right = transform.right;
                right.y = 0f;
                right.Normalize();

                Vector3 move = (right * h + forward * v) * (_keyPanSpeed * Time.unscaledDeltaTime);
                _targetPivot += move;
            }

            // Elevation (Q / E or R / F)
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.R))
            {
                _trackedBody = null;
                _targetPivot += Vector3.up * (_keyPanSpeed * Time.unscaledDeltaTime);
            }
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.F))
            {
                // If F is pressed while body is selected, focus on body handled separately,
                // but if Q is pressed, pan down
                if (Input.GetKey(KeyCode.Q))
                {
                    _trackedBody = null;
                    _targetPivot -= Vector3.up * (_keyPanSpeed * Time.unscaledDeltaTime);
                }
            }

            // Reset view shortcut (Home key)
            if (Input.GetKeyDown(KeyCode.Home))
            {
                ResetView();
            }
        }

        private void HandleMouseInput()
        {
            // Right-Click Drag: Orbit around pivot
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                _yaw += mouseX * _orbitSensitivityX * Time.unscaledDeltaTime;
                _pitch -= mouseY * _orbitSensitivityY * Time.unscaledDeltaTime;
                _pitch = Mathf.Clamp(_pitch, -85f, 85f);
            }

            // Middle-Click Drag: Pan in view plane
            if (Input.GetMouseButton(2))
            {
                _trackedBody = null;
                float panX = -Input.GetAxis("Mouse X") * _panSpeed * (_distance / 50f) * Time.unscaledDeltaTime;
                float panY = -Input.GetAxis("Mouse Y") * _panSpeed * (_distance / 50f) * Time.unscaledDeltaTime;

                _targetPivot += transform.right * panX + transform.up * panY;
            }

            // Scroll Wheel: Exponential zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                float zoomDelta = scroll * _zoomSensitivity * (_targetDistance * 0.25f);
                _targetDistance = Mathf.Clamp(_targetDistance - zoomDelta, _minDistance, _maxDistance);
            }
        }

        private void UpdateCameraTransform()
        {
            // Update tracking if following a body
            if (_trackedBody != null && _followTrackedBody)
            {
                _targetPivot = _trackedBody.PhysicsPosition;
            }

            // Smooth interpolation towards targets
            _currentPivot = Vector3.Lerp(_currentPivot, _targetPivot, Time.unscaledDeltaTime * _smoothDamping);
            _distance = Mathf.Lerp(_distance, _targetDistance, Time.unscaledDeltaTime * _smoothDamping);

            // Compute spherical orbital coordinates
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -_distance);

            transform.position = _currentPivot + offset;
            transform.LookAt(_currentPivot);
        }

        /// <summary>
        /// Smoothly frames and begins tracking the specified celestial body.
        /// </summary>
        public void FocusOnBody(CelestialBody body, bool autoTrack = true)
        {
            if (body == null) return;

            _trackedBody = body;
            _followTrackedBody = autoTrack;
            _targetPivot = body.PhysicsPosition;

            // Frame distance proportional to body radius
            float desiredDistance = Mathf.Clamp(body.VisualRadius * 5.5f, 5.0f, 60.0f);
            _targetDistance = desiredDistance;
        }

        /// <summary>
        /// Resets camera to standard isometric solar system overview.
        /// </summary>
        public void ResetView()
        {
            _trackedBody = null;
            _targetPivot = _defaultPivot;
            _targetDistance = _defaultDistance;
            _yaw = _defaultYaw;
            _pitch = _defaultPitch;
        }
    }
}
