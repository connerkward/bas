namespace Tracking4All
{
    public class MPHandLandmarkAdapter : LandmarkAdapter<IProvider<MPHandLandmarks, Landmark>, HandLandmarks>
    {
        public MPHandLandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
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
    }
}