using System;
using UnityEngine;

namespace GravityLab.Core
{
    /// <summary>
    /// Global configuration settings for numerical integration, gravitational constants,
    /// scaling factors, and visualization limits.
    /// </summary>
    [Serializable]
    public class SimulationSettings
    {
        [Header("Gravitational Mechanics")]
        [Tooltip("Gravitational constant G in simulation units.")]
        [Range(0.01f, 5.0f)]
        public float gravitationalConstant = 1.0f;

        [Tooltip("Plummer softening factor epsilon (SU) to prevent infinite acceleration during near-collisions.")]
        [Range(0.05f, 5.0f)]
        public float softeningFactor = 0.5f;

        [Tooltip("Safety clamp on maximum acceleration magnitude (SU/s²) to prevent numerical divergence.")]
        public float maxAccelerationClamp = 250.0f;

        [Header("Temporal Controls")]
        [Tooltip("Fixed physical timestep delta_t in seconds.")]
        [Range(0.001f, 0.05f)]
        public float fixedTimestep = 0.016f;

        [Tooltip("Simulation speed multiplier.")]
        [Range(0.1f, 10.0f)]
        public float timeScale = 1.0f;

        [Tooltip("Maximum allowed sub-steps per frame to prevent simulation stalling (spiral of death).")]
        public int maxSubStepsPerFrame = 25;

        [Header("Trail Visualization")]
        public bool showOrbitTrails = true;

        [Tooltip("Maximum number of vertices recorded per body trail.")]
        [Range(50, 2000)]
        public int maxTrailPoints = 600;

        [Tooltip("Minimum distance in SU a body must travel before adding a new trail vertex.")]
        public float trailMinVertexDistance = 0.25f;

        [Tooltip("Width of the orbital trajectory line in simulation units.")]
        public float trailWidth = 0.35f;

        public SimulationSettings Clone()
        {
            return (SimulationSettings)this.MemberwiseClone();
        }
    }
}
