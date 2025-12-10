namespace Tracking4All
{
    /// <summary>
    /// Convert any MP based pose landmark provider to a standard landmark pose provider.
    /// </summary>
    public class MPPoseLandmarkAdapter : LandmarkAdapter<IProvider<MPPoseLandmarks, Landmark>, PoseLandmarks>
    {
        public MPPoseLandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
        {
        }

        protected override void Convert()
        {
            PassthroughConversion();
        }

        protected override Landmark Get(int i)
        {
            return WorkingData.Get(WorkingGroup, i);
        }

        private Landmark Get(MPPoseLandmarks l)
        {
            return WorkingData.Get(WorkingGroup, l);
        }
    }
}