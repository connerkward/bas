// IJointProvider
// (C) 2024 G8gaming Ltd.
namespace Tracking4All
{
    public interface IJointProvider<INDEXER, JOINT_TYPE> : IProvider<INDEXER, JOINT_TYPE>
        where INDEXER : System.Enum
        where JOINT_TYPE : PuppetJoint, new()
    {
        // Or 1 "bidirectional dictionary"
        // public Dictionary<INDEXER, HumanBodyBones> HumanMapping { get; }
        // public Dictionary<HumanBodyBones, INDEXER> JointMapping { get; }

        /// <summary>
        /// Get the joint relative to the avatar/puppet only.
        /// <para>I.e. ignores mirroring and such (you probably don't want this).</para>
        /// </summary>
        /// <param name="group"></param>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public JOINT_TYPE GetAbsoluteJoint(int group, INDEXER indexer);


        public event GroupUpdated OnJointsUpdated;
    }
}