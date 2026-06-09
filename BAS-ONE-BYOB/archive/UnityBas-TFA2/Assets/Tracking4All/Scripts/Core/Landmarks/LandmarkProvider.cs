namespace Tracking4All
{
    [System.Serializable]
    public class LandmarkProvider<INDEXER> : InterfaceProvider<ILandmarkProvider<INDEXER>>, ILandmarkProvider<INDEXER>
        where INDEXER : System.Enum
    {
        public float TimeSinceLastUpdate => Provider.TimeSinceLastUpdate;
        public float LastUpdateTime => Provider.LastUpdateTime;
        public bool IsAlive => Tracking4All.Instance.IsProviderLive(TimeSinceLastUpdate);

        int IProvider<INDEXER, Landmark>.DataCount => Provider.DataCount;

        public event IProvider<INDEXER, Landmark>.GroupUpdated OnLandmarksUpdated
        {
            add { Provider.OnLandmarksUpdated += value; }

            remove { Provider.OnLandmarksUpdated -= value; }
        }

        public event IProvider<INDEXER, Landmark>.GroupUpdated OnLandmarksStopped
        {
            add
            {
                Provider.OnLandmarksStopped += value;
            }

            remove
            {
                Provider.OnLandmarksStopped -= value;
            }
        }

        public Landmark Get(int group, INDEXER index)
        {
            return Provider.Get(group, index);
        }

        public Landmark Get(int group, int index)
        {
            return Provider.Get(group, index);
        }

        void IProvider<INDEXER, Landmark>.DisposeProviderData(int group)
        {
            Provider.DisposeProviderData(group);
        }
    }
}