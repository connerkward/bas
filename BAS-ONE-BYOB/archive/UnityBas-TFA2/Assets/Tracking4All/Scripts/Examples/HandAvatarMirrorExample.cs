// HandAvatarMirrorExample
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class HandAvatarMirrorExample : MonoBehaviour
    {
        public PuppetJointProvider<HandJoints, HandJoint> landmarkProvider;
        public Handedness handedness;
        public HandJoints target;
        public Transform moveToLandmark;

        private void OnEnable()
        {
            landmarkProvider.OnJointsUpdated += LandmarkProvider_OnLandmarksUpdated;
        }
        private void OnDisable()
        {
            landmarkProvider.OnJointsUpdated -= LandmarkProvider_OnLandmarksUpdated;
        }

        private void LandmarkProvider_OnLandmarksUpdated(int group)
        {
            moveToLandmark.transform.position = landmarkProvider.Get((int)handedness, (int)target).PuppetJointTransform.position;
        }
    }
}