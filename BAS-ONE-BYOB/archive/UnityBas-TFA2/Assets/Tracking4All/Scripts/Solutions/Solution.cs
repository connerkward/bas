using UnityEngine;

namespace Tracking4All
{
    [DefaultExecutionOrder(-1)]
    /// <summary>
    /// Solutions provide data providers for a single group accessible by the Unity main-thread.
    /// </summary>
    public abstract class Solution : MonoBehaviour, IAdapterSettings
    {
        [SerializeField] protected AdapterSettingsProvider adapterSettings;

        /// <summary>
        /// The group int this solution represents (for protected use).
        /// </summary>
        protected abstract int GroupInt { get; }
        /// <summary>
        /// The number of data points this solution can output.
        /// </summary>
        protected abstract int DataCount { get; }

        public bool PerspectiveFlip => ((IAdapterSettings)adapterSettings).PerspectiveFlip;
        public bool Mirror => ((IAdapterSettings)adapterSettings).Mirror;

        protected virtual void OnEnable()
        {
            RegisterCallbacks();
        }
        /// <summary>
        /// Register callbacks, subsequent hooks may not be unity main thread safe.
        /// </summary>
        protected abstract void RegisterCallbacks();
        protected virtual void OnDisable()
        {
            DeregisterCallbacks();
        }
        protected abstract void DeregisterCallbacks();

        protected virtual void Update()
        {
            UpdateDatas();
            // OnSolutionUpdated?.Invoke();
        }

        /// <summary>
        /// Update all data in the solution on the unity main thread.
        /// <para>Runs with UnityEngine.Update.</para>
        /// <para>UpdateData() might be handy.</para>
        /// </summary>
        protected abstract void UpdateDatas();

        /// <summary>
        /// Helper function for solutions that updates one type of data that the solution provides.
        /// </summary>
        /// <typeparam name="INDEXER"></typeparam>
        /// <typeparam name="DATA"></typeparam>
        /// <param name="group">The group this solution provides for.</param>
        /// <param name="thisProvider">This solution provider/current values.</param>
        /// <param name="updateFromProvider">The provider to target/try to reach.</param>
        /// <param name="modifier">The modifier(s) to apply.</param>
        /// <param name="into">Where to write the resultant values.</param>
        protected virtual void UpdateData<INDEXER, DATA>(
            int group, int dataCount,
            IProvider<INDEXER, DATA> thisProvider, IProvider<INDEXER, DATA> updateFromProvider,
            IModifier<DATA> modifier, Table<DATA> into, IProvider<INDEXER, DATA>.GroupUpdated updated,
            out bool stayAlive)
            where INDEXER : System.Enum
            where DATA : new()
        {
            DATA current;
            DATA target;
            stayAlive = false;

            if (modifier != null)
                modifier.PreCalculate(Time.deltaTime, dataCount);

            for (int i = 0; i < thisProvider.DataCount; ++i)
            {
                current = thisProvider.Get(group, i);
                target = updateFromProvider.Get(group, i);

                if (modifier != null)
                {
                    modifier.Modify(i, ref current, ref target, ref stayAlive, Time.deltaTime);
                }
                else
                {
                    current = target;
                }

                into.Set(i, current);
            }

            if (modifier != null)
                modifier.PostCalculate(Time.deltaTime);

            updated?.Invoke(group);
        }

        /// <summary>
        /// Helper function to dispose data correctly.
        /// </summary>
        /// <typeparam name="INDEXER"></typeparam>
        /// <typeparam name="DATA"></typeparam>
        /// <param name="group"></param>
        /// <param name="provider">The provider to dispose</param>
        /// <param name="into">The associated table being updated</param>
        protected void DisposeData<INDEXER, DATA>(int group, IProvider<INDEXER, DATA> provider, Table<DATA> into,
            IProvider<INDEXER, DATA>.GroupUpdated updated, IProvider<INDEXER, DATA>.GroupUpdated stopped)
            where INDEXER : System.Enum
            where DATA : IDisposableData, new()
        {
            // Immediately cleanly dispose, back propagate the instruction.
            // OnUpdated has slightly customized behaviour: disposing will still invoke update for intuitiveness.

            DATA current;
            for (int i = 0; i < into.Count; ++i)
            {
                current = into.Get(i);
                current.Dispose();

                into.Set(i, current);
            }

            // provider.DisposeProviderData(group);

            //updated?.Invoke(group);
            stopped?.Invoke(group);
        }

        // DEPRECIATED: subscribe via providers instead.
        // public delegate void OnSolutionUpdate();
        // public event OnSolutionUpdate OnSolutionUpdated;


        /// <summary>
        /// A data component that can be updated within a Solution on Update.
        /// <para>Reduces redundancy.</para>
        /// </summary>
        /// <typeparam name="INDEXER"></typeparam>
        /// <typeparam name="DATA_TYPE"></typeparam>
        public class SolutionDataComponent<INDEXER, DATA_TYPE>
            where INDEXER : System.Enum
            where DATA_TYPE : IDisposableData, new()
        {
            protected bool live;
            protected Table<DATA_TYPE> data;
            protected float lastUpdateTime = Mathf.NegativeInfinity;

            protected bool modifiersWantStayLive;

            public bool Live => live;
            public float LastLandmarkUpdateTime => lastUpdateTime;
            public int Count => data.Count;

            public void SetLastUpdateTime(float lastUpdateTime)
            {
                this.lastUpdateTime = lastUpdateTime;
            }
            public DATA_TYPE Get(int index)
            {
                return data.Get(index);
            }

            public SolutionDataComponent()
            {
                data = new Table<DATA_TYPE>(Helpers.GetLength(typeof(INDEXER)));
            }

            public virtual void UpdateSolution(Solution solution,
                int group, int dataCount, IProvider<INDEXER, DATA_TYPE> thisProvider,
                IProvider<INDEXER, DATA_TYPE> updateFromProvider,
                IModifier<DATA_TYPE> modifier,
                IProvider<INDEXER, DATA_TYPE>.GroupUpdated updated,
                IProvider<INDEXER, DATA_TYPE>.GroupUpdated stopped)
            {
                if (IsAlive(updateFromProvider))
                {
                    live = true;

                    solution.UpdateData(group, dataCount,
                        thisProvider, updateFromProvider,
                        modifier, data, updated,
                        out modifiersWantStayLive);

                    if (modifiersWantStayLive)
                    {
                        SetLastUpdateTime(Helpers.GetTime());
                    }
                }
                else if (live)
                {
                    live = false;

                    solution.DisposeData(group, updateFromProvider, data, updated, stopped);
                }
            }

            protected virtual bool IsAlive(IProvider<INDEXER, DATA_TYPE> updateFromProvider)
            {
                return updateFromProvider.IsAliveWith(lastUpdateTime) || modifiersWantStayLive;
            }
        }
    }
}