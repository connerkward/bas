// IJointProvider
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    public interface IJointProvider<INDEXER> : IProvider<INDEXER, PuppetJoint>
        where INDEXER : System.Enum
    {
        public event GroupUpdated OnJointsUpdated;
    }
}