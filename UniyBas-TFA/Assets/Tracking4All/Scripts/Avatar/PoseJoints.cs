// HumanoidJoint
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// The standard names for humanoid avatar joints.
    /// </summary>
    public enum PoseJoints
    {
        Hips,
        Spine,
        Chest,

        Neck,
        Head,

        RightShoulder, RightUpperArm, RightLowerArm, RightHand,
        LeftShoulder, LeftUpperArm, LeftLowerArm, LeftHand,

        RightUpperLeg, RightLowerLeg, RightFoot,
        LeftUpperLeg, LeftLowerLeg, LeftFoot
    }
}