// CopyPoseHelper
// (C) 2024 G8gaming Ltd.
using System.Linq;
using UnityEngine;

namespace Tracking4All
{
    public class CopyPoseHelper : MonoBehaviour
    {
        public Transform source;
        public Transform destination;
        public bool copy = false;

        private void OnValidate()
        {
            if (copy)
            {
                Transform[] sourceTransforms = source.GetComponentsInChildren<Transform>(true);
                Transform[] destinationTransforms = destination.GetComponentsInChildren<Transform>(true);
                foreach (var sourceTransform in sourceTransforms)
                {
                    var matchingDestination = destinationTransforms.FirstOrDefault(dest => dest.name == sourceTransform.name);

                    if (matchingDestination != null)
                    {
                        matchingDestination.localPosition = sourceTransform.localPosition;
                        matchingDestination.localRotation = sourceTransform.localRotation;
                    }
                }

                print("Copied pose.");
            }

            copy = false;
        }
    }
}