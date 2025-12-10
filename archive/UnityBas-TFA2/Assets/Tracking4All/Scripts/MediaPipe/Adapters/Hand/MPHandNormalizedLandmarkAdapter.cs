namespace Tracking4All
{
    public class MPHandNormalizedLandmarkAdapter : NormalizedLandmarkAdapter<
        IProvider<MPHandLandmarks, NormalizedLandmark>, HandLandmarks>
    {
        public MPHandNormalizedLandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
        {
        }

        protected override void Convert()
        {
            PassthroughConversion();
        }

        protected override NormalizedLandmark Get(int i)
        {
            return WorkingData.Get(WorkingGroup, i);
        }
    }
}