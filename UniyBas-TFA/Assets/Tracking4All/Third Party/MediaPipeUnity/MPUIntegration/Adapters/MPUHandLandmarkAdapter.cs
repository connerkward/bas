using Mediapipe;
using System.Collections;
using System.Collections.Generic;
using Tracking4All;
using UnityEngine;
using Landmark = Tracking4All.Landmark;

public class MPUHandLandmarkAdapter : LandmarkAdapter<Mediapipe.LandmarkList, MPHandLandmarks>
{
    public MPUHandLandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
    {
    }

    protected override void Convert()
    {
        for (int i = 0; i < DataCount; ++i)
        {
            Set(i, Get(i));
        }
    }

    Mediapipe.Landmark landmark;
    protected override Landmark Get(int i)
    {
        landmark = WorkingData.Landmark[i];

        if (adapterSettings.IsBackFacingCamera)
        {
            return new Landmark(
                new Vector3(landmark.HasX ? landmark.X: 0, landmark.HasY ? -landmark.Y : 0, landmark.HasZ ? landmark.Z : 0),
                landmark.Visibility,
                landmark.Presence
            );
        }
        else
        {
            return new Landmark(
                new Vector3(landmark.HasX ? landmark.X: 0, landmark.HasY ? -landmark.Y : 0, landmark.HasZ ? -landmark.Z : 0),
                landmark.Visibility,
                landmark.Presence
            );
        }
    }
}
