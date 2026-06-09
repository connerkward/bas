// SequentialTable
// (C) 2024 G8gaming Ltd.
namespace Tracking4All
{
    /// <summary>
    /// A table that stores the previous and current values.
    /// <para>I.e. when set, you can fetch the previous value still.</para>
    /// </summary>
    public class SequentialTable<DATA_TYPE>
        where DATA_TYPE : new()
    {
        private DATA_TYPE[] old;
        private DATA_TYPE[] current;

        /// <summary>
        /// The number of elements in the landmark table.
        /// </summary>
        public int Count => old.Length;

        public SequentialTable(int count)
        {
            old = new DATA_TYPE[count];
            current = new DATA_TYPE[count];

            for (int i = 0; i < count; ++i)
            {
                old[i] = new DATA_TYPE();
                current[i] = new DATA_TYPE();
            }
        }

        public void Set(int landmark, DATA_TYPE l)
        {
            old[landmark] = current[landmark];
            current[landmark] = l;
        }

        /// <summary>
        /// Get the current most up to date value.
        /// </summary>
        /// <param name="landmark"></param>
        /// <returns></returns>
        public DATA_TYPE Get(int landmark)
        {
            return current[landmark];
        }
        /// <summary>
        /// Get the previous value.
        /// </summary>
        /// <param name="landmark"></param>
        /// <returns></returns>
        public DATA_TYPE GetLast(int landmark)
        {
            return old[landmark];
        }
    }
}