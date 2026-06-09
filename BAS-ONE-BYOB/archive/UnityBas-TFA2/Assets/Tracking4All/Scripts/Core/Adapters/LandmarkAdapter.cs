namespace Tracking4All
{
    /// <summary>
    /// Landmark Adapter which converts FROM_DATA to a collection of landmarks identified by TO.
    /// </summary>
    /// <typeparam name="FROM_DATA"></typeparam>
    /// <typeparam name="TO"></typeparam>
    public abstract class LandmarkAdapter<FROM_DATA, TO> : Adapter<FROM_DATA, TO, Landmark>, ILandmarkProvider<TO>
        where TO : System.Enum
    {
        protected LandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
        {
        }

        public event IProvider<TO, Landmark>.GroupUpdated OnLandmarksUpdated;
        public event IProvider<TO, Landmark>.GroupUpdated OnLandmarksStopped;

        protected override void InvokeUpdateMessage(int group)
        {
            OnLandmarksUpdated?.Invoke(group);
        }
        protected override void InvokeStoppedMessage(int group)
        {
            OnLandmarksStopped?.Invoke(group);
        }

        public override void Update(int group, FROM_DATA from)
        {
            base.Update(group, from);

            InvokeUpdateMessage(group);
        }
    }
}