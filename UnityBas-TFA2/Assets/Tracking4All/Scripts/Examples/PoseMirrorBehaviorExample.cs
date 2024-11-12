// PoseMirrorBehaviorExample
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class PoseMirrorBehaviorExample : MonoBehaviour
    {
        public LandmarkProvider<PoseLandmarks> landmarkProvider;
        public PoseLandmarks target;
        public Transform moveToLandmark;
        public bool threadSafe = true;

        Vector3 t;

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
            if (threadSafe)
                moveToLandmark.transform.position = landmarkProvider.Get(group, (int)target).Position;
            else
                t = landmarkProvider.Get(group, (int)target).Position;
        }

        private void Update()
        {
            if (!threadSafe)
                moveToLandmark.transform.position = t;
        }
    }
}