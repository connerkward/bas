// MoveToPoseLandmark
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class MoveToPoseLandmarkExample : MonoBehaviour
    {
        public PoseLandmarks landmark;
        public LandmarkProvider<PoseLandmarks> poseProvider;
        public Transform move;

        private void Update()
        {
            move.position = poseProvider.Get(0, landmark).Position;
        }
    }
}