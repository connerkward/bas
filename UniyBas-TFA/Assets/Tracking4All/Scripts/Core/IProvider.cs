namespace Tracking4All
{
    /// <summary>
    /// Provides access to DATA_TYPE stored with indexing by INDEXER
    /// </summary>
    /// <typeparam name="INDEXER">Enum used to index the data.</typeparam>
    /// <typeparam name="DATA_TYPE">The data to provide.</typeparam>
    public interface IProvider<INDEXER, DATA_TYPE>
        where INDEXER : System.Enum
        where DATA_TYPE : new()
    {
        public abstract int DataCount { get; }
        public abstract float TimeSinceLastUpdate { get; }
        /// <summary>
        /// True if this provider is considered alive.
        /// </summary>
        public bool IsAlive => Tracking4All.Instance.IsProviderLive(TimeSinceLastUpdate);

        public delegate void GroupUpdated(int group);

        /// <summary>
        /// Get data in group by index.
        /// </summary>
        /// <param name="group">The group the data is stored in default=0.</param>
        /// <param name="index">Index of the data to get.</param>
        /// <returns></returns>
        public DATA_TYPE Get(int group, INDEXER index);

        public DATA_TYPE Get(int group, int index);
    }
}