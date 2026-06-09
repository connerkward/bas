using Tracking4All;
using UnityEngine;
using NormalizedLandmark = Tracking4All.NormalizedLandmark;

public class MPUHandNormalizedLandmarkAdapter : NormalizedLandmarkAdapter<Mediapipe.NormalizedLandmarkList, MPHandLandmarks>
{
    public MPUHandNormalizedLandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
    {
    }

    protected override void Convert()
    {
        for (int i = 0; i < DataCount; ++i)
        {
            Set(i, Get(i));
        }
    }

    Mediapipe.NormalizedLandmark landmark;
    protected override NormalizedLandmark Get(int i)
    {
        landmark = WorkingData.Landmark[i];

        return new NormalizedLandmark(
            new Vector3(landmark.HasX ? 1f - landmark.X : 0, landmark.HasY ? 1f - landmark.Y : 0, landmark.HasZ ? landmark.Z : 0),
            landmark.Visibility,
            landmark.Presence
        );

        // Depreciated behavior: normalized landmarks are in screen coords always.
        /*if (adapterSettings.CameraView)
        {
            return new NormalizedLandmark(
                new Vector3(landmark.HasX ? -landmark.X  : 0, landmark.HasY ? -landmark.Y  : 0, landmark.HasZ ? landmark.Z  : 0),
                landmark.Visibility,
                landmark.Presence
            );
        }
        else
        {
            return new NormalizedLandmark(
                new Vector3(landmark.HasX ? landmark.X : 0, landmark.HasY ? -landmark.Y  : 0, landmark.HasZ ? -landmark.Z: 0),
                landmark.Visibility,
                landmark.Presence
            );
        }*/
    }
}
