// Transformer
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public abstract class Transformer : Modifier, ILandmarkModifier
    {
        protected abstract Matrix4x4 LocalToWorldMatrix { get; }

        public void Modify(int dataIndex, ref Landmark current, ref Landmark target, ref bool stayAlive, float deltaTime)
        {
            if (!Enabled) return;
            //current.Position = LocalToWorldMatrix.MultiplyPoint3x4(current.Position);
            target.Position = LocalToWorldMatrix.MultiplyPoint3x4(target.Position);
        }
    }
}