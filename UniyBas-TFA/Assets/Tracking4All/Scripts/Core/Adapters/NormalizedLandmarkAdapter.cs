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

        public override void Update(int group, FROM_DATA from)
        {
            base.Update(group, from);

            OnNormalizedLandmarksUpdated?.Invoke(group);
        }
    }
}