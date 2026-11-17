using System.Collections.Generic;
using UnityEngine;
using GravityLab.Core;

namespace GravityLab.Physics
{
    public enum IntegratorType
    {
        VelocityVerlet, // Primary symplectic 2nd-order method (recommended)
        EulerCromer,    // Semi-implicit Euler 1st-order method (educational comparison)
        RungeKutta4     // Classical RK4 4th-order method (high short-term accuracy)
    }

    /// <summary>
    /// Executes numerical integration to advance the N-body gravitational equations of motion over time.
    /// Primary algorithm: Symplectic Velocity Verlet (Störmer-Verlet).
    /// </summary>
    public static class OrbitalIntegrator
    {
        // Reusable scratch buffers to eliminate garbage collector allocations during physics ticks
        private static Vector3[] _accelBuffer = new Vector3[128];
        private static Vector3[] _tempPosBuffer = new Vector3[128];
        private static Vector3[] _tempVelBuffer = new Vector3[128];

        private static void EnsureBufferCapacity(int count)
        {
            if (_accelBuffer.Length < count)
            {
                int newCap = Mathf.NextPowerOfTwo(count);
                _accelBuffer = new Vector3[newCap];
                _tempPosBuffer = new Vector3[newCap];
                _tempVelBuffer = new Vector3[newCap];
            }
        }

        /// <summary>
        /// Performs a single fixed-timestep numerical integration step using Velocity Verlet.
        /// </summary>
        public static void IntegrateVelocityVerlet(
            IReadOnlyList<CelestialBody> bodies,
            SimulationSettings settings,
            float dt)
        {
            int count = bodies.Count;
            if (count == 0) return;
            EnsureBufferCapacity(count);

            // Step 1: Update positions using current velocity and current acceleration
            // x(t + dt) = x(t) + v(t) * dt + 0.5 * a(t) * dt^2
            float halfDtSq = 0.5f * dt * dt;
            for (int i = 0; i < count; i++)
            {
                var body = bodies[i];
                if (body == null || body.IsStaticAttractor) continue;

                Vector3 newPos = body.PhysicsPosition + (body.PhysicsVelocity * dt) + (body.PhysicsAcceleration * halfDtSq);
                body.PhysicsPosition = newPos;
            }

            // Step 2: Compute new accelerations a(t + dt) at the updated positions
            GravityCalculator.CalculateAllAccelerations(bodies, settings, _accelBuffer);

            // Step 3: Update velocities using average of old and new accelerations
            // v(t + dt) = v(t) + 0.5 * (a(t) + a(t + dt)) * dt
            float halfDt = 0.5f * dt;
            for (int i = 0; i < count; i++)
            {
                var body = bodies[i];
                if (body == null || body.IsStaticAttractor) continue;

                Vector3 newAccel = _accelBuffer[i];
                Vector3 newVel = body.PhysicsVelocity + ((body.PhysicsAcceleration + newAccel) * halfDt);

                body.PhysicsVelocity = newVel;
                body.PhysicsAcceleration = newAccel;
            }
        }

        /// <summary>
        /// Semi-implicit Euler-Cromer integration step (for educational comparison).
        /// v(t + dt) = v(t) + a(t) * dt
        /// x(t + dt) = x(t) + v(t + dt) * dt
        /// </summary>
        public static void IntegrateEulerCromer(
            IReadOnlyList<CelestialBody> bodies,
            SimulationSettings settings,
            float dt)
        {
            int count = bodies.Count;
            if (count == 0) return;
            EnsureBufferCapacity(count);

            GravityCalculator.CalculateAllAccelerations(bodies, settings, _accelBuffer);

            for (int i = 0; i < count; i++)
            {
                var body = bodies[i];
                if (body == null || body.IsStaticAttractor) continue;

                body.PhysicsAcceleration = _accelBuffer[i];
                body.PhysicsVelocity += body.PhysicsAcceleration * dt;
                body.PhysicsPosition += body.PhysicsVelocity * dt;
            }
        }
    }
}
