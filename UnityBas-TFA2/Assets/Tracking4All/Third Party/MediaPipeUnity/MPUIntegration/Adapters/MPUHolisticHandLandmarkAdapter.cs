// MPUHolisticHandLandmarkAdapter
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class MPUHolisticHandLandmarkAdapter : LandmarkAdapter<Mediapipe.LandmarkList, MPHandLandmarks>
    {
        // NOTE: adapter is PER HAND.

        public MPUHolisticHandLandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
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
            // holistic version

            landmark = WorkingData.Landmark[i];
            landmark.X *= -1;

            return new Landmark(
                    new Vector3(landmark.HasX ? (adapterSettings.Mirror ? -1 : 1) * (adapterSettings.PerspectiveFlip ? -1 : 1) * landmark.X : 0,
                        landmark.HasY ? -landmark.Y : 0,
                        landmark.HasZ ? (adapterSettings.PerspectiveFlip ? 1 : -1) * landmark.Z : 0),
                    landmark.Visibility,
                    landmark.Presence
                );
        }
    }
}