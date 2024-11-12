// DriverPair
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Just stores data and drives the animator bone given the puppet transform.
    /// <para>Given a PuppetJoint and corresponding AvatarSkeletonBone calculate the avatar rotation.</para>
    /// </summary>
    public class DirectJointBoneDriver : ISkeletonJointDriver
    {
        private PuppetJoint joint;

        private Transform jointAbsoluteParent;
        private HumanBodyBones bone;

        // Hooks to references so that the puppet joint can reconstruct itself whenever.
        public Transform JointTransform => joint.PuppetJointTransform;
        private Quaternion jointBindRot => joint.BindingRotation;

        private bool initialized = false;
        private Transform vp;

        public DirectJointBoneDriver(PuppetJoint joint)
        {
            this.joint = joint;
        }

        public void Initialize(Transform jointAbsoluteParent, HumanBodyBones bone)
        {
            this.jointAbsoluteParent = jointAbsoluteParent;
            this.bone = bone;
            initialized = true;
        }
        public void Update(AvatarSkeleton skeleton)
        {
            if (!joint.IsWellConstructed)
            {
                Logger.LogWarning("Tried to update with a badly constructed input. Make sure joint providers are running before dependencies.");
                return;
            }
            if (!initialized)
            {
                Logger.LogWarning("Joint driver was not initialized before update was called! Must initialize first…");
                return;
            }

            Quaternion trackRot = Quaternion.LookRotation(
                jointAbsoluteParent.localToWorldMatrix.MultiplyVector(JointTransform.forward),
                jointAbsoluteParent.localToWorldMatrix.MultiplyVector(JointTransform.up));

            skeleton.SetRotation(bone, (trackRot * Quaternion.Inverse(jointBindRot)) * skeleton.GetBindingRotation(bone));
        }
        public void Dispose()
        {
            initialized = false;
        }
    }
}