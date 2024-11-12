namespace Tracking4All
{
    /// <summary>
    /// Provides a list of DATA_TYPE updated by whatever is providing the data (ex: MPU, python, etc)
    /// </summary>
    public class Table<DATA_TYPE>
        where DATA_TYPE : new()
    {
        private DATA_TYPE[] table;

        /// <summary>
        /// The number of elements in the landmark table.
        /// </summary>
        public int Count => table.Length;

        public Table(int count)
        {
            table = new DATA_TYPE[count];

            for (int i = 0; i < count; ++i)
            {
                table[i] = new DATA_TYPE();
            }
        }

        public void Set(int landmark, DATA_TYPE l)
        {
            table[landmark] = l;
        }

        public DATA_TYPE Get(int landmark)
        {
            return table[landmark];
        }
    }
}