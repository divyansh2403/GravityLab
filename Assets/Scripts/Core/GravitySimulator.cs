using System;
using System.Collections.Generic;
using UnityEngine;
using GravityLab.Physics;
using GravityLab.Utilities;

namespace GravityLab.Core
{
    /// <summary>
    /// Master simulation driver for Newtonian N-body gravitational mechanics.
    /// Manages the collection of simulated celestial bodies, drives fixed-timestep integration,
    /// synchronizes rendering transforms, and computes diagnostic energies.
    /// </summary>
    [DisallowMultipleComponent]
    public class GravitySimulator : MonoBehaviour
    {
        [Header("Settings & Integrator")]
        [SerializeField] private SimulationSettings _settings = new SimulationSettings();
        [SerializeField] private IntegratorType _integratorType = IntegratorType.VelocityVerlet;

        [Header("Active Bodies")]
        [SerializeField] private List<CelestialBody> _bodies = new List<CelestialBody>();

        private SimulationClock _clock = new SimulationClock();

        // Events
        public event Action<CelestialBody> OnBodyAdded;
        public event Action<CelestialBody> OnBodyRemoved;

        public SimulationSettings Settings => _settings;
        public SimulationClock Clock => _clock;
        public IReadOnlyList<CelestialBody> ActiveBodies => _bodies;
        public int BodyCount => _bodies.Count;

        private void Awake()
        {
            if (_settings == null) _settings = new SimulationSettings();
        }

        private void Start()
        {
            // Initial acceleration calculation for all bodies
            Vector3[] initialAccels = new Vector3[_bodies.Count];
            GravityCalculator.CalculateAllAccelerations(_bodies, _settings, initialAccels);
            for (int i = 0; i < _bodies.Count; i++)
            {
                if (_bodies[i] != null)
                {
                    _bodies[i].PhysicsAcceleration = initialAccels[i];
                }
            }
        }

        private void Update()
        {
            // Drive fixed-timestep physics accumulation and sub-stepping
            _clock.Tick(Time.unscaledDeltaTime, _settings, ExecutePhysicsStep);

            // Synchronize visual transforms and record orbit trail points
            SyncAllVisuals();
        }

        /// <summary>
        /// Executes a single deterministic numerical integration step.
        /// </summary>
        private void ExecutePhysicsStep(float dt)
        {
            switch (_integratorType)
            {
                case IntegratorType.VelocityVerlet:
                    OrbitalIntegrator.IntegrateVelocityVerlet(_bodies, _settings, dt);
                    break;
                case IntegratorType.EulerCromer:
                    OrbitalIntegrator.IntegrateEulerCromer(_bodies, _settings, dt);
                    break;
                default:
                    OrbitalIntegrator.IntegrateVelocityVerlet(_bodies, _settings, dt);
                    break;
            }
        }

        /// <summary>
        /// Synchronizes visual GameObject transforms with updated physics positions
        /// and appends vertices to orbit trails.
        /// </summary>
        private void SyncAllVisuals()
        {
            for (int i = 0; i < _bodies.Count; i++)
            {
                var body = _bodies[i];
                if (body == null) continue;

                body.SyncVisualTransform();

                if (_settings.showOrbitTrails && body.Trail != null)
                {
                    body.Trail.RecordPosition(body.PhysicsPosition);
                }
            }
        }

        /// <summary>
        /// Registers a new celestial body into the gravitational simulation.
        /// </summary>
        public void RegisterBody(CelestialBody body)
        {
            if (body == null || _bodies.Contains(body)) return;

            _bodies.Add(body);
            body.OnBodyDestroyed += HandleBodyDestroyed;

            // Compute initial acceleration on new body
            Vector3[] accels = new Vector3[_bodies.Count];
            GravityCalculator.CalculateAllAccelerations(_bodies, _settings, accels);
            for (int i = 0; i < _bodies.Count; i++)
            {
                if (_bodies[i] != null) _bodies[i].PhysicsAcceleration = accels[i];
            }

            SimulationLogger.LogPhysicsEvent(body.BodyName, "Registered", $"Mass: {body.Mass:F2} MU");
            OnBodyAdded?.Invoke(body);
        }

        /// <summary>
        /// Removes a celestial body from the simulation.
        /// </summary>
        public bool UnregisterBody(CelestialBody body)
        {
            if (body == null || !_bodies.Contains(body)) return false;

            if (body.IsProtected)
            {
                SimulationLogger.LogWarning($"Cannot delete protected system body: {body.BodyName}");
                return false;
            }

            body.OnBodyDestroyed -= HandleBodyDestroyed;
            _bodies.Remove(body);

            SimulationLogger.LogPhysicsEvent(body.BodyName, "Unregistered", "Removed from simulation.");
            OnBodyRemoved?.Invoke(body);
            return true;
        }

        private void HandleBodyDestroyed(CelestialBody body)
        {
            if (_bodies.Contains(body))
            {
                _bodies.Remove(body);
                OnBodyRemoved?.Invoke(body);
            }
        }

        /// <summary>
        /// Resets all registered celestial bodies to their initial positions and velocities,
        /// clearing trails and resetting the simulation clock.
        /// </summary>
        public void ResetSimulation()
        {
            _clock.ResetClock();

            for (int i = 0; i < _bodies.Count; i++)
            {
                var body = _bodies[i];
                if (body != null)
                {
                    body.ResetToInitialState();
                }
            }

            // Recalculate initial gravitational field
            Vector3[] accels = new Vector3[_bodies.Count];
            GravityCalculator.CalculateAllAccelerations(_bodies, _settings, accels);
            for (int i = 0; i < _bodies.Count; i++)
            {
                if (_bodies[i] != null) _bodies[i].PhysicsAcceleration = accels[i];
            }

            SimulationLogger.LogInfo("Simulation state reset to initial conditions.");
        }

        /// <summary>
        /// Clears all orbit trails across all bodies.
        /// </summary>
        public void ClearAllTrails()
        {
            for (int i = 0; i < _bodies.Count; i++)
            {
                var body = _bodies[i];
                if (body != null && body.Trail != null)
                {
                    body.Trail.ClearTrail();
                }
            }
        }

        /// <summary>
        /// Sets trail visibility for all bodies.
        /// </summary>
        public void SetTrailsVisibility(bool visible)
        {
            _settings.showOrbitTrails = visible;
            for (int i = 0; i < _bodies.Count; i++)
            {
                var body = _bodies[i];
                if (body != null && body.Trail != null)
                {
                    body.Trail.SetVisibility(visible);
                }
            }
        }

        /// <summary>
        /// Destroys all non-protected custom bodies.
        /// </summary>
        public void ClearCustomBodies()
        {
            List<CelestialBody> toRemove = new List<CelestialBody>();
            for (int i = 0; i < _bodies.Count; i++)
            {
                var b = _bodies[i];
                if (b != null && !b.IsProtected)
                {
                    toRemove.Add(b);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                UnregisterBody(toRemove[i]);
                Destroy(toRemove[i].gameObject);
            }
        }

        /// <summary>
        /// Destroys all bodies regardless of protected state (used when switching modes/presets).
        /// </summary>
        public void ClearAllBodies()
        {
            var copy = new List<CelestialBody>(_bodies);
            _bodies.Clear();

            for (int i = 0; i < copy.Count; i++)
            {
                if (copy[i] != null)
                {
                    copy[i].OnBodyDestroyed -= HandleBodyDestroyed;
                    Destroy(copy[i].gameObject);
                }
            }
        }
    }
}
