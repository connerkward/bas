namespace Tracking4All
{
    public interface IModifier<DATA_TYPE>
    {
        public bool Enabled { get; }

        public void PreUpdate(float deltaTime);
        public void Modify(ref DATA_TYPE current, ref DATA_TYPE target, float deltaTime);
    }
}