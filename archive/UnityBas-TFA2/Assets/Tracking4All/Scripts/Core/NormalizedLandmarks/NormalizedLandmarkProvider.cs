namespace Tracking4All
{
    [System.Serializable]
    public class NormalizedLandmarkProvider<INDEXER> : InterfaceProvider<INormalizedLandmarkProvider<INDEXER>>,
        INormalizedLandmarkProvider<INDEXER>
        where INDEXER : System.Enum
    {
        public float TimeSinceLastUpdate => Provider.TimeSinceLastUpdate;
        public float LastUpdateTime => Provider.LastUpdateTime;
        public bool IsAlive => Tracking4All.Instance.IsProviderLive(TimeSinceLastUpdate);

        int IProvider<INDEXER, NormalizedLandmark>.DataCount => Provider.DataCount;

        public event IProvider<INDEXER, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksUpdated
        {
            add { Provider.OnNormalizedLandmarksUpdated += value; }

            remove { Provider.OnNormalizedLandmarksUpdated -= value; }
        }

        public event IProvider<INDEXER, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksStopped
        {
            add
            {
                Provider.OnNormalizedLandmarksStopped += value;
            }

            remove
            {
                Provider.OnNormalizedLandmarksStopped -= value;
            }
        }

        public NormalizedLandmark Get(int group, INDEXER index)
        {
            return Provider.Get(group, index);
        }

        public NormalizedLandmark Get(int group, int index)
        {
            return Provider.Get(group, index);
        }

        void IProvider<INDEXER, NormalizedLandmark>.DisposeProviderData(int group)
        {
            Provider.DisposeProviderData(group);
        }
    }
}