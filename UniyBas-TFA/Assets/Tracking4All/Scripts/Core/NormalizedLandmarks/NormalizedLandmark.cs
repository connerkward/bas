using UnityEngine;

namespace Tracking4All
{
    public struct NormalizedLandmark
    {
        /// <summary>
        /// The position of the landmark.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Probability of the landmark not being occluded.
        /// </summary>
        public float Visibility;

        /// <summary>
        /// Probability of the landmark being within frame.
        /// </summary>
        public float Presence;

        public NormalizedLandmark(Vector3 position, float visibility, float presence)
        {
            this.Position = position;
            this.Visibility = visibility;
            this.Presence = presence;
        }
    }
}