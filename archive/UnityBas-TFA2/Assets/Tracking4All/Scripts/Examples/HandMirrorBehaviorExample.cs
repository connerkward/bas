// HandMirrorBehaviorExample
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class HandMirrorBehaviorExample : MonoBehaviour
    {
        public LandmarkProvider<HandLandmarks> landmarkProvider;
        public Handedness handedness;
        public HandLandmarks target;
        public Transform moveToLandmark;

        private void OnEnable()
        {
            landmarkProvider.OnLandmarksUpdated += LandmarkProvider_OnLandmarksUpdated;
        }
        private void OnDisable()
        {
            landmarkProvider.OnLandmarksUpdated -= LandmarkProvider_OnLandmarksUpdated;
        }

        private void LandmarkProvider_OnLandmarksUpdated(int group)
        {
            Vector3 average = (Get(target)) / 1f;
            moveToLandmark.transform.position = average;
        }

        private Vector3 Get(HandLandmarks handLandmarks)
        {
            return landmarkProvider.Get((int)handedness, handLandmarks).Position;
        }
    }
}