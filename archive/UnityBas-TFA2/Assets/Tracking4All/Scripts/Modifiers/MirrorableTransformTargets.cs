// MirrorableTransformTargets
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class MirrorableTransformTargets : TransformTargets
    {
        [SerializeField] protected AdapterSettingsProvider adapterSettingsProvider;
        [SerializeField] protected Transform mirroredTransform;

        protected override Transform Parent
        {
            get
            {
                if (adapterSettingsProvider.Mirror) return mirroredTransform;
                return base.parent;
            }
        }
    }
}