using System.Collections;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using UnityEngine;

namespace Tracking4All
{
    public interface IMPUImageSourceSolution
    {
        public MPUCameraSettings CameraSettings { get; }

        public bool Restart();
    }

    [DefaultExecutionOrder(-10)]
    /// <summary>
    /// A custom graph runner to interface with MPU.
    /// </summary>
    /// <typeparam name="GRAPH_RUNNER">The concrete type of graph runner.</typeparam>
    /// <typeparam name="MODEL_SETTINGS">The concrete type of model settings for the graph.</typeparam>
    public abstract class MPUImageSourceSolution<GRAPH_RUNNER, MODEL_SETTINGS> : Mediapipe.Unity.Sample.ImageSourceSolution<GRAPH_RUNNER>,
        IMPUImageSourceSolution, IAdapterSettings
        where GRAPH_RUNNER : Mediapipe.Unity.Sample.GraphRunner
        where MODEL_SETTINGS : MPUModelSettings
    {
        [SerializeField] protected MODEL_SETTINGS modelSettings;
        [SerializeField] protected MPUCameraSettings cameraSettings;
        [SerializeField] private bool editorForceUpdate = false;

        private bool isRestarting = false;
        private string originalName;

        public bool PerspectiveFlip => cameraSettings.perspectiveFlip.Value;
        public MPUCameraSettings CameraSettings => cameraSettings;

        public bool Mirror => cameraSettings.mirror.Value;

        protected virtual void Awake()
        {
            originalName = gameObject.name;
        }
        protected virtual void OnEnable()
        {
            Tracking4All.Instance.RegisterSolution(this);
            SettingsManager.Instance.AddSetting(modelSettings);
            SettingsManager.Instance.AddSetting(cameraSettings);
        }
        protected virtual void OnDisable()
        {
            Tracking4All.Instance.DeregisterSolution(this);
            SettingsManager.Instance.RemoveSetting(modelSettings);
            SettingsManager.Instance.RemoveSetting(cameraSettings);
        }
        private void OnDestroy()
        {
            Stop(); // patch scene switching
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (editorForceUpdate)
            {
                print("Forcing solution restart!");
                Restart();
                editorForceUpdate = false;
            }
        }
#endif

        public override void Play()
        {
            ApplySettings(modelSettings, cameraSettings);

            base.Play();

            Tracking4All.Instance.OnScreenOrientationChanged += Instance_OnScreenOrientationChanged;
        }
        public override void Stop()
        {
            base.Stop();
            Tracking4All.Instance.OnScreenOrientationChanged -= Instance_OnScreenOrientationChanged;
        }

        /// <summary>
        /// Called when we want to apply settings.
        /// </summary>
        protected virtual void ApplySettings(MODEL_SETTINGS modelSettings, MPUCameraSettings cameraSettings)
        {
            UpdateModel(modelSettings);

            // update camera settings
            WebCamSource source = new WebCamSource(cameraSettings.width.Value, new ImageSource.ResolutionStruct[] { cameraSettings.Resolution });
            source.SelectSource(cameraSettings.cameraIndex.Value);
            // source.isHorizontallyFlipped = cameraSettings.mirror.Value;

            ImageSourceProvider.Initialize(source, null, null);
            ImageSourceProvider.Switch(ImageSourceType.WebCamera);

            StartCoroutine(InternalApplySettings());
        }
        /// <summary>
        /// Given the settings, update the graph values.
        /// </summary>
        /// <param name="modelSettings"></param>
        /// <param name="cameraSettings"></param>
        protected abstract void UpdateModel(MODEL_SETTINGS modelSettings);
        protected virtual IEnumerator InternalApplySettings()
        {
            yield return new WaitUntil(() => ImageSourceProvider.ImageSource.isPrepared);
            Logger.LogInfo("Requesting -- " + ImageSourceProvider.ImageSource.sourceName + " " + ImageSourceProvider.ImageSource.resolution);
            gameObject.name = originalName + " (" + ImageSourceProvider.ImageSource.sourceName + ")";
        }

        /// <summary>
        /// Apply all changes to the various settings/restart the solution.
        /// </summary>
        public virtual bool Restart()
        {
            if (isRestarting) return false;

            Logger.LogInfo("Restarting with configuration update.", gameObject.name);
            StartCoroutine(RestartInternal());

            return true;
        }
        private IEnumerator RestartInternal()
        {
            isRestarting = true;
            Stop();
            graphRunner.Stop();
            Logger.LogInfo("Waiting for image source to dispose…");
            yield return new WaitUntil(() => Mediapipe.Unity.Sample.ImageSourceProvider.ImageSource.isPrepared == false); // value that indicates we have successfully disposed
            yield return new WaitForSeconds(1f);
            Logger.LogInfo("Resuming detection.");
            Play();
            isRestarting = false;
        }

        private void Instance_OnScreenOrientationChanged(ScreenOrientation last, ScreenOrientation current)
        {
            Restart();
        }
    }
}