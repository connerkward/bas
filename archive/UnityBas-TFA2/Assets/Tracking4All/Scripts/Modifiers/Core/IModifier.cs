namespace Tracking4All
{
    public interface IModifier<DATA_TYPE>
    {
        // Pointer: think about Modify as basically running in a for loop on the data,
        // precalculate is before, postcalculate is after.

        /// <summary>
        /// If true, the modifier should be enabled.
        /// </summary>
        public bool Enabled { get; }

        /// <summary>
        /// Update internal state before calling indivisual modifies.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="dataCount"></param>
        public void PreCalculate(float deltaTime, int dataCount);

        /// <summary>
        /// Modify each indivisual data.
        /// <para>NOTE: each component of current must be fully set if being modified.</para>
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <param name="deltaTime"></param>
        public void Modify(int dataIndex, ref DATA_TYPE current, ref DATA_TYPE target, ref bool stayAlive, float deltaTime);

        /// <summary>
        /// Update internal state after calling indivisual modifes.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void PostCalculate(float deltaTime);
    }
}