// PresenseCutoff
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// An experimental example, if a target is below a threshold presence, don't update.
    /// Not really useful (i.e. example).
    /// </summary>
    public class PresenseCutoff : Modifier, ILandmarkModifier
    {
        [SerializeField] private float cutoff = .5f;
        public void Modify(int dataIndex, ref Landmark current, ref Landmark target, ref bool stayAlive, float deltaTime)
        {
            if (!Enabled) return;

            if (target.Presence < cutoff)
            {
                target.Position = current.Position;
            }
        }
    }
}