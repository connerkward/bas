// Example2DAvatarController
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Basic approach to use normalized landmarks to move the avatar left and right.
    /// <para>Not designed for moving cameras.</para>
    /// </summary>
    public class Example2DAvatarController : MonoBehaviour
    {
        [SerializeField] private NormalizedLandmarkProvider<PoseLandmarks> normalizedLandmarkProvider;
        [SerializeField] private float multiplier = 1f;
        [SerializeField] private float scalerX = -2.5f;
        [SerializeField] private float scalerY = 1f;

        private Vector3 origin;
        private void Start()
        {
            origin = transform.position;
        }

        private void Update()
        {
            Vector3 centre = Helpers.Average(
                normalizedLandmarkProvider.Get(0, PoseLandmarks.LEFT_HIP),
                normalizedLandmarkProvider.Get(0, PoseLandmarks.RIGHT_HIP),
                normalizedLandmarkProvider.Get(0, PoseLandmarks.RIGHT_SHOULDER),
                normalizedLandmarkProvider.Get(0, PoseLandmarks.LEFT_SHOULDER)).Position;

            transform.position = origin + ((centre.x - .5f) * scalerX * Vector3.right + (centre.y - .5f) * scalerY * Vector3.up) * multiplier;
        }
    }
}