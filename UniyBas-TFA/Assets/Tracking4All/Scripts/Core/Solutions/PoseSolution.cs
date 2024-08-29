using UnityEngine;

namespace Tracking4All
{
    public class PoseSolution : Solution,
        ILandmarkProvider<PoseLandmarks>, INormalizedLandmarkProvider<PoseLandmarks>
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

        private Table<Landmark> landmarks = new Table<Landmark>(Helpers.GetLength(typeof(PoseLandmarks)));

        private Table<NormalizedLandmark> normalizedLandmarks =
            new Table<NormalizedLandmark>(Helpers.GetLength(typeof(PoseLandmarks)));

        protected override void UpdateDatas()
        {
            if (landmarkProvider.HasInterface&&landmarkProvider.IsAlive)
            {
                UpdateData(0, this, landmarkProvider, landmarkModifiers, landmarks);
                OnLandmarksUpdated?.Invoke(0);
            }

            if (normalizedLandmarkProvider.HasInterface&&normalizedLandmarkProvider.IsAlive)
            {
                UpdateData(0, this, normalizedLandmarkProvider, normalizedLandmarkModifiers, normalizedLandmarks);
                OnNormalizedLandmarksUpdated?.Invoke(0);
            }
        }

        int IProvider<PoseLandmarks, Landmark>.DataCount => landmarks.Count;
        int IProvider<PoseLandmarks, NormalizedLandmark>.DataCount => normalizedLandmarks.Count;

        float IProvider<PoseLandmarks, NormalizedLandmark>.TimeSinceLastUpdate => ((IProvider<PoseLandmarks, NormalizedLandmark>)normalizedLandmarkProvider).TimeSinceLastUpdate;
        float IProvider<PoseLandmarks, Landmark>.TimeSinceLastUpdate => ((IProvider<PoseLandmarks, Landmark>)landmarkProvider).TimeSinceLastUpdate;

        Landmark IProvider<PoseLandmarks, Landmark>.Get(int group, PoseLandmarks index)
        {
            return landmarks.Get((int)index);
        }

        Landmark IProvider<PoseLandmarks, Landmark>.Get(int group, int index)
        {
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
    }
}