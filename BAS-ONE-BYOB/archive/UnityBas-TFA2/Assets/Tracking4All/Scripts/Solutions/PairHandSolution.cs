// PairHandSolution
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Allows you to access 2 seperate hand providers as 1 pair.
    /// </summary>
    public class PairHandSolution : MonoBehaviour, IAdapterSettings,
        ILandmarkProvider<HandLandmarks>, INormalizedLandmarkProvider<HandLandmarks>
    {
        // Combines 2 hands and allows you to access them through 1 provider interface.

        // Required to support mirroring convention.

        [SerializeField] protected AdapterSettingsProvider adapterSettings;
        [SerializeField] protected LandmarkProvider<HandLandmarks> rightLandmarkProvider;
        [SerializeField] protected NormalizedLandmarkProvider<HandLandmarks> rightNormalizedLandmarkProvider;
        [SerializeField] protected LandmarkProvider<HandLandmarks> leftLandmarkProvider;
        [SerializeField] protected NormalizedLandmarkProvider<HandLandmarks> leftNormalizedLandmarkProvider;

        int IProvider<HandLandmarks, Landmark>.DataCount => rightLandmarkProvider.Provider.DataCount;
        int IProvider<HandLandmarks, NormalizedLandmark>.DataCount => rightNormalizedLandmarkProvider.Provider.DataCount;

        float IProvider<HandLandmarks, Landmark>.LastUpdateTime => lastLandmarkUpdateTime;
        float IProvider<HandLandmarks, NormalizedLandmark>.LastUpdateTime => lastNormalizedLandmarkUpdateTime;

        public bool PerspectiveFlip => ((IAdapterSettings)adapterSettings).PerspectiveFlip;
        public bool Mirror => ((IAdapterSettings)adapterSettings).Mirror;

        public event IProvider<HandLandmarks, Landmark>.GroupUpdated OnLandmarksUpdated;
        public event IProvider<HandLandmarks, Landmark>.GroupUpdated OnLandmarksStopped;
        public event IProvider<HandLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksUpdated;
        public event IProvider<HandLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksStopped;

        private float lastLandmarkUpdateTime;
        private float lastNormalizedLandmarkUpdateTime;

        protected void OnEnable()
        {
            if (rightLandmarkProvider.HasInterface)
            {
                rightLandmarkProvider.OnLandmarksUpdated += LandmarkProvider_OnLandmarksUpdated;
                rightLandmarkProvider.OnLandmarksStopped += LandmarkProvider_OnLandmarksStopped;
            }
            if (rightNormalizedLandmarkProvider.HasInterface)
            {
                rightNormalizedLandmarkProvider.OnNormalizedLandmarksUpdated += NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
                rightLandmarkProvider.OnLandmarksStopped += NormalizedLandmarkProvider_OnLandmarksStopped;
            }

            if (leftLandmarkProvider.HasInterface)
            {
                leftLandmarkProvider.OnLandmarksUpdated += LandmarkProvider_OnLandmarksUpdated;
                leftLandmarkProvider.OnLandmarksStopped += LandmarkProvider_OnLandmarksStopped;
            }
            if (leftNormalizedLandmarkProvider.HasInterface)
            {
                leftNormalizedLandmarkProvider.OnNormalizedLandmarksUpdated += NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
                leftLandmarkProvider.OnLandmarksStopped += NormalizedLandmarkProvider_OnLandmarksStopped;
            }

        }
        protected void OnDisable()
        {
            if (rightLandmarkProvider.HasInterface)
            {
                rightLandmarkProvider.OnLandmarksUpdated -= LandmarkProvider_OnLandmarksUpdated;
                rightLandmarkProvider.OnLandmarksStopped -= LandmarkProvider_OnLandmarksStopped;
            }
            if (rightNormalizedLandmarkProvider.HasInterface)
            {
                rightNormalizedLandmarkProvider.OnNormalizedLandmarksUpdated -= NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
                rightLandmarkProvider.OnLandmarksStopped -= NormalizedLandmarkProvider_OnLandmarksStopped;
            }

            if (leftLandmarkProvider.HasInterface)
            {
                leftLandmarkProvider.OnLandmarksUpdated -= LandmarkProvider_OnLandmarksUpdated;
                leftLandmarkProvider.OnLandmarksStopped -= LandmarkProvider_OnLandmarksStopped;
            }
            if (leftNormalizedLandmarkProvider.HasInterface)
            {
                leftNormalizedLandmarkProvider.OnNormalizedLandmarksUpdated -= NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated;
                leftLandmarkProvider.OnLandmarksStopped -= NormalizedLandmarkProvider_OnLandmarksStopped;
            }
        }

        private void LandmarkProvider_OnLandmarksUpdated(int group)
        {
            lastLandmarkUpdateTime = Helpers.GetTime();
            OnLandmarksUpdated?.Invoke(group);
        }
        private void LandmarkProvider_OnLandmarksStopped(int group)
        {
            OnLandmarksStopped?.Invoke(group);
        }

        private void NormalizedLandmarkProvider_OnNormalizedLandmarksUpdated(int group)
        {
            lastNormalizedLandmarkUpdateTime = Helpers.GetTime();
            OnNormalizedLandmarksUpdated?.Invoke(group);
        }
        private void NormalizedLandmarkProvider_OnLandmarksStopped(int group)
        {
            OnNormalizedLandmarksStopped?.Invoke(group);
        }



        // Mirror behavior: flip the group when accessing if mirroring is enabled.
        void IProvider<HandLandmarks, Landmark>.DisposeProviderData(int group)
        {
            if (adapterSettings.Mirror)
            {
                group = (int)((Handedness)group).Flip();
            }

            switch ((Handedness)group)
            {
                case Handedness.LEFT:
                    leftLandmarkProvider.Provider.DisposeProviderData(group);
                    break;
                case Handedness.RIGHT:
                    rightLandmarkProvider.Provider.DisposeProviderData(group);
                    break;
            }
        }
        Landmark IProvider<HandLandmarks, Landmark>.Get(int group, HandLandmarks index)
        {
            if (adapterSettings.Mirror)
            {
                group = (int)((Handedness)group).Flip();
            }

            switch ((Handedness)group)
            {
                case Handedness.LEFT:
                    return leftLandmarkProvider.Get(group, index);
                case Handedness.RIGHT:
                    return rightLandmarkProvider.Get(group, index);
            }

            throw new System.NotImplementedException();
        }
        Landmark IProvider<HandLandmarks, Landmark>.Get(int group, int index)
        {
            if (adapterSettings.Mirror)
            {
                group = (int)((Handedness)group).Flip();
            }

            switch ((Handedness)group)
            {
                case Handedness.LEFT:
                    return leftLandmarkProvider.Get(group, index);
                case Handedness.RIGHT:
                    return rightLandmarkProvider.Get(group, index);
            }

            throw new System.NotImplementedException();
        }

        void IProvider<HandLandmarks, NormalizedLandmark>.DisposeProviderData(int group)
        {
            if (adapterSettings.Mirror)
            {
                group = (int)((Handedness)group).Flip();
            }

            switch ((Handedness)group)
            {
                case Handedness.LEFT:
                    leftNormalizedLandmarkProvider.Provider.DisposeProviderData(group);
                    break;
                case Handedness.RIGHT:
                    rightNormalizedLandmarkProvider.Provider.DisposeProviderData(group);
                    break;
            }
        }
        NormalizedLandmark IProvider<HandLandmarks, NormalizedLandmark>.Get(int group, HandLandmarks index)
        {
            if (adapterSettings.Mirror)
            {
                group = (int)((Handedness)group).Flip();
            }

            switch ((Handedness)group)
            {
                case Handedness.LEFT:
                    return leftNormalizedLandmarkProvider.Get(group, index);
                case Handedness.RIGHT:
                    return rightNormalizedLandmarkProvider.Get(group, index);
            }

            throw new System.NotImplementedException();
        }
        NormalizedLandmark IProvider<HandLandmarks, NormalizedLandmark>.Get(int group, int index)
        {
            if (adapterSettings.Mirror)
            {
                group = (int)((Handedness)group).Flip();
            }

            switch ((Handedness)group)
            {
                case Handedness.LEFT:
                    return leftNormalizedLandmarkProvider.Get(group, index);
                case Handedness.RIGHT:
                    return rightNormalizedLandmarkProvider.Get(group, index);
            }

            throw new System.NotImplementedException();
        }
    }
}