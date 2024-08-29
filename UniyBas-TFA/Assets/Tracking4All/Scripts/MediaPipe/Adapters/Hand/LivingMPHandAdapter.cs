using UnityEngine;

namespace Tracking4All
{
    public class LivingMPHandAdapter : MonoBehaviour,
        ILandmarkProvider<HandLandmarks>, INormalizedLandmarkProvider<HandLandmarks>
    {
        [SerializeField] private LandmarkProvider<MPHandLandmarks> landmarkProvider;
        [SerializeField] private InterfaceProvider<IAdapterSettings> landmarkSettings;
        [SerializeField] private NormalizedLandmarkProvider<MPHandLandmarks> normalizedLandmarkProvider;
        [SerializeField] private InterfaceProvider<IAdapterSettings> normalizedLandmarkSettings;

        private MPHandLandmarkAdapter landmarkAdapter;
        private MPHandNormalizedLandmarkAdapter normalizedLandmarkAdapter;

        int IProvider<HandLandmarks, Landmark>.DataCount => landmarkAdapter.DataCount;
        int IProvider<HandLandmarks, NormalizedLandmark>.DataCount => normalizedLandmarkAdapter.DataCount;

        float IProvider<HandLandmarks, Landmark>.TimeSinceLastUpdate => ((IProvider<HandLandmarks, Landmark>)landmarkAdapter).TimeSinceLastUpdate;
        float IProvider<HandLandmarks, NormalizedLandmark>.TimeSinceLastUpdate => ((IProvider<HandLandmarks, NormalizedLandmark>)normalizedLandmarkAdapter).TimeSinceLastUpdate;

        private void Awake()
        {
            landmarkAdapter =
                new MPHandLandmarkAdapter(landmarkSettings.Provider, Helpers.GetLength(typeof(Handedness)));
            normalizedLandmarkAdapter = new MPHandNormalizedLandmarkAdapter(normalizedLandmarkSettings.Provider,
                Helpers.GetLength(typeof(Handedness)));
        }

        private void OnEnable()
        {
            if (landmarkProvider.HasInterface) landmarkProvider.OnLandmarksUpdated += Provider_OnLandmarksUpdated;
            if (normalizedLandmarkProvider.HasInterface)
                normalizedLandmarkProvider.OnNormalizedLandmarksUpdated +=
                    NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
        }

        private void OnDisable()
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

        public event IProvider<HandLandmarks, Landmark>.GroupUpdated OnLandmarksUpdated
        {
            add { ((ILandmarkProvider<HandLandmarks>)landmarkAdapter).OnLandmarksUpdated += value; }

            remove { ((ILandmarkProvider<HandLandmarks>)landmarkAdapter).OnLandmarksUpdated -= value; }
        }

        public event IProvider<HandLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksUpdated
        {
            add
            {
                ((INormalizedLandmarkProvider<HandLandmarks>)normalizedLandmarkAdapter).OnNormalizedLandmarksUpdated +=
                    value;
            }

            remove
            {
                ((INormalizedLandmarkProvider<HandLandmarks>)normalizedLandmarkAdapter).OnNormalizedLandmarksUpdated -=
                    value;
            }
        }

        public Landmark Get(int group, HandLandmarks index)
        {
            return ((IProvider<HandLandmarks, Landmark>)landmarkAdapter).Get(group, index);
        }

        public Landmark Get(int group, int index)
        {
            return ((IProvider<HandLandmarks, Landmark>)landmarkAdapter).Get(group, index);
        }

        NormalizedLandmark IProvider<HandLandmarks, NormalizedLandmark>.Get(int group, HandLandmarks index)
        {
            return ((IProvider<HandLandmarks, NormalizedLandmark>)normalizedLandmarkAdapter).Get(group, index);
        }

        NormalizedLandmark IProvider<HandLandmarks, NormalizedLandmark>.Get(int group, int index)
        {
            return ((IProvider<HandLandmarks, NormalizedLandmark>)normalizedLandmarkAdapter).Get(group, index);
        }
    }
}