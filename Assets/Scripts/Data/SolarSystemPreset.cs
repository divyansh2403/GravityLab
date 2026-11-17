using System.Collections.Generic;
using UnityEngine;

namespace GravityLab.Data
{
    /// <summary>
    /// Repository of scientifically informed presets for celestial simulation:
    /// - Standard 9-body Solar System
    /// - Binary Star System with circumbinary planet
    /// - Figure-8 Stable Three-Body choreography (Chenciner & Montgomery)
    /// - Sun-Jupiter-Trojan Lagrange L4/L5 system
    /// </summary>
    public static class SolarSystemPreset
    {
        /// <summary>
        /// Generates the standard 9-body Solar System preset.
        /// Distances and velocities are calibrated using Newtonian circular orbital mechanics: v = sqrt(G * M / r)
        /// with G = 1.0 and Sun Mass M = 10,000 MU.
        /// </summary>
        public static List<CelestialBodyData> GetSolarSystemPreset()
        {
            var list = new List<CelestialBodyData>
            {
                // Sun (Central Star)
                new CelestialBodyData(
                    name: "Sun",
                    type: BodyType.Star,
                    mass: 10000f,
                    visualRadius: 3.8f,
                    position: Vector3.zero,
                    velocity: Vector3.zero,
                    color: new Color(1.0f, 0.85f, 0.25f, 1.0f),
                    description: "The G-type main-sequence star at the center of our solar system. Contains 99.86% of the system's total mass.",
                    realMassKg: 1.989e30,
                    realRadiusKm: 696340,
                    realSemiMajorAxisAu: 0.0,
                    realOrbitalPeriodDays: 0.0,
                    isProtected: true,
                    isStaticAttractor: false
                ),

                // Mercury (r = 10, v = 31.62)
                new CelestialBodyData(
                    name: "Mercury",
                    type: BodyType.TerrestrialPlanet,
                    mass: 0.055f,
                    visualRadius: 0.55f,
                    position: new Vector3(10f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 31.62f),
                    color: new Color(0.72f, 0.68f, 0.64f, 1.0f),
                    description: "Smallest planet, closest to the Sun. Heavily cratered silicate body with high orbital velocity and no atmosphere.",
                    realMassKg: 3.301e23,
                    realRadiusKm: 2439.7,
                    realSemiMajorAxisAu: 0.387,
                    realOrbitalPeriodDays: 87.97,
                    isProtected: true
                ),

                // Venus (r = 15, v = 25.82)
                new CelestialBodyData(
                    name: "Venus",
                    type: BodyType.TerrestrialPlanet,
                    mass: 0.815f,
                    visualRadius: 0.85f,
                    position: new Vector3(15f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 25.82f),
                    color: new Color(0.92f, 0.82f, 0.60f, 1.0f),
                    description: "Terrestrial planet with dense, toxic CO2 atmosphere producing an extreme runaway greenhouse effect. Hottest planet in the system.",
                    realMassKg: 4.867e24,
                    realRadiusKm: 6051.8,
                    realSemiMajorAxisAu: 0.723,
                    realOrbitalPeriodDays: 224.7,
                    isProtected: true
                ),

                // Earth (r = 22, v = 21.32)
                new CelestialBodyData(
                    name: "Earth",
                    type: BodyType.TerrestrialPlanet,
                    mass: 1.0f,
                    visualRadius: 0.95f,
                    position: new Vector3(22f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 21.32f),
                    color: new Color(0.20f, 0.55f, 0.90f, 1.0f),
                    description: "Our home planet. Only known astronomical object harboring life, liquid surface water oceans, and an active nitrogen-oxygen atmosphere.",
                    realMassKg: 5.972e24,
                    realRadiusKm: 6371.0,
                    realSemiMajorAxisAu: 1.000,
                    realOrbitalPeriodDays: 365.25,
                    isProtected: true
                ),

                // Mars (r = 30, v = 18.26)
                new CelestialBodyData(
                    name: "Mars",
                    type: BodyType.TerrestrialPlanet,
                    mass: 0.107f,
                    visualRadius: 0.70f,
                    position: new Vector3(30f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 18.26f),
                    color: new Color(0.88f, 0.38f, 0.22f, 1.0f),
                    description: "The 'Red Planet' due to surface iron oxide (rust). Home to Olympus Mons, the largest volcano in the solar system.",
                    realMassKg: 6.417e23,
                    realRadiusKm: 3389.5,
                    realSemiMajorAxisAu: 1.524,
                    realOrbitalPeriodDays: 686.98,
                    isProtected: true
                ),

                // Jupiter (r = 52, v = 13.87)
                new CelestialBodyData(
                    name: "Jupiter",
                    type: BodyType.GasGiant,
                    mass: 317.8f,
                    visualRadius: 2.3f,
                    position: new Vector3(52f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 13.87f),
                    color: new Color(0.85f, 0.70f, 0.50f, 1.0f),
                    description: "Largest planet in the solar system. Massive gas giant primarily composed of hydrogen and helium with the iconic Great Red Spot storm.",
                    realMassKg: 1.898e27,
                    realRadiusKm: 69911.0,
                    realSemiMajorAxisAu: 5.204,
                    realOrbitalPeriodDays: 4332.6,
                    isProtected: true
                ),

                // Saturn (r = 82, v = 11.04)
                new CelestialBodyData(
                    name: "Saturn",
                    type: BodyType.GasGiant,
                    mass: 95.2f,
                    visualRadius: 1.9f,
                    position: new Vector3(82f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 11.04f),
                    color: new Color(0.92f, 0.85f, 0.65f, 1.0f),
                    description: "Second-largest planet, famous for its extensive, brilliant ring system of water ice particles and rocky debris.",
                    realMassKg: 5.683e26,
                    realRadiusKm: 58232.0,
                    realSemiMajorAxisAu: 9.582,
                    realOrbitalPeriodDays: 10759.2,
                    isProtected: true,
                    hasRings: true
                ),

                // Uranus (r = 120, v = 9.13)
                new CelestialBodyData(
                    name: "Uranus",
                    type: BodyType.IceGiant,
                    mass: 14.5f,
                    visualRadius: 1.4f,
                    position: new Vector3(120f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 9.13f),
                    color: new Color(0.55f, 0.85f, 0.88f, 1.0f),
                    description: "Ice giant with a pale cyan hue from atmospheric methane. Possesses a unique extreme axial tilt of 97.8 degrees, rotating on its side.",
                    realMassKg: 8.681e25,
                    realRadiusKm: 25362.0,
                    realSemiMajorAxisAu: 19.20,
                    realOrbitalPeriodDays: 30685.4,
                    isProtected: true
                ),

                // Neptune (r = 160, v = 7.91)
                new CelestialBodyData(
                    name: "Neptune",
                    type: BodyType.IceGiant,
                    mass: 17.1f,
                    visualRadius: 1.35f,
                    position: new Vector3(160f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 7.91f),
                    color: new Color(0.25f, 0.45f, 0.95f, 1.0f),
                    description: "Outermost classical planet. Dynamic, deep azure atmosphere with supersonic winds reaching over 2,100 km/h.",
                    realMassKg: 1.024e26,
                    realRadiusKm: 24622.0,
                    realSemiMajorAxisAu: 30.05,
                    realOrbitalPeriodDays: 60189.0,
                    isProtected: true
                )
            };

            return list;
        }

