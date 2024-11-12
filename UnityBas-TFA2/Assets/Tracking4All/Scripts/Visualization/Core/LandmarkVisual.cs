using UnityEngine;

namespace Tracking4All
{
    public class LandmarkVisual : MonoBehaviour
    {
        [SerializeField] protected new MeshRenderer renderer;

        public void UpdateLandmark(Landmark landmark, LandmarkVisualizationMode visualizationMode)
        {
            transform.localPosition = landmark.Position;
        }
    }
}