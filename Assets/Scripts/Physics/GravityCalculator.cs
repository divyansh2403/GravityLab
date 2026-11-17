using System.Collections.Generic;
using UnityEngine;
using GravityLab.Core;

namespace GravityLab.Physics
{
    /// <summary>
    /// Implements Newtonian N-Body gravitational mechanics:
    /// F = G * (m1 * m2) / r^2
    /// Incorporates Plummer softening to prevent singularity / infinite acceleration at close range.
    /// Computes total kinetic and gravitational potential energy for numerical conservation checks.
    /// </summary>
    public static class GravityCalculator
    {
        /// <summary>
        /// Calculates the gravitational acceleration exerted on targetBody by otherBody.
        /// a = (G * M_other / (r^2 + eps^2)^(3/2)) * (r_other - r_target)
        /// </summary>
        public static Vector3 CalculateAccelerationPair(
            Vector3 targetPos,
            Vector3 otherPos,
            float otherMass,
            float G,
            float softeningFactor,
            float maxAccelClamp)
        {
            Vector3 displacement = otherPos - targetPos;
            float distSq = displacement.sqrMagnitude;

            // Plummer softening: r_eff^2 = r^2 + epsilon^2
            float softenedDistSq = distSq + (softeningFactor * softeningFactor);
            float dist = Mathf.Sqrt(softenedDistSq);

            if (dist < 0.0001f)
                return Vector3.zero;

            // a = G * M / (softenedDistSq)^(3/2) * displacement
            // = (G * M / (softenedDistSq * dist)) * displacement
            float invDenominator = 1.0f / (softenedDistSq * dist);
            float accelMagnitude = G * otherMass * invDenominator;

            Vector3 acceleration = displacement * accelMagnitude;

            // Safety clamp to prevent catastrophic numerical ejection
            if (acceleration.sqrMagnitude > maxAccelClamp * maxAccelClamp)
            {
                acceleration = acceleration.normalized * maxAccelClamp;
            }

            return acceleration;
        }

        /// <summary>
        /// Computes the net gravitational acceleration vector for all active celestial bodies.
        /// Returns an array of accelerations matching the bodies list index.
        /// </summary>
        public static void CalculateAllAccelerations(
            IReadOnlyList<CelestialBody> bodies,
            SimulationSettings settings,
            Vector3[] outAccelerations)
        {
            int count = bodies.Count;
            if (count == 0) return;

            // Zero out accelerations
            for (int i = 0; i < count; i++)
            {
                outAccelerations[i] = Vector3.zero;
            }

            float G = settings.gravitationalConstant;
            float eps = settings.softeningFactor;
            float clamp = settings.maxAccelerationClamp;

            // Pairwise symmetric calculation (Newton's 3rd Law: F_ij = -F_ji)
            for (int i = 0; i < count; i++)
            {
                CelestialBody bodyA = bodies[i];
                if (bodyA == null) continue;

                Vector3 posA = bodyA.PhysicsPosition;
                float massA = bodyA.Mass;

                for (int j = i + 1; j < count; j++)
                {
                    CelestialBody bodyB = bodies[j];
                    if (bodyB == null) continue;

                    Vector3 posB = bodyB.PhysicsPosition;
                    float massB = bodyB.Mass;

                    Vector3 displacement = posB - posA;
                    float distSq = displacement.sqrMagnitude;
                    float softenedDistSq = distSq + (eps * eps);
                    float dist = Mathf.Sqrt(softenedDistSq);

                    if (dist < 0.00001f) continue;

                    float factor = G / (softenedDistSq * dist);

                    // Acceleration on A due to B:
                    if (!bodyA.IsStaticAttractor)
                    {
                        Vector3 accelA = displacement * (factor * massB);
                        if (accelA.sqrMagnitude > clamp * clamp)
                            accelA = accelA.normalized * clamp;
                        outAccelerations[i] += accelA;
                    }

                    // Acceleration on B due to A (equal and opposite force: a_B = -displacement * factor * massA):
                    if (!bodyB.IsStaticAttractor)
                    {
                        Vector3 accelB = -displacement * (factor * massA);
                        if (accelB.sqrMagnitude > clamp * clamp)
                            accelB = accelB.normalized * clamp;
                        outAccelerations[j] += accelB;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates total mechanical energy of the system: Kinetic + Potential.
        /// In an ideal N-body system with conservative forces, total energy is constant.
        /// </summary>
        public static void CalculateSystemEnergy(
            IReadOnlyList<CelestialBody> bodies,
            SimulationSettings settings,
            out float totalKineticEnergy,
            out float totalPotentialEnergy)
        {
            totalKineticEnergy = 0f;
            totalPotentialEnergy = 0f;

            int count = bodies.Count;
            float G = settings.gravitationalConstant;
            float eps = settings.softeningFactor;

            // Kinetic Energy: 0.5 * m * v^2
            for (int i = 0; i < count; i++)
            {
                var body = bodies[i];
                if (body == null) continue;
                totalKineticEnergy += 0.5f * body.Mass * body.PhysicsVelocity.sqrMagnitude;
            }

            // Potential Energy: - sum_{i < j} G * mi * mj / sqrt(r_ij^2 + eps^2)
            for (int i = 0; i < count; i++)
            {
                var bodyA = bodies[i];
                if (bodyA == null) continue;

                for (int j = i + 1; j < count; j++)
                {
                    var bodyB = bodies[j];
                    if (bodyB == null) continue;

                    float dist = Mathf.Sqrt((bodyB.PhysicsPosition - bodyA.PhysicsPosition).sqrMagnitude + (eps * eps));
                    if (dist > 0.0001f)
                    {
                        totalPotentialEnergy -= (G * bodyA.Mass * bodyB.Mass) / dist;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the Center of Mass (Barycenter) and total system momentum.
        /// </summary>
        public static void CalculateCenterOfMass(
            IReadOnlyList<CelestialBody> bodies,
            out Vector3 centerOfMass,
            out Vector3 totalMomentum)
        {
            centerOfMass = Vector3.zero;
            totalMomentum = Vector3.zero;
            float totalMass = 0f;

            for (int i = 0; i < bodies.Count; i++)
            {
                var b = bodies[i];
                if (b == null) continue;

                totalMass += b.Mass;
                centerOfMass += b.PhysicsPosition * b.Mass;
                totalMomentum += b.PhysicsVelocity * b.Mass;
            }

            if (totalMass > 0.0001f)
            {
                centerOfMass /= totalMass;
            }
        }
    }
}
