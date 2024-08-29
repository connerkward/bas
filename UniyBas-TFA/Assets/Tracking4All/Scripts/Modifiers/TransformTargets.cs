using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Transform landmarks so they behave as if it was a child of the input transform parent.
    /// </summary>
    public class TransformTargets : Modifier, ILandmarkModifier
    {
        [SerializeField] private Transform parent;
        public float y;

        public void Modify(ref Landmark current, ref Landmark target, float deltaTime)
        {
            if (!Enabled) return;
            target.Position = parent.localToWorldMatrix.MultiplyPoint3x4(target.Position);
        }
    }
}