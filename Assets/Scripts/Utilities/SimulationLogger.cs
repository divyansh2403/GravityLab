using System;
using UnityEngine;

namespace GravityLab.Utilities
{
    /// <summary>
    /// Specialized logger for astronomical simulation events, state changes, and numerical warnings.
    /// </summary>
    public static class SimulationLogger
    {
        public static bool EnableLogging = true;

        public static void LogInfo(string message)
        {
            if (!EnableLogging) return;
            Debug.Log($"[GravityLab::INFO {DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        public static void LogWarning(string message)
        {
            if (!EnableLogging) return;
            Debug.LogWarning($"[GravityLab::WARN {DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        public static void LogError(string message)
        {
            Debug.LogError($"[GravityLab::ERROR {DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        public static void LogPhysicsEvent(string bodyName, string action, string details)
        {
            if (!EnableLogging) return;
            Debug.Log($"[GravityLab::PHYSICS] Body: <b>{bodyName}</b> | Action: {action} | {details}");
        }
    }
}
