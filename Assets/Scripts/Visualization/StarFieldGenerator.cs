using UnityEngine;

namespace GravityLab.Visualization
{
    /// <summary>
    /// Procedurally creates a lightweight, highly optimized 3D spherical starfield
    /// using Unity's ParticleSystem. Renders thousands of celestial stars with variable
    /// sizes and subtle stellar temperatures without impacting GPU performance on integrated graphics.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class StarFieldGenerator : MonoBehaviour
    {
        [Header("Starfield Configuration")]
        [SerializeField] private int _starCount = 1500;
        [SerializeField] private float _fieldRadius = 500f;
        [SerializeField] private float _minStarSize = 1.0f;
        [SerializeField] private float _maxStarSize = 2.8f;

        private ParticleSystem _particleSystem;
        private ParticleSystem.Particle[] _particles;

        private void Awake()
        {
            GenerateStarField();
        }

        /// <summary>
        /// Generates the static spherical distribution of background stars.
        /// </summary>
        public void GenerateStarField()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            if (_particleSystem == null)
            {
                _particleSystem = gameObject.AddComponent<ParticleSystem>();
            }

            var main = _particleSystem.main;
            main.maxParticles = _starCount;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = _particleSystem.emission;
            emission.enabled = false;

            var shape = _particleSystem.shape;
            shape.enabled = false;

            var renderer = GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;

                Shader particleShader = Shader.Find("Mobile/Particles/Additive");
                if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
                if (particleShader == null) particleShader = Shader.Find("Sprites/Default");

                if (particleShader != null)
                {
                    renderer.sharedMaterial = new Material(particleShader);
                }
            }

            _particles = new ParticleSystem.Particle[_starCount];

            // Stellar spectral color templates (O, B, A, F, G, K, M stellar classes)
            Color[] starPalette = new Color[]
            {
                new Color(0.7f, 0.85f, 1.0f, 0.9f), // Blue-white (Class O/B)
                new Color(1.0f, 1.0f, 1.0f, 0.85f), // Pure White (Class A)
                new Color(1.0f, 0.95f, 0.8f, 0.8f), // Yellow-white (Class F)
                new Color(1.0f, 0.85f, 0.6f, 0.8f), // Yellow (Class G - Solar)
                new Color(1.0f, 0.65f, 0.4f, 0.75f) // Orange-red (Class K/M)
            };

            for (int i = 0; i < _starCount; i++)
            {
                // Uniform point distribution on spherical shell
                Vector3 randomDir = Random.onUnitSphere;
                float distance = _fieldRadius + Random.Range(-40f, 40f);

                _particles[i].position = transform.position + (randomDir * distance);
                _particles[i].startSize = Random.Range(_minStarSize, _maxStarSize);
                _particles[i].startColor = starPalette[Random.Range(0, starPalette.Length)];
                _particles[i].remainingLifetime = 999999f;
            }

            _particleSystem.SetParticles(_particles, _starCount);
        }
    }
}
