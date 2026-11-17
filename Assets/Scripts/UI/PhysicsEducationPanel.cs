using UnityEngine;
using UnityEngine.UI;

namespace GravityLab.UI
{
    /// <summary>
    /// Educational modal dialog detailing the physics theory, mathematical formulas,
    /// numerical integration principles, and softening mechanisms governing GravityLab.
    /// </summary>
    public class PhysicsEducationPanel : MonoBehaviour
    {
        [Header("Dialog Root")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _closeButton;

        [Header("Content Displays")]
        [SerializeField] private Text _formulaBannerText;
        [SerializeField] private Text _variablesLegendText;
        [SerializeField] private Text _topicsBodyText;

        private void Start()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(ClosePanel);
            PopulateEducationalContent();
        }

        public void OpenPanel()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            PopulateEducationalContent();
        }

        public void ClosePanel()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void PopulateEducationalContent()
        {
            if (_formulaBannerText != null)
            {
                _formulaBannerText.text =
                    "NEWTON'S LAW OF UNIVERSAL GRAVITATION\n" +
                    "F = G · (m₁ · m₂) / r²\n" +
                    "a₁ = G · m₂ / (r² + ε²)^(3/2) · r̂";
            }

            if (_variablesLegendText != null)
            {
                _variablesLegendText.text =
                    "• F = Gravitational force vector (Newtons)\n" +
                    "• G = Gravitational constant (defines coupling strength)\n" +
                    "• m₁, m₂ = Masses of interacting celestial bodies\n" +
                    "• r = Euclidean distance between body centers (|r₂ - r₁|)\n" +
                    "• ε = Plummer softening factor (prevents 1/0 singularities)\n" +
                    "• Δt = Fixed discrete numerical simulation timestep";
            }

            if (_topicsBodyText != null)
            {
                _topicsBodyText.text =
                    "1. NEWTON'S LAW OF GRAVITATION\n" +
                    "Every mass in the universe attracts every other mass with a force directly proportional to the product of their masses and inversely proportional to the square of the distance between them.\n\n" +

                    "2. GRAVITATIONAL FORCE & NEWTON'S THIRD LAW\n" +
                    "Gravitational interactions are symmetric: F_ij = -F_ji. Body A pulls on Body B with identical force magnitude as Body B pulls on Body A, but in the exact opposite direction.\n\n" +

                    "3. MASS & GRAVITATIONAL ACCELERATION\n" +
                    "From Newton's 2nd Law (F = m·a), a body's acceleration does not depend on its own mass, but exclusively on the mass of the attractor: a_i = G · m_j / r².\n\n" +

                    "4. ORBITAL VELOCITY\n" +
                    "For a circular orbit around mass M, gravitational pull provides the necessary centripetal force: G·M·m / r² = m·v² / r, yielding: v_orb = √(G·M / r).\n\n" +

                    "5. ORBITAL MOTION (KEPLERIAN TRAJECTORIES)\n" +
                    "Under pure two-body gravity, orbits are conic sections (circles, ellipses, parabolas, or hyperbolas). In multi-body systems, planetary interactions introduce orbital perturbations.\n\n" +

                    "6. N-BODY SIMULATION\n" +
                    "For N > 2 bodies, no general closed-form analytical solution exists (Poincaré's Three-Body Theorem). Motions must be solved computationally via numerical integration over discrete time slices.\n\n" +

                    "7. NUMERICAL INTEGRATION\n" +
                    "Differential equations d²x/dt² = a(x) cannot be integrated continuously on a computer. We discretize time into intervals Δt and compute successive kinematic states.\n\n" +

                    "8. VELOCITY VERLET METHOD\n" +
                    "Unlike basic Forward Euler (which artificially injects energy into orbits, causing planets to spiral outward), Velocity Verlet is a symplectic, time-reversible, 2nd-order algorithm that conserves total mechanical energy and angular momentum over long simulations:\n" +
                    "  x(t+Δt) = x(t) + v(t)Δt + 0.5 a(t)Δt²\n" +
                    "  v(t+Δt) = v(t) + 0.5 [a(t) + a(t+Δt)] Δt\n\n" +

                    "9. GRAVITATIONAL SOFTENING (PLUMMER EPSILON)\n" +
                    "When two point-masses pass very close, 1/r² approaches infinity, causing catastrophic velocity spikes. We replace r with √(r² + ε²), mimicking extended physical radii and ensuring numerical stability.\n\n" +

                    "10. SIMULATION TIMESTEP (Δt) & STABILITY\n" +
                    "A smaller Δt increases numerical accuracy but demands more computations. GravityLab utilizes fixed sub-stepping to preserve physical precision even when simulation playback speed is accelerated.";
            }
        }
    }
}
