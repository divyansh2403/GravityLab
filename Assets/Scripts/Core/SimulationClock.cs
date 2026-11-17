using System;
using UnityEngine;

namespace GravityLab.Core
{
    /// <summary>
    /// Manages simulation time progression, fixed-timestep accumulation, sub-stepping,
    /// play/pause states, and rendering framerate diagnostics.
    /// </summary>
    public class SimulationClock
    {
        private float _elapsedSimulationTime = 0f;
        private long _totalStepsTaken = 0;
        private bool _isPaused = false;
        private float _accumulator = 0f;

        // Frame rate calculation
        private float _fpsTimer = 0f;
        private int _fpsFrames = 0;
        private float _currentFps = 60f;

        public event Action<bool> OnPauseStateChanged;
        public event Action OnClockReset;

        public float ElapsedSimulationTime => _elapsedSimulationTime;
        public long TotalStepsTaken => _totalStepsTaken;
        public bool IsPaused => _isPaused;
        public float CurrentFps => _currentFps;

        public void Play()
        {
            if (_isPaused)
            {
                _isPaused = false;
                OnPauseStateChanged?.Invoke(false);
            }
        }

        public void Pause()
        {
            if (!_isPaused)
            {
                _isPaused = true;
                OnPauseStateChanged?.Invoke(true);
            }
        }

        public void TogglePause()
        {
            if (_isPaused) Play();
            else Pause();
        }

        public void ResetClock()
        {
            _elapsedSimulationTime = 0f;
            _totalStepsTaken = 0;
            _accumulator = 0f;
            OnClockReset?.Invoke();
        }

        /// <summary>
        /// Updates the simulation clock by accumulating real deltaTime multiplied by timeScale,
        /// executing the physics integration callback in stable, deterministic fixed slices.
        /// </summary>
        public void Tick(float unscaledDeltaTime, SimulationSettings settings, Action<float> onFixedPhysicsStep)
        {
            UpdateFps(unscaledDeltaTime);

            if (_isPaused || onFixedPhysicsStep == null || settings == null)
                return;

            float dt = Mathf.Clamp(unscaledDeltaTime, 0f, 0.1f);
            float scaledDt = dt * settings.timeScale;
            _accumulator += scaledDt;

            float fixedStep = settings.fixedTimestep;
            int subStepCount = 0;

            while (_accumulator >= fixedStep && subStepCount < settings.maxSubStepsPerFrame)
            {
                onFixedPhysicsStep(fixedStep);
                _elapsedSimulationTime += fixedStep;
                _totalStepsTaken++;
                _accumulator -= fixedStep;
                subStepCount++;
            }

            // Prevent accumulator debt buildup if computer falls behind
            if (_accumulator > fixedStep * 2f)
            {
                _accumulator = 0f;
            }
        }

        private void UpdateFps(float dt)
        {
            _fpsFrames++;
            _fpsTimer += dt;
            if (_fpsTimer >= 0.5f)
            {
                _currentFps = _fpsFrames / _fpsTimer;
                _fpsFrames = 0;
                _fpsTimer = 0f;
            }
        }
    }
}
