// Avatar
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    [RequireComponent(typeof(AvatarSkeleton))]
    /// <summary>
    /// The (Humanoid) Avatar is comprised of AvatarJointProvider(s) that receive data from joint provider(s) to drive the motion of a humanoid using it's AvatarSkeleton.
    /// </summary>
    public class Avatar : MonoBehaviour
    {
        [SerializeField] protected AvatarSkeleton avatarSkeleton;
        [SerializeField] protected AvatarJointBoneProvider<PoseJoints, PoseJoint> pose;
        [SerializeField] protected AvatarJointBoneProvider<HandJoints, HandJoint> rightHand;
        [SerializeField] protected AvatarJointBoneProvider<HandJoints, HandJoint> leftHand;

        private void OnValidate()
        {
            if (avatarSkeleton == null) avatarSkeleton = GetComponent<AvatarSkeleton>();
        }
        private void OnEnable()
        {
            ResetAvatar();
        }
        private void OnDisable()
        {
            ReleaseAvatar();
        }

        private void InitializeDrivers()
        {
            // Depending on the combination of available joints, set drivers to the skeleton.

            if (pose.HasProvider)
            {
                SetAsPoseDriver(HumanBodyBones.Head);

                SetAsPoseDriver(HumanBodyBones.Hips);
                SetAsPoseDriver(HumanBodyBones.Spine);
                SetAsPoseDriver(HumanBodyBones.Chest);

                SetAsPoseDriver(HumanBodyBones.RightShoulder);
                SetAsPoseDriver(HumanBodyBones.RightUpperArm);
                SetAsPoseDriver(HumanBodyBones.RightLowerArm);
                SetAsPoseDriver(HumanBodyBones.RightUpperLeg);
                SetAsPoseDriver(HumanBodyBones.RightLowerLeg);
                SetAsPoseDriver(HumanBodyBones.RightFoot);

                SetAsPoseDriver(HumanBodyBones.LeftShoulder);
                SetAsPoseDriver(HumanBodyBones.LeftUpperArm);
                SetAsPoseDriver(HumanBodyBones.LeftLowerArm);
                SetAsPoseDriver(HumanBodyBones.LeftUpperLeg);
                SetAsPoseDriver(HumanBodyBones.LeftLowerLeg);
                SetAsPoseDriver(HumanBodyBones.LeftFoot);

                if (!rightHand.HasProvider) SetAsPoseDriver(HumanBodyBones.RightHand);
                if (!leftHand.HasProvider) SetAsPoseDriver(HumanBodyBones.LeftHand);
            }

            if (rightHand.HasProvider)
            {
                SetAsRightHandDriver(HumanBodyBones.RightHand);

                SetAsRightHandDriver(HumanBodyBones.RightIndexDistal);
                SetAsRightHandDriver(HumanBodyBones.RightIndexIntermediate);
                SetAsRightHandDriver(HumanBodyBones.RightIndexProximal);

                SetAsRightHandDriver(HumanBodyBones.RightMiddleDistal);
                SetAsRightHandDriver(HumanBodyBones.RightMiddleIntermediate);
                SetAsRightHandDriver(HumanBodyBones.RightMiddleProximal);

                SetAsRightHandDriver(HumanBodyBones.RightRingDistal);
                SetAsRightHandDriver(HumanBodyBones.RightRingIntermediate);
                SetAsRightHandDriver(HumanBodyBones.RightRingProximal);

                SetAsRightHandDriver(HumanBodyBones.RightLittleDistal);
                SetAsRightHandDriver(HumanBodyBones.RightLittleIntermediate);
                SetAsRightHandDriver(HumanBodyBones.RightLittleProximal);

                SetAsRightHandDriver(HumanBodyBones.RightThumbDistal);
                SetAsRightHandDriver(HumanBodyBones.RightThumbIntermediate);
                SetAsRightHandDriver(HumanBodyBones.RightThumbProximal);
            }

            if (leftHand.HasProvider)
            {
                SetAsLeftHandDriver(HumanBodyBones.LeftHand);

                SetAsLeftHandDriver(HumanBodyBones.LeftIndexDistal);
                SetAsLeftHandDriver(HumanBodyBones.LeftIndexIntermediate);
                SetAsLeftHandDriver(HumanBodyBones.LeftIndexProximal);

                SetAsLeftHandDriver(HumanBodyBones.LeftMiddleDistal);
                SetAsLeftHandDriver(HumanBodyBones.LeftMiddleIntermediate);
                SetAsLeftHandDriver(HumanBodyBones.LeftMiddleProximal);

                SetAsLeftHandDriver(HumanBodyBones.LeftRingDistal);
                SetAsLeftHandDriver(HumanBodyBones.LeftRingIntermediate);
                SetAsLeftHandDriver(HumanBodyBones.LeftRingProximal);

                SetAsLeftHandDriver(HumanBodyBones.LeftLittleDistal);
                SetAsLeftHandDriver(HumanBodyBones.LeftLittleIntermediate);
                SetAsLeftHandDriver(HumanBodyBones.LeftLittleProximal);

                SetAsLeftHandDriver(HumanBodyBones.LeftThumbDistal);
                SetAsLeftHandDriver(HumanBodyBones.LeftThumbIntermediate);
                SetAsLeftHandDriver(HumanBodyBones.LeftThumbProximal);
            }

        }

        /// <summary>
        /// Resets and rebuilds data to use the avatar. Useful if you switch out the provider at runtime.
        /// </summary>
        /// <param name="poseProvider"></param>
        public void ResetAvatar(
            IPoseJointProvider<PoseJoints> poseProvider = null,
            IHandJointProvider<HandJoints> rightHandProvider = null,
            IHandJointProvider<HandJoints> leftHandProvider = null)
        {
            // can sometimes be null, probably due to serialized field not creating object yet???
            if (pose == null) pose = new AvatarJointBoneProvider<PoseJoints, PoseJoint>();
            if (rightHand == null) rightHand = new AvatarJointBoneProvider<HandJoints, HandJoint>();
            if (leftHand == null) leftHand = new AvatarJointBoneProvider<HandJoints, HandJoint>();

            if (avatarSkeleton == null) avatarSkeleton = GetComponent<AvatarSkeleton>();

            // Reset the skeleton to the binding pose.
            avatarSkeleton.ResetPose();

            // Release drivers before rebuilding.
            ReleaseAvatar();

            // Rebuild drivers.
            pose.Reset(avatarSkeleton, poseProvider, PosePuppet.mapping);
            rightHand.Reset(avatarSkeleton, rightHandProvider, HandPuppet.GetMapping(Handedness.RIGHT));
            leftHand.Reset(avatarSkeleton, leftHandProvider, HandPuppet.GetMapping(Handedness.LEFT));

            // Set drivers on the skeleton.
            InitializeDrivers();
        }
        private void ReleaseAvatar()
        {
            if (pose.HasProvider) pose.Release(avatarSkeleton);
            if (rightHand.HasProvider) rightHand.Release(avatarSkeleton);
            if (leftHand.HasProvider) leftHand.Release(avatarSkeleton);
        }

        private void SetAsPoseDriver(HumanBodyBones bone)
        {
            var d = pose.GetDriver(bone);
            d.Initialize(transform, bone);
            avatarSkeleton.SetDriver(bone, d);
        }
        private void SetAsRightHandDriver(HumanBodyBones bone)
        {
            var d = rightHand.GetDriver(bone);
            d.Initialize(transform, bone);
            avatarSkeleton.SetDriver(bone, d);
        }
        private void SetAsLeftHandDriver(HumanBodyBones bone)
        {
            var d = leftHand.GetDriver(bone);
            d.Initialize(transform, bone);
            avatarSkeleton.SetDriver(bone, d);
        }
    }
}