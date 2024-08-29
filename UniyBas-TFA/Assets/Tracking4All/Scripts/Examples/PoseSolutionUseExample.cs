using System.Collections;
using System.Collections.Generic;
using Tracking4All;
using UnityEngine;

public class PoseSolutionUseExample : MonoBehaviour
{
    public LandmarkProvider<PoseLandmarks> landmarkProvider;

    private void OnEnable()
    {
        // Subscribe to landmark provider updates.
        landmarkProvider.OnLandmarksUpdated += LandmarkProvider_OnLandmarksUpdated;
    }
    private void OnDisable()
    {
        // Unsubscribe from landmark provider updates when this gameobject is disabled.
        landmarkProvider.OnLandmarksUpdated -= LandmarkProvider_OnLandmarksUpdated;   
    }

    private void LandmarkProvider_OnLandmarksUpdated(int group)
    {
        // Get the landmark NOSE from the landmark provider.
        Landmark nose = landmarkProvider.Provider.Get(group, PoseLandmarks.NOSE);

        // Print out the position of the nose.
        print(nose.Position);
    }
}
