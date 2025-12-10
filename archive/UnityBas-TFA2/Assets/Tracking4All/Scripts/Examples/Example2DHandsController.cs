// Example2DHandsController
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Basic approach to move hands using normalized landmarks.
    /// </summary>
    public class Example2DHandsController : MonoBehaviour
    {
        [SerializeField] private NormalizedLandmarkProvider<HandLandmarks> normalizedLandmarkProvider;
        [SerializeField] private float multiplier = 1f;
        [SerializeField] private float scalerX = -2.5f;
        [SerializeField] private float scalerY = 1f;

        [SerializeField] private Transform rightHandParent;
        [SerializeField] private Transform leftHandParent;

        private Vector3 rightOrigin, leftOrigin;
        private void Start()
        {
            rightOrigin = rightHandParent.position;
            leftOrigin = rightHandParent.position;
        }

        private void Update()
        {
            Displace(rightHandParent, rightOrigin, Handedness.RIGHT);
            Displace(leftHandParent, leftOrigin, Handedness.LEFT);
        }
        private void Displace(Transform handParent, Vector3 origin, Handedness handedness)
        {
            Vector3 centre = normalizedLandmarkProvider.Get((int)handedness, HandLandmarks.WRIST).Position;
            handParent.position = origin + ((centre.x - .5f) * scalerX * Vector3.right + (centre.y - .5f) * scalerY * Vector3.up) * multiplier;

        }
    }
}