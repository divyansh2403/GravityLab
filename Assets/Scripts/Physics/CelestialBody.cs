using System;
using UnityEngine;
using GravityLab.Data;
using GravityLab.Visualization;

namespace GravityLab.Physics
{
    /// <summary>
    /// Represents a discrete physical celestial body in the N-body gravitational simulation.
    /// Encapsulates mass, state vectors (position, velocity, acceleration), and scientific metadata.
    /// Strictly decouples numerical integration physics from Unity's rendering transform.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    public class CelestialBody : MonoBehaviour
    {
        [Header("Physical Attributes")]
        [SerializeField] private string _bodyName = "Celestial Body";
        [SerializeField] private BodyType _bodyType = BodyType.TerrestrialPlanet;
        [SerializeField] private float _mass = 1.0f;
        [SerializeField] private float _visualRadius = 1.0f;
        [SerializeField] private bool _isProtected = false;
        [SerializeField] private bool _isStaticAttractor = false;

        [Header("Initial State Cache")]
        [SerializeField] private Vector3 _initialPosition;
        [SerializeField] private Vector3 _initialVelocity;
        [SerializeField] private float _initialMass;

        [Header("Real-Time Kinematics (Physics Space)")]
        [SerializeField] private Vector3 _physicsPosition;
        [SerializeField] private Vector3 _physicsVelocity;
        [SerializeField] private Vector3 _physicsAcceleration;

        [Header("Scientific Metadata")]
        [TextArea(2, 4)]
        [SerializeField] private string _description = "Astronomical body.";
        public double realMassKg;
        public double realRadiusKm;
        public double realSemiMajorAxisAu;
        public double realOrbitalPeriodDays;

        [Header("Visual Components")]
        [SerializeField] private PlanetVisuals _visuals;
        [SerializeField] private OrbitTrail _orbitTrail;
        [SerializeField] private SphereCollider _collider;

        // Events
        public event Action<CelestialBody> OnBodyDestroyed;
        public event Action<CelestialBody> OnBodyClicked;

        // Public Properties
        public string BodyName
        {
            get => _bodyName;
            set
            {
                _bodyName = value;
                gameObject.name = value;
            }
        }

        public BodyType Type
        {
            get => _bodyType;
            set => _bodyType = value;
        }

        public float Mass
        {
            get => _mass;
            set => _mass = Mathf.Max(0.0001f, value);
        }

        public float VisualRadius
        {
            get => _visualRadius;
            set
            {
                _visualRadius = Mathf.Max(0.05f, value);
                UpdateVisualScale();
            }
        }

        public bool IsProtected
        {
            get => _isProtected;
            set => _isProtected = value;
        }

        public bool IsStaticAttractor
        {
            get => _isStaticAttractor;
            set => _isStaticAttractor = value;
        }

        public Vector3 PhysicsPosition
        {
            get => _physicsPosition;
            set => _physicsPosition = value;
        }

        public Vector3 PhysicsVelocity
        {
            get => _physicsVelocity;
            set => _physicsVelocity = value;
        }

        public Vector3 PhysicsAcceleration
        {
            get => _physicsAcceleration;
            set => _physicsAcceleration = value;
        }

        public Vector3 NetGravitationalForce => _physicsAcceleration * _mass;
        public float Speed => _physicsVelocity.magnitude;
        public float AccelerationMagnitude => _physicsAcceleration.magnitude;
        public string Description => _description;
        public PlanetVisuals Visuals => _visuals;
        public OrbitTrail Trail => _orbitTrail;

        private void Awake()
        {
            EnsureComponents();
        }

        private void Start()
        {
            // Sync initial state if not initialized
            if (_initialMass <= 0f)
            {
                CaptureCurrentAsInitial();
            }
        }

        /// <summary>
        /// Ensures all necessary visual and collision components exist.
        /// </summary>
        public void EnsureComponents()
        {
            if (_visuals == null) _visuals = GetComponent<PlanetVisuals>();
            if (_visuals == null) _visuals = gameObject.AddComponent<PlanetVisuals>();

            if (_orbitTrail == null) _orbitTrail = GetComponent<OrbitTrail>();
            if (_orbitTrail == null) _orbitTrail = gameObject.AddComponent<OrbitTrail>();

            if (_collider == null) _collider = GetComponent<SphereCollider>();
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<SphereCollider>();
                _collider.radius = 0.5f;
            }
        }

        /// <summary>
        /// Initializes the body from a CelestialBodyData specification.
        /// </summary>
        public void InitializeFromData(CelestialBodyData data)
        {
            EnsureComponents();

            _bodyName = data.name;
            gameObject.name = data.name;
            _bodyType = data.type;
            _mass = Mathf.Max(0.0001f, data.mass);
            _visualRadius = Mathf.Max(0.05f, data.visualRadius);
            _isProtected = data.isProtected;
            _isStaticAttractor = data.isStaticAttractor;
            _description = data.description;

            realMassKg = data.realMassKg;
            realRadiusKm = data.realRadiusKm;
            realSemiMajorAxisAu = data.realSemiMajorAxisAu;
            realOrbitalPeriodDays = data.realOrbitalPeriodDays;

            _initialPosition = data.initialPosition;
            _initialVelocity = data.initialVelocity;
            _initialMass = _mass;

            _physicsPosition = _initialPosition;
            _physicsVelocity = _initialVelocity;
            _physicsAcceleration = Vector3.zero;

            transform.position = _physicsPosition;
            UpdateVisualScale();

            if (_visuals != null)
            {
                _visuals.InitializeVisuals(data.displayColor, _bodyType == BodyType.Star, data.hasRings, data.ringColor);
            }

            if (_orbitTrail != null)
            {
                _orbitTrail.InitializeTrail(data.displayColor);
            }
        }

        public void CaptureCurrentAsInitial()
        {
            _initialPosition = _physicsPosition;
            _initialVelocity = _physicsVelocity;
            _initialMass = _mass;
        }

        /// <summary>
        /// Resets position, velocity, and trail to initial conditions.
        /// </summary>
        public void ResetToInitialState()
        {
            _physicsPosition = _initialPosition;
            _physicsVelocity = _initialVelocity;
            _physicsAcceleration = Vector3.zero;
            _mass = _initialMass;

            transform.position = _physicsPosition;
            if (_orbitTrail != null)
            {
                _orbitTrail.ClearTrail();
            }
        }

        /// <summary>
        /// Synchronizes the Unity GameObject rendering transform with the numerical physics position.
        /// </summary>
        public void SyncVisualTransform()
        {
            transform.position = _physicsPosition;
        }

        private void UpdateVisualScale()
        {
            float diameter = _visualRadius * 2.0f;
            transform.localScale = new Vector3(diameter, diameter, diameter);
        }

        private void OnMouseDown()
        {
            OnBodyClicked?.Invoke(this);
        }

        private void OnDestroy()
        {
            OnBodyDestroyed?.Invoke(this);
        }
    }
}
