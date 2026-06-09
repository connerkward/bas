namespace Tracking4All
{
    public abstract class NormalizedLandmarkAdapter<FROM_DATA, TO> : Adapter<FROM_DATA, TO, NormalizedLandmark>,
        INormalizedLandmarkProvider<TO>
        where TO : System.Enum
    {
        protected NormalizedLandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
        {
        }

        public event IProvider<TO, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksUpdated;
        public event IProvider<TO, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksStopped;

        protected override void InvokeUpdateMessage(int group)
        {
            OnNormalizedLandmarksUpdated?.Invoke(group);
        }
        protected override void InvokeStoppedMessage(int group)
        {
            OnNormalizedLandmarksStopped?.Invoke(group);
        }

        public override void Update(int group, FROM_DATA from)
        {
            base.Update(group, from);

            InvokeUpdateMessage(group);
        }


    }
}