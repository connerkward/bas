namespace Tracking4All
{
    public interface ILandmarkProvider<INDEXER> : IProvider<INDEXER, Landmark>
        where INDEXER : System.Enum
    {
        public event GroupUpdated OnLandmarksUpdated;
        public event GroupUpdated OnLandmarksStopped;
    }
}