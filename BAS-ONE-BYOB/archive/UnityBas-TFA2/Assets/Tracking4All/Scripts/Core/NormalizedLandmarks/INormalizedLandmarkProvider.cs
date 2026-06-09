namespace Tracking4All
{
    public interface INormalizedLandmarkProvider<INDEXER> : IProvider<INDEXER, NormalizedLandmark>
        where INDEXER : System.Enum
    {
        public event GroupUpdated OnNormalizedLandmarksUpdated;
        public event GroupUpdated OnNormalizedLandmarksStopped;
    }
}