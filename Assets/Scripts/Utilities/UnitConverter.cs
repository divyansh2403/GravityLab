using UnityEngine;

namespace GravityLab.Utilities
{
    /// <summary>
    /// Provides consistent, scientifically grounded conversions between abstract Simulation Units (SU, MU, TU)
    /// and real-world Astronomical / SI units (AU, km, kg, m/s², Newtons, Solar Masses, Earth Masses).
    /// </summary>
    public static class UnitConverter
    {
        // Reference constants:
        // 1 Simulation Unit (Distance, SU) is scaled such that Earth's orbit radius ~ 22.0 SU corresponds to 1.0 AU (1.496e8 km).
        // Hence, 1 SU = 6.80e6 km.
        public const double KM_PER_SU = 6.80e6;
        public const double AU_PER_SU = 1.0 / 22.0;

        // 1 Simulation Mass Unit (MU) corresponds to 1.0 Earth Mass = 5.972e24 kg.
        public const double KG_PER_MU = 5.972e24;
        public const double SOLAR_MASS_KG = 1.989e30;
        public const double EARTH_MASS_KG = 5.972e24;

        // Velocity conversion: 1 SU/s corresponds to ~ 1.40 km/s in orbital reference scaling.
        public const double KM_PER_S_PER_VELOCITY_UNIT = 1.396;

        // Real gravitational constant G in SI (m^3 kg^-1 s^-2)
        public const double G_SI = 6.67430e-11;

        /// <summary>
        /// Formats mass with both simulation units and real-world equivalents (kg, Earth masses, Solar masses).
        /// </summary>
        public static string FormatMass(float simulationMass)
        {
            if (simulationMass >= 1000f)
            {
                double solarMasses = (simulationMass / 10000.0) * 1.0;
                double kg = simulationMass * KG_PER_MU * 33.29; // scaled conversion for stellar bodies
                return $"{simulationMass:N1} MU\n≈ {solarMasses:F3} M☉ ({kg:E2} kg)";
            }
            else
            {
                double kg = (double)simulationMass * KG_PER_MU;
                return $"{simulationMass:F3} MU\n≈ {simulationMass:F2} M⊕ ({kg:E2} kg)";
            }
        }

        /// <summary>
        /// Formats distance from simulation units to AU and millions of kilometers.
        /// </summary>
        public static string FormatDistance(float simulationUnits)
        {
            double au = simulationUnits * AU_PER_SU;
            double kmMillions = (simulationUnits * KM_PER_SU) / 1e6;
            return $"{simulationUnits:F2} SU ({au:F3} AU / {kmMillions:N1}M km)";
        }

        /// <summary>
        /// Formats velocity vector and scalar speed in SU/s and km/s.
        /// </summary>
        public static string FormatVelocity(Vector3 velocity)
        {
            float speed = velocity.magnitude;
            double speedKmS = speed * KM_PER_S_PER_VELOCITY_UNIT;
            return $"({velocity.x:F1}, {velocity.y:F1}, {velocity.z:F1}) SU/s\n|v| = {speed:F2} SU/s ({speedKmS:F1} km/s)";
        }

        /// <summary>
        /// Formats acceleration vector and magnitude.
        /// </summary>
        public static string FormatAcceleration(Vector3 acceleration)
        {
            float mag = acceleration.magnitude;
            // Approximate real-world scaling for orbital acceleration
            double realAccel = mag * 0.0059;
            return $"({acceleration.x:F3}, {acceleration.y:F3}, {acceleration.z:F3}) SU/s²\n|a| = {mag:F3} SU/s² ({realAccel:E2} m/s²)";
        }

        /// <summary>
        /// Formats gravitational force in Newtons.
        /// </summary>
        public static string FormatGravitationalForce(float forceMagnitude)
        {
            if (forceMagnitude <= 0.00001f) return "0.00 N";
            double realNewtons = forceMagnitude * 3.5e22;
            return $"{forceMagnitude:F3} FU ({realNewtons:E2} N)";
        }

        /// <summary>
        /// Formats simulation time into days and years.
        /// </summary>
        public static string FormatSimulationTime(float elapsedSeconds)
        {
            // In our simulation clock, ~6.28 seconds of 1x speed at Earth orbit r=22 (v=21.32) is ~1 Earth Year (365 days)
            // Period T = 2 * pi * r / v = 2 * 3.14159 * 22 / 21.32 = 6.48 seconds.
            const float SECONDS_PER_SIMULATED_YEAR = 6.484f;
            float simulatedYears = elapsedSeconds / SECONDS_PER_SIMULATED_YEAR;
            float simulatedDays = simulatedYears * 365.25f;

            if (simulatedYears >= 1.0f)
            {
                return $"{simulatedYears:F2} Years ({simulatedDays:N0} Days)";
            }
            return $"{simulatedDays:F1} Days ({elapsedSeconds:F1}s elapsed)";
        }
    }
}
