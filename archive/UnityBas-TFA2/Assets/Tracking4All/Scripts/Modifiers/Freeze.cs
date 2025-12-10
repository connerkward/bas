// Freeze
// (C) 2024 G8gaming Ltd.
namespace Tracking4All
{
    public class Freeze : Modifier,
        ILandmarkModifier
    {
        public void Modify(int dataIndex, ref Landmark current, ref Landmark target, ref bool stayAlive, float deltaTime)
        {
            if (!Enabled) return;

            target.Position = current.Position;
            target.Presence = current.Presence;
            target.Visibility = current.Visibility;
        }
    }
}