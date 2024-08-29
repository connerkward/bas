using UnityEngine;

namespace Tracking4All
{
    public class LandmarkVisual : MonoBehaviour
    {
        public void UpdateLandmark(Landmark landmark)
        {
            transform.localPosition = landmark.Position;
        }
    }
}