        /// <summary>
        /// Generates a Binary Star System with a circumbinary planet.
        /// </summary>
        public static List<CelestialBodyData> GetBinaryStarPreset()
        {
            var list = new List<CelestialBodyData>
            {
                // Star Alpha (Mass 5000, r = 15, v = 9.13)
                new CelestialBodyData(
                    name: "Helios Alpha",
                    type: BodyType.Star,
                    mass: 5000f,
                    visualRadius: 2.8f,
                    position: new Vector3(-12f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, -10.2f),
                    color: new Color(1.0f, 0.70f, 0.20f, 1.0f),
                    description: "Primary component of a close binary star system, orbiting the mutual center of mass."
                ),
                // Star Beta (Mass 5000, r = 15, v = 9.13)
                new CelestialBodyData(
                    name: "Helios Beta",
                    type: BodyType.Star,
                    mass: 5000f,
                    visualRadius: 2.5f,
                    position: new Vector3(12f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 10.2f),
                    color: new Color(0.40f, 0.75f, 1.0f, 1.0f),
                    description: "Secondary blue-white stellar component orbiting in tandem with Helios Alpha."
                ),
                // Circumbinary Planet 1 (Tatooine-like)
                new CelestialBodyData(
                    name: "Circumbinary Terra",
                    type: BodyType.TerrestrialPlanet,
                    mass: 1.5f,
                    visualRadius: 0.9f,
                    position: new Vector3(55f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 13.48f),
                    color: new Color(0.3f, 0.85f, 0.45f, 1.0f),
                    description: "A stable circumbinary (P-type) habitable planet orbiting both stars outside their chaotic perturbation zone."
                ),
                // Circumbinary Gas Giant
                new CelestialBodyData(
                    name: "Circumbinary Jove",
                    type: BodyType.GasGiant,
                    mass: 150f,
                    visualRadius: 1.8f,
                    position: new Vector3(100f, 0f, 0f),
                    velocity: new Vector3(0f, 0f, 10.0f),
                    color: new Color(0.9f, 0.5f, 0.75f, 1.0f),
                    description: "Outer gas giant maintaining long-term orbital stability around the binary barycenter."
                )
            };
            return list;
        }

