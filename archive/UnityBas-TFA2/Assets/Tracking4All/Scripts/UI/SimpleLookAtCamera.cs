// SimpleLookAtCamera
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class SimpleLookAtCamera : MonoBehaviour
    {
        private Transform at;

        private void OnEnable()
        {
            at = Camera.main.transform;
        }

        private void LateUpdate()
        {
            transform.LookAt(at);
        }
    }
}