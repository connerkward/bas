// LivingMPHolisticHandAdapter
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class LivingMPHolisticHandAdapter : LivingMPHandAdapter
    {
        // Customized hand landmark adapter for holistic.
        // More strict about hand structure and more willing to drop frames that are anomalies...

        [SerializeField] private bool filterEnabled = true;

        private const float PalmFlipToleranceStartValue = 50f;

        private Vector3 lastUp;
        private float palmFlipTolerance = PalmFlipToleranceStartValue;

        protected override bool IsLandmarkUpdatePassed(int group)
        {
            if (!filterEnabled) return true;

            MPHandLandmarks r0 = MPHandLandmarks.RING1;
            MPHandLandmarks i0 = MPHandLandmarks.INDEX1;
            MPHandLandmarks m0 = MPHandLandmarks.MIDDLE1;
            MPHandLandmarks wrist = MPHandLandmarks.WRIST;

            Vector3 handForwardCentre = Helpers.Average(Get(group, m0), Get(group, r0), Get(group, i0));
            Vector3 handForward = (handForwardCentre - Get(group, wrist));
            Vector3 handUp = Helpers.CalculateNormal(Get(group, wrist), Get(group, i0), Get(group, r0));
            Vector3 handRight = Vector3.Cross(handForward, handUp);
            if (group == (int)Handedness.LEFT) handUp *= -1;

            Vector3 dirp = (Get(group, MPHandLandmarks.PINKY2) - Get(group, wrist)).normalized;
            Vector3 dirr = (Get(group, MPHandLandmarks.RING2) - Get(group, wrist)).normalized;

            Vector3 dirpb = (Get(group, MPHandLandmarks.PINKY3) - Get(group, MPHandLandmarks.PINKY1)).normalized;
            Vector3 dirib = (Get(group, MPHandLandmarks.INDEX3) - Get(group, MPHandLandmarks.INDEX1)).normalized;
            Vector3 dirmb = (Get(group, MPHandLandmarks.MIDDLE3) - Get(group, MPHandLandmarks.MIDDLE1)).normalized;

            if (Vector3.Dot(dirp, handUp) > 0.01
                || Vector3.Dot(dirr, handUp) > 0.01) return false; // skip if finger is in stupid place

            float deviation = Mathf.Acos(Vector3.Dot(lastUp, handUp)) * Mathf.Rad2Deg;
            lastUp = handUp;

            if (deviation > palmFlipTolerance) // resist palm flips
            {
                palmFlipTolerance += 10f;
                return false;
            }

            palmFlipTolerance = PalmFlipToleranceStartValue;

            return true;
        }

        protected Vector3 Get(int group, MPHandLandmarks l)
        {
            return landmarkProvider.Get(group, l).Position;
        }

    }
}