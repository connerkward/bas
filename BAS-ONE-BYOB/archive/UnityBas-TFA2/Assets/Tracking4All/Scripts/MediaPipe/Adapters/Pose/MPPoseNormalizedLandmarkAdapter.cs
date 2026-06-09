namespace Tracking4All
{
    public class MPPoseNormalizedLandmarkAdapter : NormalizedLandmarkAdapter<
        IProvider<MPPoseLandmarks, NormalizedLandmark>, PoseLandmarks>
    {
        public MPPoseNormalizedLandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
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

        private NormalizedLandmark Get(MPPoseLandmarks l)
        {
            return WorkingData.Get(WorkingGroup, l);
        }
    }
}