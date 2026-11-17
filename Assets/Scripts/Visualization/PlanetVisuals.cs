using UnityEngine;

namespace GravityLab.Visualization
{
    /// <summary>
    /// Handles procedural rendering, materials, axial planetary rotation,
    /// planetary rings (for Saturn), and selection highlights.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class PlanetVisuals : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _meshFilter;

        [Header("Rotational Dynamics")]
        [SerializeField] private float _spinSpeed = 15f; // degrees per second
        [SerializeField] private Vector3 _spinAxis = Vector3.up;

        [Header("Selection Effects")]
        [SerializeField] private GameObject _selectionMarker;

        private Material _planetMaterial;
        private bool _isStar = false;
        private Light _stellarLight;
        private GameObject _ringObject;

        private void Awake()
        {
            EnsureMeshComponents();
        }

        private void Update()
        {
            // Axial planetary rotation
            transform.Rotate(_spinAxis, _spinSpeed * Time.deltaTime, Space.Self);
        }

        public void EnsureMeshComponents()
        {
            if (_renderer == null) _renderer = GetComponent<MeshRenderer>();
            if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
        }

        /// <summary>
        /// Configures procedural material, colors, star emission, and optional planetary rings.
        /// </summary>
        public void InitializeVisuals(Color color, bool isStar, bool hasRings = false, Color ringColor = default)
        {
            EnsureMeshComponents();
            _isStar = isStar;

            // Use Built-in Standard or Mobile shader
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");
            if (shader == null) shader = Shader.Find("Diffuse");

            _planetMaterial = new Material(shader);
            _planetMaterial.color = color;

            if (_isStar)
            {
                // Emissive glowing star material
                _planetMaterial.EnableKeyword("_EMISSION");
                _planetMaterial.SetColor("_EmissionColor", color * 1.5f);
                _planetMaterial.SetFloat("_Glossiness", 0.0f);

                // Add or configure light source
                if (_stellarLight == null)
                {
                    _stellarLight = GetComponentInChildren<Light>();
                    if (_stellarLight == null)
                    {
                        GameObject lightObj = new GameObject("StellarPointLight");
                        lightObj.transform.SetParent(transform, false);
                        _stellarLight = lightObj.AddComponent<Light>();
                    }
                }

                _stellarLight.type = LightType.Point;
                _stellarLight.color = color;
                _stellarLight.range = 350.0f;
                _stellarLight.intensity = 1.8f;
                _stellarLight.shadows = LightShadows.None;
            }
            else
            {
                _planetMaterial.SetFloat("_Glossiness", 0.35f);
                _planetMaterial.SetFloat("_Metallic", 0.05f);
            }

            _renderer.sharedMaterial = _planetMaterial;

            // Saturn ring creation
            if (hasRings && _ringObject == null)
            {
                CreatePlanetaryRings(ringColor == default ? new Color(0.85f, 0.8f, 0.65f, 0.6f) : ringColor);
            }
        }

        private void CreatePlanetaryRings(Color ringColor)
        {
            _ringObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _ringObject.name = "PlanetaryRings";
            _ringObject.transform.SetParent(transform, false);
            // Flatten cylinder into a thin disc
            _ringObject.transform.localScale = new Vector3(2.4f, 0.015f, 2.4f);
            _ringObject.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);

            // Remove collider from visual ring
            Collider ringCol = _ringObject.GetComponent<Collider>();
            if (ringCol != null) DestroyImmediate(ringCol);

            MeshRenderer ringRen = _ringObject.GetComponent<MeshRenderer>();
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");

            Material ringMat = new Material(shader);
            ringMat.color = ringColor;
            ringRen.sharedMaterial = ringMat;
        }

        /// <summary>
        /// Displays or hides the visual target selection marker.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            if (isSelected)
            {
                if (_selectionMarker == null)
                {
                    CreateSelectionMarker();
                }
                _selectionMarker.SetActive(true);
            }
            else
            {
                if (_selectionMarker != null)
                {
                    _selectionMarker.SetActive(false);
                }
            }
        }

        private void CreateSelectionMarker()
        {
            _selectionMarker = new GameObject("SelectionMarker");
            _selectionMarker.transform.SetParent(transform, false);
            _selectionMarker.transform.localPosition = Vector3.zero;

            LineRenderer line = _selectionMarker.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
            line.loop = true;
            line.positionCount = 36;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            Shader unlit = Shader.Find("Sprites/Default");
            if (unlit == null) unlit = Shader.Find("Mobile/Particles/Additive");
            if (unlit == null) unlit = Shader.Find("Standard");

            Material mat = new Material(unlit);
            mat.color = new Color(0.2f, 0.85f, 1.0f, 0.85f);
            line.sharedMaterial = mat;

            // Generate circular reticle
            float radius = 1.35f;
            for (int i = 0; i < 36; i++)
            {
                float angle = i * Mathf.PI * 2f / 36f;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }
    }
}
