using UnityEngine;

namespace GravityLab.Data
{
    /// <summary>
    /// Categorization of celestial bodies for scientific visualization and UI grouping.
    /// </summary>
    public enum BodyType
    {
        Star,
        TerrestrialPlanet,
        GasGiant,
        IceGiant,
        DwarfPlanet,
        Moon,
        Asteroid,
        Custom
    }

    /// <summary>
    /// Serializable data model defining initial parameters and scientific metadata for a celestial body.
    /// </summary>
    [System.Serializable]
    public class CelestialBodyData
    {
        [Header("Identity")]
        public string name = "New Body";
        public BodyType type = BodyType.Custom;
        [TextArea(2, 4)]
        public string description = "A celestial body in gravitational interaction.";

        [Header("Physical Parameters (Simulation Units)")]
        [Tooltip("Mass in Simulation Mass Units (MU). 1 MU = 1.0 Earth Mass.")]
        public float mass = 1.0f;

        [Tooltip("Visual rendering radius in Simulation Units (SU).")]
        public float visualRadius = 1.0f;

        [Tooltip("Initial 3D Cartesian position (SU).")]
        public Vector3 initialPosition = Vector3.zero;

        [Tooltip("Initial 3D velocity vector (SU / s).")]
        public Vector3 initialVelocity = Vector3.zero;

        [Header("Visual Appearance")]
        public Color displayColor = Color.white;
        public bool hasRings = false;
        public Color ringColor = new Color(0.85f, 0.8f, 0.7f, 0.6f);

        [Header("Real-World Reference Data (Scientific Display)")]
        public double realMassKg = 5.972e24;
        public double realRadiusKm = 6371.0;
        public double realSemiMajorAxisAu = 1.0;
        public double realOrbitalPeriodDays = 365.25;

        [Header("Simulation Constraints")]
        [Tooltip("If true, this body cannot be deleted from the simulation.")]
        public bool isProtected = false;

        [Tooltip("If true, this body acts as a fixed gravitational attractor (unmoved by others).")]
        public bool isStaticAttractor = false;

        public CelestialBodyData() { }

        public CelestialBodyData(
            string name,
            BodyType type,
            float mass,
            float visualRadius,
            Vector3 position,
            Vector3 velocity,
            Color color,
            string description,
            double realMassKg = 0,
            double realRadiusKm = 0,
            double realSemiMajorAxisAu = 0,
            double realOrbitalPeriodDays = 0,
            bool isProtected = false,
            bool isStaticAttractor = false,
            bool hasRings = false)
        {
            this.name = name;
            this.type = type;
            this.mass = Mathf.Max(0.0001f, mass);
            this.visualRadius = Mathf.Max(0.1f, visualRadius);
            this.initialPosition = position;
            this.initialVelocity = velocity;
            this.displayColor = color;
            this.description = description;
            this.realMassKg = realMassKg;
            this.realRadiusKm = realRadiusKm;
            this.realSemiMajorAxisAu = realSemiMajorAxisAu;
            this.realOrbitalPeriodDays = realOrbitalPeriodDays;
            this.isProtected = isProtected;
            this.isStaticAttractor = isStaticAttractor;
            this.hasRings = hasRings;
        }

        public CelestialBodyData Clone()
        {
            return (CelestialBodyData)this.MemberwiseClone();
        }
    }
}
