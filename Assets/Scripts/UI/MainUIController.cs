using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GravityLab.Core;
using GravityLab.Utilities;
using GravityLab.Physics;

namespace GravityLab.UI
{
    /// <summary>
    /// Master UI coordinator orchestrating the Top Bar, Bottom Information Bar,
    /// Left Controls, Right Telemetry, and Modal Dialogs.
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        [Header("Top Bar")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _subtitleText;
        [SerializeField] private Text _statusBadgeText;
        [SerializeField] private Image _statusBadgeBg;
        [SerializeField] private Text _speedBadgeText;

        [Header("Bottom Information Bar")]
        [SerializeField] private Text _bodyCountText;
        [SerializeField] private Text _simTimeText;
        [SerializeField] private Text _timestepText;
        [SerializeField] private Text _fpsText;
        [SerializeField] private Text _energyConservationText;

        [Header("Toast Notification")]
        [SerializeField] private GameObject _notificationRoot;
        [SerializeField] private Text _notificationText;

        [Header("Sub-Panels")]
        [SerializeField] private SimulationControlPanel _controlPanel;
        [SerializeField] private PlanetInformationPanel _infoPanel;
        [SerializeField] private BodyCreationPanel _creationPanel;
        [SerializeField] private PhysicsEducationPanel _physicsPanel;

        [Header("Top Buttons")]
        [SerializeField] private Button _openCreateBodyTopBtn;
        [SerializeField] private Button _openPhysicsGuideTopBtn;

        private Coroutine _notificationCoroutine;

        private void Start()
        {
            if (_titleText != null) _titleText.text = "GRAVITYLAB";
            if (_subtitleText != null) _subtitleText.text = "Interactive Space & N-Body Gravity Simulator";

            if (_openCreateBodyTopBtn != null) _openCreateBodyTopBtn.onClick.AddListener(OpenCreationModal);
            if (_openPhysicsGuideTopBtn != null) _openPhysicsGuideTopBtn.onClick.AddListener(OpenPhysicsModal);

            if (SimulationManager.Instance != null)
            {
                SimulationManager.Instance.OnNotification += ShowNotification;
            }

            if (_notificationRoot != null) _notificationRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (SimulationManager.Instance != null)
            {
                SimulationManager.Instance.OnNotification -= ShowNotification;
            }
        }

        private void Update()
        {
            UpdateTopBar();
            UpdateBottomBar();
        }

        private void UpdateTopBar()
        {
            if (SimulationManager.Instance == null || SimulationManager.Instance.Simulator == null) return;
            var clock = SimulationManager.Instance.Simulator.Clock;
            var settings = SimulationManager.Instance.Simulator.Settings;

            if (_statusBadgeText != null)
            {
                if (clock.IsPaused)
                {
                    _statusBadgeText.text = "⏸ PAUSED";
                    if (_statusBadgeBg != null) _statusBadgeBg.color = new Color(0.85f, 0.55f, 0.1f, 0.85f);
                }
                else
                {
                    _statusBadgeText.text = "● RUNNING";
                    if (_statusBadgeBg != null) _statusBadgeBg.color = new Color(0.15f, 0.75f, 0.35f, 0.85f);
                }
            }

            if (_speedBadgeText != null)
            {
                _speedBadgeText.text = $"Speed: {settings.timeScale:F1}x";
            }
        }

        private void UpdateBottomBar()
        {
            if (SimulationManager.Instance == null || SimulationManager.Instance.Simulator == null) return;
            var sim = SimulationManager.Instance.Simulator;
            var clock = sim.Clock;
            var settings = sim.Settings;

            if (_bodyCountText != null)
            {
                _bodyCountText.text = $"Bodies: {sim.BodyCount}";
            }

            if (_simTimeText != null)
            {
                _simTimeText.text = $"Sim Time: {UnitConverter.FormatSimulationTime(clock.ElapsedSimulationTime)}";
            }

            if (_timestepText != null)
            {
                _timestepText.text = $"Δt: {settings.fixedTimestep:F3}s";
            }

            if (_fpsText != null)
            {
                _fpsText.text = $"FPS: {clock.CurrentFps:F0}";
            }

            if (_energyConservationText != null)
            {
                GravityCalculator.CalculateSystemEnergy(sim.ActiveBodies, settings, out float ke, out float pe);
                float totalE = ke + pe;
                _energyConservationText.text = $"Energy: {totalE:E2} EU (KE: {ke:E1} | PE: {pe:E1})";
            }
        }

        public void OpenCreationModal()
        {
            if (_creationPanel != null) _creationPanel.OpenPanel();
        }

        public void OpenPhysicsModal()
        {
            if (_physicsPanel != null) _physicsPanel.OpenPanel();
        }

        public void ShowNotification(string message)
        {
            if (_notificationCoroutine != null) StopCoroutine(_notificationCoroutine);
            _notificationCoroutine = StartCoroutine(NotificationRoutine(message));
        }

        private IEnumerator NotificationRoutine(string msg)
        {
            if (_notificationRoot != null && _notificationText != null)
            {
                _notificationText.text = msg;
                _notificationRoot.SetActive(true);
                yield return new WaitForSeconds(3.5f);
                _notificationRoot.SetActive(false);
            }
        }
    }
}
