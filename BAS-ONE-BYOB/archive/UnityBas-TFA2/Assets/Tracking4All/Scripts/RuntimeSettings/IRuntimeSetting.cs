// IRuntimeSetting
// (C) 2024 G8gaming Ltd.
namespace Tracking4All
{
    /// <summary>
    /// Base implementation for a run time setting of a specific type.
    /// </summary>
    /// <typeparam name="TYPE"></typeparam>
    public interface IRuntimeSetting<TYPE>
    {
        public TYPE Value { get; }

        public void Set(TYPE t);
    }
}