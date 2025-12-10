using UnityEngine;

namespace Tracking4All
{
    public class PoseSolution : Solution,
        ILandmarkProvider<PoseLandmarks>, INormalizedLandmarkProvider<PoseLandmarks>, IAdapterSettings
    {
        [SerializeField] protected LandmarkProvider<PoseLandmarks> landmarkProvider;
        [SerializeField] protected LandmarkModifierStack landmarkModifiers;
        [SerializeField] protected NormalizedLandmarkProvider<PoseLandmarks> normalizedLandmarkProvider;
        [SerializeField] protected NormalizedLandmarkModifierStack normalizedLandmarkModifiers;

        /// <summary>
        /// Invoked when the landmarks update. Unity main thread safe.
        /// </summary>
        public event IProvider<PoseLandmarks, Landmark>.GroupUpdated OnLandmarksUpdated;
        /// <summary>
        /// Invoked when the landmarks update. Unity main thread safe.
        /// </summary>
        public event IProvider<PoseLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksUpdated;
        /// <summary>
        /// Invoked when landmarks stop updating ( no longer alive ).
        /// </summary>
        public event IProvider<PoseLandmarks, Landmark>.GroupUpdated OnLandmarksStopped;
        /// <summary>
        /// Invoked when landmarks stop updating ( no longer alive ).
        /// </summary>
        public event IProvider<PoseLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksStopped;

        protected override int GroupInt => 0;
        protected override int DataCount => DATA_COUNT;
        public static readonly int DATA_COUNT = Helpers.GetLength(typeof(PoseLandmarks));


        private SolutionDataComponent<PoseLandmarks, Landmark> landmarks = new();
        private SolutionDataComponent<PoseLandmarks, NormalizedLandmark> normalizedLandmarks = new();

        protected override void RegisterCallbacks()
        {
            if (landmarkProvider.HasInterface)
                landmarkProvider.OnLandmarksUpdated += LandmarkProvider_OnLandmarksUpdated;
            if (normalizedLandmarkProvider.HasInterface)
                normalizedLandmarkProvider.OnNormalizedLandmarksUpdated += NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
        }
        protected override void DeregisterCallbacks()
        {
            if (landmarkProvider.HasInterface)
                landmarkProvider.OnLandmarksStopped -= LandmarkProvider_OnLandmarksUpdated;
            if (normalizedLandmarkProvider.HasInterface)
                normalizedLandmarkProvider.OnNormalizedLandmarksStopped -= NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
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

        private void LandmarkProvider_OnLandmarksUpdated(int group)
        {
            if (group != GroupInt) return;

            // Manually store the update time ONLY for this group.
            landmarks.SetLastUpdateTime(Helpers.GetTime());
        }
        private void NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated(int group)
        {
            if (group != GroupInt) return;

            // Manually store the update time ONLY for this group.
            normalizedLandmarks.SetLastUpdateTime(Helpers.GetTime());
        }

        int IProvider<PoseLandmarks, Landmark>.DataCount => landmarks.Count;
        int IProvider<PoseLandmarks, NormalizedLandmark>.DataCount => normalizedLandmarks.Count;

        // updated each frame
        float IProvider<PoseLandmarks, NormalizedLandmark>.LastUpdateTime => normalizedLandmarks.LastLandmarkUpdateTime;
        float IProvider<PoseLandmarks, Landmark>.LastUpdateTime => landmarks.LastLandmarkUpdateTime;

        Landmark IProvider<PoseLandmarks, Landmark>.Get(int group, PoseLandmarks index)
        {
            //if (Mirror) return landmarks.Get((int)index.Mirror());

            return landmarks.Get((int)index);
        }

        Landmark IProvider<PoseLandmarks, Landmark>.Get(int group, int index)
        {
            //if (Mirror) return landmarks.Get((int)((PoseLandmarks)index).Mirror());

            return landmarks.Get((int)index);
        }

        NormalizedLandmark IProvider<PoseLandmarks, NormalizedLandmark>.Get(int group, PoseLandmarks index)
        {
            return normalizedLandmarks.Get((int)index);
        }

        NormalizedLandmark IProvider<PoseLandmarks, NormalizedLandmark>.Get(int group, int index)
        {
            return normalizedLandmarks.Get((int)index);
        }

        void IProvider<PoseLandmarks, Landmark>.DisposeProviderData(int group)
        {
            landmarkProvider.Provider.DisposeProviderData(group);
        }

        void IProvider<PoseLandmarks, NormalizedLandmark>.DisposeProviderData(int group)
        {
            normalizedLandmarkProvider.Provider.DisposeProviderData(group);
        }
    }
}