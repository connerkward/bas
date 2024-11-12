// HandSolutionExampleUse
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class HandSolutionExampleUse : MonoBehaviour
    {
        public LandmarkProvider<HandLandmarks> leftHandProvider;
        public LandmarkProvider<HandLandmarks> rightHandProvider;

        private void OnEnable()
        {
            // Subscribe to landmark provider updates.
            if (rightHandProvider.HasInterface)
            {
                // Called in sync with the hand data is being updated.
                rightHandProvider.OnLandmarksUpdated += RightHandProvider_OnLandmarksUpdated;
                // Called when the hand stops being detected.
                rightHandProvider.OnLandmarksStopped += RightHandProvider_OnLandmarksStopped;
            }

            if (leftHandProvider.HasInterface)
            {
                leftHandProvider.OnLandmarksUpdated += LeftHandProvider_OnLandmarksUpdated;
                leftHandProvider.OnLandmarksStopped += LeftHandProvider_OnLandmarksStopped;
            }
        }
        private void OnDisable()
        {
            // Unsubscribe from landmark provider updates when this gameobject is disabled.
            if (rightHandProvider.HasInterface)
            {
                rightHandProvider.OnLandmarksUpdated -= RightHandProvider_OnLandmarksUpdated;
                rightHandProvider.OnLandmarksStopped -= RightHandProvider_OnLandmarksStopped;
            }
            if (leftHandProvider.HasInterface)
            {
                leftHandProvider.OnLandmarksUpdated -= LeftHandProvider_OnLandmarksUpdated;
                leftHandProvider.OnLandmarksStopped -= LeftHandProvider_OnLandmarksStopped;
            }
        }

        private void LeftHandProvider_OnLandmarksUpdated(int group)
        {
            // Called in sync with the hand data is being updated.

            // Do whatever you want with the data
            Vector3 thumbBasePosition = leftHandProvider.Get(group, HandLandmarks.THUMB1).Position;

            print("Left-hand update");
        }
        private void LeftHandProvider_OnLandmarksStopped(int group)
        {
            // Called when the hand stops being detected.

            // Logic when detection stops (ex: show message "lost tracking").

            print("Left-hand stopped");
        }

        private void RightHandProvider_OnLandmarksUpdated(int group)
        {
            print("Right-hand update");
        }
        private void RightHandProvider_OnLandmarksStopped(int group)
        {
            print("Right-hand stopped");
        }
    }
}