using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Wrapper around MPPoseLandmarkAdapter to make it a living unity component.
    /// </summary>
    public class LivingMPPoseAdapter : MonoBehaviour,
        ILandmarkProvider<PoseLandmarks>, INormalizedLandmarkProvider<PoseLandmarks>
    {
        [SerializeField] private LandmarkProvider<MPPoseLandmarks> landmarkProvider;
        [SerializeField] private InterfaceProvider<IAdapterSettings> landmarkSettings;
        [SerializeField] private NormalizedLandmarkProvider<MPPoseLandmarks> normalizedLandmarkProvider;
        [SerializeField] private InterfaceProvider<IAdapterSettings> normalizedLandmarkSettings;

        private MPPoseLandmarkAdapter landmarkAdapter;
        private MPPoseNormalizedLandmarkAdapter normalizedLandmarkAdapter;

        private void Awake()
        {
            landmarkAdapter = new MPPoseLandmarkAdapter(landmarkSettings.Provider, 1);
            normalizedLandmarkAdapter = new MPPoseNormalizedLandmarkAdapter(normalizedLandmarkSettings.Provider, 1);
        }

        private void Start()
        {
            if (landmarkProvider.HasInterface) landmarkProvider.OnLandmarksUpdated += Provider_OnLandmarksUpdated;
            if (normalizedLandmarkProvider.HasInterface)
                normalizedLandmarkProvider.OnNormalizedLandmarksUpdated +=
                    NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
        }

        private void OnDestroy()
        {
            if (landmarkProvider.HasInterface) landmarkProvider.OnLandmarksUpdated -= Provider_OnLandmarksUpdated;
            if (normalizedLandmarkProvider.HasInterface)
                normalizedLandmarkProvider.OnNormalizedLandmarksUpdated -=
                    NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
        }

        private void Provider_OnLandmarksUpdated(int group)
        {
            landmarkAdapter.Update(group, landmarkProvider);
        }

        private void NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated(int group)
        {
            normalizedLandmarkAdapter.Update(group, normalizedLandmarkProvider);
        }


        // implement through adapter
        int IProvider<PoseLandmarks, Landmark>.DataCount => ((IProvider<PoseLandmarks, Landmark>)landmarkAdapter).DataCount;
        int IProvider<PoseLandmarks, NormalizedLandmark>.DataCount => ((IProvider<PoseLandmarks, NormalizedLandmark>)normalizedLandmarkAdapter).DataCount;

        float IProvider<PoseLandmarks, Landmark>.TimeSinceLastUpdate => ((IProvider<PoseLandmarks, Landmark>)landmarkAdapter).TimeSinceLastUpdate;
        float IProvider<PoseLandmarks, NormalizedLandmark>.TimeSinceLastUpdate => ((IProvider<PoseLandmarks, NormalizedLandmark>)normalizedLandmarkAdapter).TimeSinceLastUpdate;

        public event IProvider<PoseLandmarks, Landmark>.GroupUpdated OnLandmarksUpdated
        {
            add { ((ILandmarkProvider<PoseLandmarks>)landmarkAdapter).OnLandmarksUpdated += value; }

            remove { ((ILandmarkProvider<PoseLandmarks>)landmarkAdapter).OnLandmarksUpdated -= value; }
        }

        public event IProvider<PoseLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksUpdated
        {
            add
            {
                ((INormalizedLandmarkProvider<PoseLandmarks>)normalizedLandmarkAdapter).OnNormalizedLandmarksUpdated +=
                    value;
            }

            remove
            {
                ((INormalizedLandmarkProvider<PoseLandmarks>)normalizedLandmarkAdapter).OnNormalizedLandmarksUpdated -=
                    value;
            }
        }

        public Landmark Get(int group, PoseLandmarks index)
        {
            return ((IProvider<PoseLandmarks, Landmark>)landmarkAdapter).Get(group, index);
        }

        public Landmark Get(int group, int index)
        {
            return ((IProvider<PoseLandmarks, Landmark>)landmarkAdapter).Get(group, index);
        }

        NormalizedLandmark IProvider<PoseLandmarks, NormalizedLandmark>.Get(int group, PoseLandmarks index)
        {
            return ((IProvider<PoseLandmarks, NormalizedLandmark>)normalizedLandmarkAdapter).Get(group, index);
        }

        NormalizedLandmark IProvider<PoseLandmarks, NormalizedLandmark>.Get(int group, int index)
        {
            return ((IProvider<PoseLandmarks, NormalizedLandmark>)normalizedLandmarkAdapter).Get(group, index);
        }
    }
}