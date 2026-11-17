using System.Collections.Generic;
using UnityEngine;

namespace GravityLab.Visualization
{
    /// <summary>
    /// Manages the visual rendering of orbital trajectories behind a celestial body using a LineRenderer.
    /// Uses distance-based sampling and strict point count pooling to guarantee high performance
    /// without memory leaks on integrated graphics.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class OrbitTrail : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private int _maxPoints = 500;
        [SerializeField] private float _minDistance = 0.25f;
        [SerializeField] private float _startWidth = 0.35f;
        [SerializeField] private float _endWidth = 0.05f;

        private List<Vector3> _points = new List<Vector3>(512);
        private Vector3 _lastRecordedPos;
        private bool _isTrailVisible = true;
        private Color _trailColor = Color.white;

        public bool IsVisible => _isTrailVisible;
        public int PointCount => _points.Count;

        private void Awake()
        {
            EnsureLineRenderer();
        }

        private void EnsureLineRenderer()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }

            if (_lineRenderer != null)
            {
                _lineRenderer.useWorldSpace = true;
                _lineRenderer.startWidth = _startWidth;
                _lineRenderer.endWidth = _endWidth;
                _lineRenderer.numCornerVertices = 2;
                _lineRenderer.numCapVertices = 2;
                _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _lineRenderer.receiveShadows = false;
            }
        }

        /// <summary>
        /// Initializes the trail with a specific display color and creates a smooth alpha gradient.
        /// </summary>
        public void InitializeTrail(Color color, int maxPoints = 500, float minDistance = 0.25f)
        {
            EnsureLineRenderer();

            _trailColor = color;
            _maxPoints = maxPoints;
            _minDistance = minDistance;

            // Configure color gradient: fading from 85% alpha near the planet to 0% at the distant tail
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(color, 0.0f),
                    new GradientColorKey(color, 1.0f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.85f, 0.0f),
                    new GradientAlphaKey(0.40f, 0.5f),
                    new GradientAlphaKey(0.00f, 1.0f)
                }
            );

            _lineRenderer.colorGradient = gradient;
            ClearTrail();
        }

        /// <summary>
        /// Updates the trail path if the body has moved past the distance threshold.
        /// </summary>
        public void RecordPosition(Vector3 currentPosition)
        {
            if (!_isTrailVisible) return;

            if (_points.Count == 0)
            {
                _points.Add(currentPosition);
                _lastRecordedPos = currentPosition;
                UpdateLineRenderer();
                return;
            }

            float distSq = (currentPosition - _lastRecordedPos).sqrMagnitude;
            if (distSq >= _minDistance * _minDistance)
            {
                // Insert at beginning (newest point)
                _points.Insert(0, currentPosition);
                _lastRecordedPos = currentPosition;

                // Evict oldest point if exceeding limit
                if (_points.Count > _maxPoints)
                {
                    _points.RemoveAt(_points.Count - 1);
                }

                UpdateLineRenderer();
            }
            else
            {
                // Dynamically update the head point to keep line glued to planet
                _points[0] = currentPosition;
                _lineRenderer.SetPosition(0, currentPosition);
            }
        }

        private void UpdateLineRenderer()
        {
            if (_lineRenderer == null) return;

            int count = _points.Count;
            _lineRenderer.positionCount = count;
            for (int i = 0; i < count; i++)
            {
                _lineRenderer.SetPosition(i, _points[i]);
            }
        }

        /// <summary>
        /// Clears all vertices from the trail buffer.
        /// </summary>
        public void ClearTrail()
        {
            _points.Clear();
            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
            }
        }

        /// <summary>
        /// Toggles trail visibility without discarding recorded points.
        /// </summary>
        public void SetVisibility(bool visible)
        {
            _isTrailVisible = visible;
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = visible;
            }
        }
    }
}
