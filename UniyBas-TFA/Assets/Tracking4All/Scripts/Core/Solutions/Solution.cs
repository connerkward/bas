using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Solutions provide data which is accessible by the Unity main-thread.
    /// </summary>
    public abstract class Solution : MonoBehaviour
    {
        public delegate void OnSolutionUpdate();

        /// <summary>
        /// Subscribe to this to get a call when the solution is completed updating.
        /// <para>Unity main thread safe.</para>
        /// </summary>
        public event OnSolutionUpdate OnSolutionUpdated;

        protected virtual void Update()
        {
            UpdateDatas();
            OnSolutionUpdated?.Invoke();
        }

        /// <summary>
        /// Update all data in the solution.
        /// <para>UpdateData() might be handy.</para>
        /// </summary>
        protected abstract void UpdateDatas();

        /// <summary>
        /// Helper function for solutions that updates one type of data that the solution provides.
        /// <para>NOTE: does not invoke any specific events, must be done manually.</para>
        /// </summary>
        /// <typeparam name="INDEXER"></typeparam>
        /// <typeparam name="DATA"></typeparam>
        /// <param name="group">The group this solution provides for.</param>
        /// <param name="thisProvider">This solution provider/current values.</param>
        /// <param name="targetProvider">The provider to target/try to reach.</param>
        /// <param name="modifier">The modifier(s) to apply.</param>
        /// <param name="into">Where to write the resultant values.</param>
        protected virtual void UpdateData<INDEXER, DATA>(
            int group,
            IProvider<INDEXER, DATA> thisProvider, IProvider<INDEXER, DATA> targetProvider,
            IModifier<DATA> modifier, Table<DATA> into)
            where INDEXER : System.Enum
            where DATA : new()
        {
            DATA current;
            DATA target;

            if(modifier!=null)
            modifier.PreUpdate(Time.deltaTime);

            for (int i = 0; i < thisProvider.DataCount; ++i)
            {
                current = thisProvider.Get(group, i);
                target = targetProvider.Get(group, i);

                if (modifier != null)
                {
                    modifier.Modify(ref current, ref target, Time.deltaTime);
                }
                else
                {
                    current = target;
                }

                into.Set(i, current);
            }
        }
    }
}