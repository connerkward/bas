using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Transform landmarks so they behave as if it was a child of the input transform parent.
    /// </summary>
    public class TransformTargets : Transformer
    {
        [SerializeField] protected Transform parent;

        protected virtual Transform Parent => parent;

        protected override Matrix4x4 LocalToWorldMatrix => Parent.localToWorldMatrix;
    }
}