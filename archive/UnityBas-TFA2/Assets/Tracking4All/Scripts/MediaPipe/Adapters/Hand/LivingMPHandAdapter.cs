using UnityEngine;

namespace Tracking4All
{
    public class LivingMPHandAdapter : LivingAdapter, IAdapterSettings,
        ILandmarkProvider<HandLandmarks>, INormalizedLandmarkProvider<HandLandmarks>
    {
        [SerializeField] protected LandmarkProvider<MPHandLandmarks> landmarkProvider;
        [SerializeField] private NormalizedLandmarkProvider<MPHandLandmarks> normalizedLandmarkProvider;
        [SerializeField] private AdapterSettingsProvider adapterSettings;

        private MPHandLandmarkAdapter landmarkAdapter;
        private MPHandNormalizedLandmarkAdapter normalizedLandmarkAdapter;

        private void Awake()
        {
            landmarkAdapter =
                new MPHandLandmarkAdapter(adapterSettings, Helpers.GetLength(typeof(Handedness)));
            normalizedLandmarkAdapter = new MPHandNormalizedLandmarkAdapter(adapterSettings,
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
            if (IsLandmarkUpdatePassed(group)) landmarkAdapter.Update(group, landmarkProvider);
        }
        private void NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated(int group)
        {
            normalizedLandmarkAdapter.Update(group, normalizedLandmarkProvider);
        }

        protected virtual bool IsLandmarkUpdatePassed(int group)
        {
            return true;
        }

        // implement through adapter
        int IProvider<HandLandmarks, Landmark>.DataCount => landmarkAdapter.DataCount;
        int IProvider<HandLandmarks, NormalizedLandmark>.DataCount => normalizedLandmarkAdapter.DataCount;

        float IProvider<HandLandmarks, Landmark>.LastUpdateTime => landmarkAdapter.LastUpdateTime;
        float IProvider<HandLandmarks, NormalizedLandmark>.LastUpdateTime => normalizedLandmarkAdapter.LastUpdateTime;

        public bool PerspectiveFlip => ((IAdapterSettings)adapterSettings).PerspectiveFlip;

        public bool Mirror => ((IAdapterSettings)adapterSettings).Mirror;

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

        public event IProvider<HandLandmarks, Landmark>.GroupUpdated OnLandmarksStopped
        {
            add
            {
                ((ILandmarkProvider<HandLandmarks>)landmarkAdapter).OnLandmarksStopped += value;
            }

            remove
            {
                ((ILandmarkProvider<HandLandmarks>)landmarkAdapter).OnLandmarksStopped -= value;
            }
        }

        public event IProvider<HandLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksStopped
        {
            add
            {
                ((INormalizedLandmarkProvider<HandLandmarks>)normalizedLandmarkAdapter).OnNormalizedLandmarksStopped += value;
            }

            remove
            {
                ((INormalizedLandmarkProvider<HandLandmarks>)normalizedLandmarkAdapter).OnNormalizedLandmarksStopped -= value;
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

        void IProvider<HandLandmarks, Landmark>.DisposeProviderData(int group)
        {
            landmarkAdapter.DisposeProviderData(group);
        }

        void IProvider<HandLandmarks, NormalizedLandmark>.DisposeProviderData(int group)
        {
            normalizedLandmarkAdapter.DisposeProviderData(group);
        }
    }
}