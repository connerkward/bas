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
        /// <summary>
        /// The total number of data points provided.
        /// </summary>
        public abstract int DataCount { get; }
        /// <summary>
        /// The time the last update occurred.
        /// </summary>
        public abstract float LastUpdateTime { get; }
        /// <summary>
        /// The delta time from the last update.
        /// </summary>
        public float TimeSinceLastUpdate => Helpers.GetTime() - LastUpdateTime;
        /// <summary>
        /// True if this provider is considered alive.
        /// </summary>
        public bool IsAlive => Tracking4All.Instance.IsProviderLive(TimeSinceLastUpdate);
        /// <summary>
        /// Return whether or not the provider would be alive if the update time was as inputed.
        /// <para>Useful where we are managing the update time manually.</para>
        /// </summary>
        /// <param name="lastUpdateTime"></param>
        /// <returns></returns>
        public bool IsAliveWith(float lastUpdateTime)
        {
            return Tracking4All.Instance.IsProviderLive(Helpers.GetTime() - lastUpdateTime);
        }

        public delegate void GroupUpdated(int group);

        public abstract void DisposeProviderData(int group);//

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