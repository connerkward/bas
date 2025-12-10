using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// The publicly facing hand. Sources it's data from its respective providers.
    /// <para>A hand solution is for exactly 1 hand either right or left.</para>
    /// </summary>
    public class HandSolution : Solution,
        ILandmarkProvider<HandLandmarks>, INormalizedLandmarkProvider<HandLandmarks>
    {
        [SerializeField] protected Handedness handGroup;
        [SerializeField] protected LandmarkProvider<HandLandmarks> landmarkProvider;
        [SerializeField] protected LandmarkModifierStack landmarkModifiers;
        [SerializeField] protected NormalizedLandmarkProvider<HandLandmarks> normalizedLandmarkProvider;
        [SerializeField] protected NormalizedLandmarkModifierStack normalizedLandmarkModifiers;

        public Handedness HandGroup => handGroup;
        protected override int GroupInt => (int)HandGroup;
        protected override int DataCount => DATA_COUNT;
        public static readonly int DATA_COUNT = Helpers.GetLength(typeof(HandLandmarks));

        private SolutionDataComponent<HandLandmarks, Landmark> landmarks = new();
        private SolutionDataComponent<HandLandmarks, NormalizedLandmark> normalizedLandmarks = new();

        protected override void RegisterCallbacks()
        {
            if (landmarkProvider.HasInterface)
            {
                landmarkProvider.OnLandmarksUpdated += LandmarkProvider_OnLandmarksUpdated;
            }
            if (normalizedLandmarkProvider.HasInterface)
            {
                normalizedLandmarkProvider.OnNormalizedLandmarksUpdated += NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
            }
        }
        protected override void DeregisterCallbacks()
        {
            if (landmarkProvider.HasInterface)
            {
                landmarkProvider.OnLandmarksUpdated -= LandmarkProvider_OnLandmarksUpdated;
            }
            if (normalizedLandmarkProvider.HasInterface)
            {
                normalizedLandmarkProvider.OnNormalizedLandmarksUpdated -= NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
            }
        }

        protected override void UpdateDatas()
        {
            if (landmarkProvider.HasInterface)
                landmarks.UpdateSolution(this, GroupInt, DataCount, this, landmarkProvider, landmarkModifiers,
                    OnLandmarksUpdated, OnLandmarksStopped);

            if (normalizedLandmarkProvider.HasInterface)
                normalizedLandmarks.UpdateSolution(this, GroupInt, DataCount, this, normalizedLandmarkProvider,
                    normalizedLandmarkModifiers, OnNormalizedLandmarksUpdated, OnNormalizedLandmarksStopped);
        }

        // Invocations through these are not unity thread safe.
        private void LandmarkProvider_OnLandmarksUpdated(int group)
        {
            if (group != GroupInt) return;

            // Manually store the update time ONLY for this group.
            landmarks.SetLastUpdateTime(Helpers.GetTime());
        }
        private void NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated(int group)
        {
            if (group != GroupInt) return;

            normalizedLandmarks.SetLastUpdateTime(Helpers.GetTime());
        }

        // Implement through
        int IProvider<HandLandmarks, Landmark>.DataCount => landmarks.Count;
        int IProvider<HandLandmarks, NormalizedLandmark>.DataCount => landmarks.Count;

        // Returns the sources detection rate rather then delta time.
        float IProvider<HandLandmarks, Landmark>.LastUpdateTime => landmarks.LastLandmarkUpdateTime;
        float IProvider<HandLandmarks, NormalizedLandmark>.LastUpdateTime => landmarks.LastLandmarkUpdateTime;

        public event IProvider<HandLandmarks, Landmark>.GroupUpdated OnLandmarksUpdated;
        public event IProvider<HandLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksUpdated;
        public event IProvider<HandLandmarks, Landmark>.GroupUpdated OnLandmarksStopped;
        public event IProvider<HandLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksStopped;

        Landmark IProvider<HandLandmarks, Landmark>.Get(int group, HandLandmarks index)
        {
            return landmarks.Get((int)index);
        }

        Landmark IProvider<HandLandmarks, Landmark>.Get(int group, int index)
        {
            return landmarks.Get((int)index);
        }

        NormalizedLandmark IProvider<HandLandmarks, NormalizedLandmark>.Get(int group, HandLandmarks index)
        {
            return normalizedLandmarks.Get((int)index);
        }

        NormalizedLandmark IProvider<HandLandmarks, NormalizedLandmark>.Get(int group, int index)
        {
            return normalizedLandmarks.Get((int)index);
        }

        void IProvider<HandLandmarks, Landmark>.DisposeProviderData(int group)
        {
            landmarkProvider.Provider.DisposeProviderData(group);
        }
        void IProvider<HandLandmarks, NormalizedLandmark>.DisposeProviderData(int group)
        {
            normalizedLandmarkProvider.Provider.DisposeProviderData(group);
        }

    }
}