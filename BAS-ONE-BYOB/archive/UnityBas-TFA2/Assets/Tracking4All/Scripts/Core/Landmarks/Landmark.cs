using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Landmark datatype used throughout the system.
    /// </summary>
    public struct Landmark : IDisposableData
    {
        /// <summary>
        /// The position of the landmark.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// 0-1 probability for whether the landmark is visible/occluded by other objects.
        /// <para>Use this to generally check 'how reliable' a landmark is.</para>
        /// <para>1=more visible, 0 = less visible.</para>
        /// </summary>
        public float Visibility;

        /// <summary>
        /// 0-1 probability for whether the landmark is present on the scene (within scene bounds).
        /// <para>Use this to check if the landmark is out of the camera's view or not.</para>
        /// <para>1=more present, 0 = less present.</para>
        /// </summary>
        public float Presence;

        public Landmark(Vector3 position, float visibility, float presence)
        {
            this.Position = position;
            this.Visibility = visibility;
            this.Presence = presence;
        }

        public void Dispose()
        {
            Visibility = 0;
            Presence = 0;
        }

        public Landmark Disposed()
        {
            Dispose();
            return this;
        }
    }
}