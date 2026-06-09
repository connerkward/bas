using Tracking4All;
using UnityEngine;

public class PoseSolutionUseExample : MonoBehaviour
{
    public LandmarkProvider<PoseLandmarks> landmarkProvider;
    public Vector3 nosePosition;
    public float nosePresence;

    private void OnEnable()
    {
        // Subscribe to landmark provider updates.
        if (landmarkProvider.HasInterface)
        {
            landmarkProvider.OnLandmarksUpdated += LandmarkProvider_OnLandmarksUpdated;
            landmarkProvider.OnLandmarksStopped += LandmarkProvider_OnLandmarksStopped;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from landmark provider updates when this gameobject is disabled.
        if (landmarkProvider.HasInterface)
        {
            landmarkProvider.OnLandmarksUpdated -= LandmarkProvider_OnLandmarksUpdated;
            landmarkProvider.OnLandmarksStopped -= LandmarkProvider_OnLandmarksStopped;
        }
    }

    private void LandmarkProvider_OnLandmarksUpdated(int group)
    {
        // Read from the landmark provider, it has just been updated!

        // Get the landmark NOSE from the landmark provider.
        Landmark nose = landmarkProvider.Provider.Get(group, PoseLandmarks.NOSE);

        // Do anything with the data
        nosePosition = nose.Position;
        nosePresence = nose.Presence;

        if (landmarkProvider.IsAlive == false)
        {
            print("Landmark provide or isn't alive anymore!");
        }

        if (nose.Presence == 0)
        {
            print("The nose is no longer present!");
        }

        // Print out the position of the nose.
        // print(nose.Position);
    }

    private void LandmarkProvider_OnLandmarksStopped(int group)
    {
        // Called when the landmark provider stops providing this data (ex: lost detection).
        // NOTE: you could also check landmarkProvider.IsAlive instead.

        print("Lost landmarks");
    }
}
