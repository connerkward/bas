// PoseAvatarMirrorBehavior
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class PoseAvatarMirrorBehavior : MonoBehaviour
    {
        public PuppetJointProvider<PoseJoints, PoseJoint> landmarkProvider;
        public PoseJoints target;
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
            moveToLandmark.transform.position = landmarkProvider.Get(group, (int)target).PuppetJointTransform.position;
        }
    }
}