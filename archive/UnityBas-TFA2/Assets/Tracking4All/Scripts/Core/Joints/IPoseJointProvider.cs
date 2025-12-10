// IPoseJointProvider
// (C) 2024 G8gaming Ltd.
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    public interface IPoseJointProvider<INDEXER> : IJointProvider<INDEXER, PoseJoint>
    where INDEXER : System.Enum
    {
        public Dictionary<INDEXER, HumanBodyBones> HumanMapping { get; }

        public new event GroupUpdated OnJointsUpdated;
    }
}