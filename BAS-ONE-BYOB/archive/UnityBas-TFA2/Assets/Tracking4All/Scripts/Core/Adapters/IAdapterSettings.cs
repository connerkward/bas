namespace Tracking4All
{
    public interface IAdapterSettings
    {
        /// <summary>
        /// If true, use the cameras perspective. If false, use the players perspective.
        /// </summary>
        public bool PerspectiveFlip { get; }
        /// <summary>
        /// If true, flip right and left of the pose.
        /// </summary>
        public bool Mirror { get; }
    }
}