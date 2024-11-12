using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    [DefaultExecutionOrder(-100)]
    /// <summary>
    /// Tracking4All manager. Exactly one should be in the scene at all timesThat
    /// </summary>
    public class Tracking4All : MonoBehaviour
    {
        /// <summary>
        /// Access the singleton instance of Tracking4All.
        /// </summary>
        public static Tracking4All Instance;

        [SerializeField] private float providerAliveThreshold = 1f; // How long can a provider pause before its considered dead/asleep.
        /// <summary>
        /// Returns true if the provider is considered alive.
        /// </summary>
        /// <param name="timeSinceLastUpdate">The time since update value from the provider. Not absolute time.</param>
        /// <returns></returns>
        public bool IsProviderLive(float timeSinceLastUpdate)
        {
            return timeSinceLastUpdate < providerAliveThreshold;
        }
        public float ProviderAliveThreshold => providerAliveThreshold;

        public delegate void ScreenOrientationChanged(ScreenOrientation last, ScreenOrientation current);
        public event ScreenOrientationChanged OnScreenOrientationChanged;
        public ScreenOrientation ScreenOrientation { get; private set; }
        private ScreenOrientation lastOrientation;

        private static System.Diagnostics.Stopwatch watch;
        /// <summary>
        /// Get the elapsed time in seconds since Tracking4All was initialized.
        /// </summary>
        /// <returns></returns>
        public static float GetElapsedTime()
        {
            return (float)watch.Elapsed.TotalSeconds;
        }

        private List<IMPUImageSourceSolution> imageSourceSolutions = new();
        public void RegisterSolution(IMPUImageSourceSolution solution)
        {
            imageSourceSolutions.Add(solution);
        }
        public void DeregisterSolution(IMPUImageSourceSolution solution)
        {
            imageSourceSolutions.Remove(solution);
        }
        public void RestartSolutions()
        {
            foreach (var i in imageSourceSolutions)
            {
                if (i == null) continue;
                i.Restart();
            }
        }

        public GameObject GENERATED { get; protected set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Logger.LogError("There should only be one Tracking4All script in a scene at a time.");
            }

            lastOrientation = UnityEngine.Screen.orientation;
            Instance = this;
            watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            GENERATED = new GameObject("--Tracking4All Generated--");
        }

        private void Update()
        {
            ScreenOrientation current = UnityEngine.Screen.orientation;
            ScreenOrientation = current;

            if (lastOrientation != current)
            {
                OnScreenOrientationChanged?.Invoke(lastOrientation, current);
            }

            lastOrientation = current;
        }

        public void ProcessCommand(string s)
        {
            switch (s)
            {
                case "ori":
                    Logger.LogError(ScreenOrientation);
                    break;
            }

            if (s.Contains("scene"))
            {
                int i = -1;
                if (int.TryParse(s.Replace("scene", "").Replace(" ", ""), out i))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(i);
                }
            }
        }

        private IMPUImageSourceSolution GetMPUSolution()
        {
            // temporary measures to control camera
            IMPUImageSourceSolution solution = FindObjectOfType<MPUHandsTrackingSolution>();
            if (solution == null) solution = FindObjectOfType<MPUPoseTrackingSolution>();

            return solution;
        }
    }
}