        /// <summary>
        /// Generates the celebrated Chenciner-Montgomery (2000) Figure-Eight Three-Body choreography.
        /// Three equal masses chase each other indefinitely along a single planar figure-8 path.
        /// </summary>
        public static List<CelestialBodyData> GetFigureEightPreset()
        {
            float m = 1500f;
            float scalePos = 25f;
            float scaleVel = 7.74f;

            // Accurate dimensionless initial conditions for the Figure-8 solution:
            // x1 = -x2 = 0.97000436, y1 = -y2 = -0.24308753
            // vx3 = -2 * vx1, vy3 = -2 * vy1
            // vx1 = vx2 = 0.46620531, vy1 = vy2 = 0.43236573
            float x1 = 0.97000436f * scalePos;
            float y1 = -0.24308753f * scalePos;

            float vx1 = 0.46620531f * scaleVel;
            float vy1 = 0.43236573f * scaleVel;

            var list = new List<CelestialBodyData>
            {
                new CelestialBodyData(
                    name: "Body Alpha",
                    type: BodyType.Star,
                    mass: m,
                    visualRadius: 1.4f,
                    position: new Vector3(x1, 0f, y1),
                    velocity: new Vector3(vx1, 0f, vy1),
                    color: new Color(1.0f, 0.3f, 0.3f, 1.0f),
                    description: "First body in the famous periodic 3-body Figure-8 choreography."
                ),
                new CelestialBodyData(
                    name: "Body Beta",
                    type: BodyType.Star,
                    mass: m,
                    visualRadius: 1.4f,
                    position: new Vector3(-x1, 0f, -y1),
                    velocity: new Vector3(vx1, 0f, vy1),
                    color: new Color(0.3f, 1.0f, 0.4f, 1.0f),
                    description: "Second body sharing the identical figure-8 trajectory with 1/3 period phase shift."
                ),
                new CelestialBodyData(
                    name: "Body Gamma",
                    type: BodyType.Star,
                    mass: m,
                    visualRadius: 1.4f,
                    position: Vector3.zero,
                    velocity: new Vector3(-2f * vx1, 0f, -2f * vy1),
                    color: new Color(0.3f, 0.6f, 1.0f, 1.0f),
                    description: "Third body passing through the center of mass with compensating momentum."
                )
            };
            return list;
        }

        /// <summary>
        /// Generates a Sun-Jupiter system demonstrating stable Lagrange L4 and L5 Trojan points.
        /// </summary>
        public static List<CelestialBodyData> GetTrojanAsteroidsPreset()
        {
            float sunMass = 10000f;
            float jupMass = 350f;
            float r = 50f;
            float v = Mathf.Sqrt((sunMass + jupMass) / r);

            // Jupiter at (r, 0, 0), moving along +Z
            // L4 is 60 degrees ahead: (r * cos(60), 0, r * sin(60)) = (r * 0.5, 0, r * 0.866)
            // L5 is 60 degrees behind: (r * cos(-60), 0, r * sin(-60)) = (r * 0.5, 0, -r * 0.866)
            float l4x = r * 0.5f;
            float l4z = r * 0.866025f;
            // Velocity rotated 60 degrees:
            float l4vx = -v * 0.866025f;
            float l4vz = v * 0.5f;

            var list = new List<CelestialBodyData>
            {
                new CelestialBodyData("Sun", BodyType.Star, sunMass, 3.5f, Vector3.zero, Vector3.zero, new Color(1f, 0.85f, 0.2f), "Central Star", isProtected: true),
                new CelestialBodyData("Jupiter", BodyType.GasGiant, jupMass, 2.0f, new Vector3(r, 0f, 0f), new Vector3(0f, 0f, v), new Color(0.88f, 0.68f, 0.45f), "Primary Gas Giant", isProtected: true),
                new CelestialBodyData("Trojan Asteroid (L4)", BodyType.Asteroid, 0.001f, 0.45f, new Vector3(l4x, 0f, l4z), new Vector3(l4vx, 0f, l4vz), new Color(0.7f, 0.9f, 0.7f), "Stable Trojan asteroid trapped in Jupiter's leading L4 Lagrange point."),
                new CelestialBodyData("Trojan Asteroid (L5)", BodyType.Asteroid, 0.001f, 0.45f, new Vector3(l4x, 0f, -l4z), new Vector3(-l4vx, 0f, l4vz), new Color(0.9f, 0.7f, 0.9f), "Stable Trojan asteroid trapped in Jupiter's trailing L5 Lagrange point.")
            };
            return list;
        }
    }
}
