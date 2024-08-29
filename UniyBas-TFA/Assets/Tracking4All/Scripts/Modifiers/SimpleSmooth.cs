using UnityEngine;

namespace Tracking4All
{
    public class SimpleSmooth : Modifier, ILandmarkModifier, INormalizedLandmarkModifier
    {
        public float speed = 10f;

        public void Modify(ref Landmark current, ref Landmark target, float deltaTime)
        {
            if (!Enabled) return;

            current.Position = Vector3.Lerp(current.Position, target.Position, deltaTime * speed);
        }

        public void Modify(ref NormalizedLandmark current, ref NormalizedLandmark target, float deltaTime)
        {
            if (!Enabled) return;

            current.Position = Vector3.Lerp(current.Position, target.Position, deltaTime * speed);
        }
    }
}