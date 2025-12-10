using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Classic responsive smoothing. Set higher for more responsiveness but more jitter, lower for less responsiveness but smoother.
    /// </summary>
    public class SimpleSmooth : Modifier, ILandmarkModifier, INormalizedLandmarkModifier
    {
        public float speed = 10f; // lower is smoother, higher is more responsive but also more noisy
        public float occlusionMeasuresSpeed = 30f; // the speed that presense and visibility change at (same as above).

        public void Modify(int dataIndex, ref Landmark current, ref Landmark target, ref bool stayAlive, float deltaTime)
        {
            if (!Enabled) return;

            current.Position = Vector3.Lerp(current.Position, target.Position, deltaTime * speed);
            current.Presence = Mathf.Lerp(current.Presence, target.Presence, deltaTime * occlusionMeasuresSpeed);
            current.Visibility = Mathf.Lerp(current.Visibility, target.Visibility, deltaTime * occlusionMeasuresSpeed);

            /*if((current.Position - target.Position).sqrMagnitude > 0.01f)
            {
                stayAlive = true;
            }*/
        }

        public void Modify(int dataIndex, ref NormalizedLandmark current, ref NormalizedLandmark target, ref bool stayAlive, float deltaTime)
        {
            if (!Enabled) return;

            current.Position = Vector3.Lerp(current.Position, target.Position, deltaTime * speed);
            current.Presence = Mathf.Lerp(current.Presence, target.Presence, deltaTime * occlusionMeasuresSpeed);
            current.Visibility = Mathf.Lerp(current.Visibility, target.Visibility, deltaTime * occlusionMeasuresSpeed);

            /*if ((current.Position - target.Position).sqrMagnitude > 0.01f)
            {
                stayAlive = true;
            }*/
        }
    }
}