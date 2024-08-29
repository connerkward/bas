using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// The publicly facing hand. Sources it's data from its respective providers.
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

        private Table<Landmark> landmarks = new Table<Landmark>(Helpers.GetLength(typeof(HandLandmarks)));

        private Table<NormalizedLandmark> normalizedLandmarks =
            new Table<NormalizedLandmark>(Helpers.GetLength(typeof(HandLandmarks)));

        protected override void UpdateDatas()
        {
            UpdateData((int)handGroup, this, landmarkProvider, landmarkModifiers, landmarks);
            // print(landmarks.Get(1).Position + " vs "+ landmarkProvider.Get((int)handGroup,1).Position);
            OnLandmarksUpdated?.Invoke((int)handGroup);

            UpdateData((int)handGroup, this, normalizedLandmarkProvider, normalizedLandmarkModifiers,
                normalizedLandmarks);
            OnNormalizedLandmarksUpdated?.Invoke((int)handGroup);
        }


        // Implement through
        int IProvider<HandLandmarks, Landmark>.DataCount => landmarks.Count;
        int IProvider<HandLandmarks, NormalizedLandmark>.DataCount => landmarks.Count;
        
        float IProvider<HandLandmarks, Landmark>.TimeSinceLastUpdate => ((IProvider<HandLandmarks, Landmark>)landmarkProvider).TimeSinceLastUpdate;
        float IProvider<HandLandmarks, NormalizedLandmark>.TimeSinceLastUpdate => ((IProvider<HandLandmarks, NormalizedLandmark>)normalizedLandmarkProvider).TimeSinceLastUpdate;

        public event IProvider<HandLandmarks, Landmark>.GroupUpdated OnLandmarksUpdated;
        public event IProvider<HandLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksUpdated;

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
    }
}