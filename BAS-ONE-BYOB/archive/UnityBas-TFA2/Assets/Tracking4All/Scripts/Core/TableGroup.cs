namespace Tracking4All
{
    /// <summary>
    /// Stores a group of tables where elements are accessible by group (int) and item index (int).
    /// </summary>
    /// <typeparam name="DATA_TYPE">The primitive datatype to store.</typeparam>
    public class TableGroup<DATA_TYPE>
        where DATA_TYPE : new()
    {
        private Table<DATA_TYPE>[] tables;

        /// <summary>
        /// The number of tables.
        /// </summary>
        public int GroupSize => tables.Length;

        public int ElementCount { get; protected set; }

        public TableGroup(int groupSize, int elementsCount)
        {
            ElementCount = elementsCount;

            tables = new Table<DATA_TYPE>[groupSize];

            for (int i = 0; i < groupSize; ++i)
            {
                tables[i] = new Table<DATA_TYPE>(elementsCount);
            }
        }

        public void Set(int group, int index, DATA_TYPE l)
        {
            tables[group].Set(index, l);
        }

        public DATA_TYPE Get(int group, int index)
        {
            return tables[group].Get(index);
        }
    }
}