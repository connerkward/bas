using Tracking4All;
using UnityEngine;
using Landmark = Tracking4All.Landmark;

public class MPUHandLandmarkAdapter : LandmarkAdapter<Mediapipe.LandmarkList, MPHandLandmarks>
{
    // NOTE: adapter is PER HAND.

    private bool flipX; // flip hand landmarks again (holistic needs this)

    public MPUHandLandmarkAdapter(IAdapterSettings settings, int groupSize, bool flipX = false) : base(settings, groupSize)
    {
        this.flipX = flipX;
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

        if (this.flipX) landmark.X *= -1;

        return new Landmark(
                new Vector3(landmark.HasX ? (adapterSettings.Mirror ? -1 : 1) * (adapterSettings.PerspectiveFlip ? -1 : 1) * landmark.X : 0,
                    landmark.HasY ? -landmark.Y : 0,
                    landmark.HasZ ? (adapterSettings.PerspectiveFlip ? 1 : -1) * landmark.Z : 0),
                landmark.Visibility,
                landmark.Presence
            );
    }
}