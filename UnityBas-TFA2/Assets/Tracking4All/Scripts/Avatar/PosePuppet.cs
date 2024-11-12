using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Tracking4All
{
    /// <summary>
    /// Calculates joints given pose landmarks provider, provides joints as output.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class PosePuppet : PuppetBase<PoseJoints, PoseJoint>, IPoseJointProvider<PoseJoints>
    {
        [Header("General")]
        [SerializeField] protected LandmarkProvider<PoseLandmarks> poseProvider;
        [SerializeField] protected PuppetBodyPart[] centreBodyParts;
        [SerializeField] protected bool generateEmptyCentreParts; // editor only flags to auto generate the parts given the hierarchy.
        [SerializeField] protected PuppetBodyPart[] rightBodyParts;
        [SerializeField] protected bool generateEmptyRightParts;
        [SerializeField] protected PuppetBodyPart[] leftBodyParts;
        [SerializeField] protected bool generateEmptyLeftParts;
        [Header("Additional Configuration")]
        [SerializeField] protected PoleConfiguration kneePoleConfiguration;
        [SerializeField] protected PoleConfiguration elbowPoleConfiguration;
        [SerializeField] protected float kneeSpacingBias = .03f;

        protected override Table<PoseJoint> Joints => joints;

        protected override void HookProviders()
        {
            if (CanHookProvider(poseProvider))
            {
                poseProvider.OnLandmarksUpdated += PoseProvider_OnLandmarksUpdated;
            }
        }
        protected override void ReleaseProviders()
        {
            if (CanHookProvider(poseProvider))
            {
                poseProvider.OnLandmarksUpdated -= PoseProvider_OnLandmarksUpdated;
            }
        }

        private void OnValidate()
        {
            if (generateEmptyCentreParts && centreBodyParts.Length == 0)
            {
                Logger.LogInfo("Generating parts.");
                centreBodyParts = GetBodyParts(new string[] { "Joint" }, new string[] { ".R", ".L" });
            }

            if (generateEmptyRightParts && rightBodyParts.Length == 0)
            {
                Logger.LogInfo("Generating parts.");
                rightBodyParts = GetBodyParts(new string[] { "Joint", ".R" }, new string[] { ".L" });
            }

            if (generateEmptyLeftParts && leftBodyParts.Length == 0)
            {
                Logger.LogInfo("Generating parts.");
                leftBodyParts = GetBodyParts(new string[] { "Joint", ".L" }, new string[] { ".R" });
            }

            generateEmptyCentreParts = false;
            generateEmptyRightParts = false;
            generateEmptyLeftParts = false;
        }

        private void PoseProvider_OnLandmarksUpdated(int group)
        {
            Tick(0);
        }
        /// <summary>
        /// Calculate the transforms from the pose data.
        /// </summary>
        protected override void CalculateTransforms()
        {
            Vector3 leftHip = Get(PoseLandmarks.LEFT_HIP);
            Vector3 leftShoulder = Get(PoseLandmarks.LEFT_SHOULDER);
            Vector3 rightHip = Get(PoseLandmarks.RIGHT_HIP);
            Vector3 rightShoulder = Get(PoseLandmarks.RIGHT_SHOULDER);

            CalculateTorso(HumanBodyBones.Hips,
                Vector3.Lerp(leftHip, leftShoulder, 0f), Vector3.Lerp(rightHip, rightShoulder, 0f),
                PoseLandmarks.LEFT_SHOULDER, PoseLandmarks.RIGHT_SHOULDER);
            CalculateTorso(HumanBodyBones.Spine,
                Vector3.LerpUnclamped(leftHip, leftShoulder, .3f), Vector3.LerpUnclamped(rightHip, rightShoulder, .3f),
                PoseLandmarks.LEFT_SHOULDER, PoseLandmarks.RIGHT_SHOULDER);
            CalculateTorso(HumanBodyBones.Chest,
                Vector3.LerpUnclamped(leftHip, leftShoulder, .75f), Vector3.LerpUnclamped(rightHip, rightShoulder, .75f),
                PoseLandmarks.LEFT_SHOULDER, PoseLandmarks.RIGHT_SHOULDER);

            CalculateHead();

            CalculateShoulder(HumanBodyBones.RightShoulder,
                PoseLandmarks.RIGHT_SHOULDER, PoseLandmarks.LEFT_SHOULDER, PoseLandmarks.RIGHT_HIP, false);
            CalculateElbow(HumanBodyBones.RightUpperArm,
                PoseLandmarks.RIGHT_SHOULDER, PoseLandmarks.RIGHT_ELBOW, PoseLandmarks.RIGHT_WRIST, Get(PoseLandmarks.RIGHT_ELBOW));
            CalculateForearm(HumanBodyBones.RightLowerArm,
                PoseLandmarks.RIGHT_WRIST, PoseLandmarks.RIGHT_ELBOW, PoseLandmarks.RIGHT_SHOULDER);
            CalculateHand(HumanBodyBones.RightHand,
                PoseLandmarks.RIGHT_WRIST, PoseLandmarks.RIGHT_INDEX, PoseLandmarks.RIGHT_PINKY);

            CalculateUpperLeg(HumanBodyBones.RightUpperLeg,
                PoseLandmarks.RIGHT_HIP, PoseLandmarks.RIGHT_KNEE, PoseLandmarks.RIGHT_ANKLE);
            CalculateLowerLeg(HumanBodyBones.RightLowerLeg,
                PoseLandmarks.RIGHT_HIP, PoseLandmarks.RIGHT_KNEE, PoseLandmarks.RIGHT_ANKLE);
            CalculateFoot(HumanBodyBones.RightFoot,
                PoseLandmarks.RIGHT_HIP, PoseLandmarks.RIGHT_KNEE, PoseLandmarks.RIGHT_HEEL, PoseLandmarks.RIGHT_FOOT);


            // Copied
            CalculateShoulder(HumanBodyBones.LeftShoulder,
                PoseLandmarks.LEFT_SHOULDER, PoseLandmarks.RIGHT_SHOULDER, PoseLandmarks.LEFT_HIP, true);
            CalculateElbow(HumanBodyBones.LeftUpperArm,
                PoseLandmarks.LEFT_SHOULDER, PoseLandmarks.LEFT_ELBOW, PoseLandmarks.LEFT_WRIST, Get(PoseLandmarks.LEFT_ELBOW), true);
            CalculateForearm(HumanBodyBones.LeftLowerArm,
                PoseLandmarks.LEFT_WRIST, PoseLandmarks.LEFT_ELBOW, PoseLandmarks.LEFT_SHOULDER, true);
            CalculateHand(HumanBodyBones.LeftHand,
                PoseLandmarks.LEFT_WRIST, PoseLandmarks.LEFT_INDEX, PoseLandmarks.LEFT_PINKY, true);

            CalculateUpperLeg(HumanBodyBones.LeftUpperLeg,
                PoseLandmarks.LEFT_HIP, PoseLandmarks.LEFT_KNEE, PoseLandmarks.LEFT_ANKLE, true);
            CalculateLowerLeg(HumanBodyBones.LeftLowerLeg,
                PoseLandmarks.LEFT_HIP, PoseLandmarks.LEFT_KNEE, PoseLandmarks.LEFT_ANKLE, true);
            CalculateFoot(HumanBodyBones.LeftFoot,
                PoseLandmarks.LEFT_HIP, PoseLandmarks.LEFT_KNEE, PoseLandmarks.LEFT_HEEL, PoseLandmarks.LEFT_FOOT, true);
        }

        private void CalculateHead()
        {
            Vector3 faceCenter = (Get(PoseLandmarks.LEFT_EAR) + Get(PoseLandmarks.RIGHT_EAR)) / 2f;
            Vector3 faceCenterSurface = (Get(PoseLandmarks.LEFT_EYE_INNER) + Get(PoseLandmarks.RIGHT_EYE_INNER) + Get(PoseLandmarks.NOSE)) / 3f;
            Vector3 rotAxis = (Get(PoseLandmarks.RIGHT_EAR) - Get(PoseLandmarks.LEFT_EAR)).normalized;
            Vector3 faceForward = (faceCenterSurface - faceCenter).normalized;
            Vector3 faceUp = Quaternion.AngleAxis(-90f, rotAxis) * faceForward;
            DrawForward(faceCenter, faceForward);
            DrawUp(faceCenter, faceUp);
            SetRotation(HumanBodyBones.Head, faceForward, faceUp);
            SetRotation(HumanBodyBones.Neck,
                Quaternion.Lerp(puppetPartMap[HumanBodyBones.Chest].BodyPart.rotation, Quaternion.LookRotation(faceForward, faceUp), .8f));
        }
        private void CalculateElbow(HumanBodyBones bone, PoseLandmarks shoulder, PoseLandmarks elbow, PoseLandmarks wrist, Vector3 centre, bool special = false)
        {
            Vector3 armBone = centre;
            Vector3 armForward = (Get(elbow) - Get(shoulder)).normalized;
            Vector3 armRight = (special ? -1 : 1) * CalculateNormal(Get(wrist), ElbowPole(elbow, special), Get(shoulder));
            Vector3 armUp = (special ? -1 : 1) * Vector3.Cross(armForward, armRight);
            DrawForward(armBone, armForward);
            DrawRight(armBone, armRight);
            DrawUp(armBone, armUp);

            SetRotation(bone, -armRight, armUp);
        }
        private void CalculateShoulder(HumanBodyBones bone, PoseLandmarks targetShoulder, PoseLandmarks otherShoulder, PoseLandmarks targetHip, bool special)
        {
            Vector3 upperSpine = (Get(targetShoulder) + Get(otherShoulder)) / 2f;
            Vector3 shoulderBone = Get(targetShoulder);

            Vector3 shoulderForward = (Get(targetShoulder) - upperSpine).normalized;
            Vector3 shoulderRight = (special ? -1 : 1) * CalculateNormal(upperSpine, shoulderBone, Get(targetHip));
            Vector3 shoulderUp = (special ? -1 : 1) * Vector3.Cross(shoulderForward, shoulderRight);
            DrawForward(shoulderBone, shoulderForward);
            DrawRight(shoulderBone, shoulderRight);
            DrawUp(shoulderBone, shoulderUp);

            SetRotation(bone, -shoulderRight, shoulderUp);
        }
        private void CalculateForearm(HumanBodyBones bone, PoseLandmarks wrist, PoseLandmarks elbow, PoseLandmarks shoulder, bool special = false)
        {
            Vector3 forearmBone = Get(wrist);
            Vector3 forearmForward = (Get(wrist) - Get(elbow)).normalized;
            Vector3 forearmRight = (special ? -1 : 1) * CalculateNormal(Get(wrist), ElbowPole(elbow, special), Get(shoulder));
            Vector3 forearmUp = (special ? -1 : 1) * Vector3.Cross(forearmForward, forearmRight);

            DrawForward(forearmBone, forearmForward);
            DrawRight(forearmBone, forearmRight);
            DrawUp(forearmBone, forearmUp);

            SetRotation(bone, -forearmRight, forearmUp);
        }
        private void CalculateHand(HumanBodyBones bone, PoseLandmarks wrist, PoseLandmarks index, PoseLandmarks pinky, bool left = false)
        {
            Vector3 handBone = Get(wrist);
            Vector3 handForward = (Get(wrist) - (Get(index) + Get(pinky)) / 2f).normalized;
            Vector3 handUp = (left ? -1 : 1) * CalculateNormal(Get(pinky), Get(index), Get(wrist));

            Vector3 handRight = (left ? -1 : 1) * Vector3.Cross(handForward, handUp);
            DrawForward(handBone, handForward);
            DrawRight(handBone, handRight);
            DrawUp(handBone, handUp);

            SetRotation(bone, -handRight, handUp);
        }
        private void CalculateTorso(HumanBodyBones bone, Vector3 leftHip, Vector3 rightHip, PoseLandmarks leftShoulder, PoseLandmarks rightShoulder)
        {
            Vector3 torsoCentre = Helpers.Average(leftHip, rightHip, Get(leftShoulder), Get(rightShoulder));
            Vector3 n1 = CalculateNormal(rightHip, Get(leftShoulder), leftHip);
            Vector3 n2 = CalculateNormal(rightHip, Get(rightShoulder), leftHip);
            Vector3 torsoForward = (n1 + n2) / 2f;
            Vector3 torsoRight = (rightHip - leftHip).normalized;
            Vector3 torsoUp = Vector3.Cross(torsoForward, torsoRight);
            DrawForward(torsoCentre, torsoForward);
            DrawRight(torsoCentre, torsoRight);
            DrawUp(torsoCentre, torsoUp);
            SetRotation(bone, torsoForward, torsoUp);
        }
        private void CalculateUpperLeg(HumanBodyBones bone, PoseLandmarks rightHip, PoseLandmarks rightKnee, PoseLandmarks rightFoot, bool mirror = false)
        {
            Vector3 c = Get(rightHip);
            Vector3 forward = (Knee(rightKnee, mirror) - Get(rightHip)).normalized;
            Vector3 right = CalculateNormal(Get(rightHip), KneePole(rightKnee, mirror), Foot(rightFoot, mirror));
            Vector3 up = Vector3.Cross(forward, right);
            DrawForward(c, forward);
            DrawRight(c, right);
            DrawUp(c, up);
            SetRotation(bone, up, -forward);
        }
        private void CalculateLowerLeg(HumanBodyBones bone, PoseLandmarks rightHip, PoseLandmarks rightKnee, PoseLandmarks rightFoot, bool mirror = false)
        {
            Vector3 c = Get(rightKnee);
            Vector3 forward = (Foot(rightFoot, mirror) - Knee(rightKnee, mirror)).normalized;
            Vector3 right = CalculateNormal(Get(rightHip), KneePole(rightKnee, mirror), Foot(rightFoot, mirror));
            Vector3 up = Vector3.Cross(forward, right);
            DrawForward(c, forward);
            DrawRight(c, right);
            DrawUp(c, up);
            SetRotation(bone, up, -forward);
        }
        private void CalculateFoot(HumanBodyBones bone, PoseLandmarks rightHip, PoseLandmarks rightKnee, PoseLandmarks rightFoot, PoseLandmarks rightToes, bool mirror = false)
        {
            Vector3 c = Get(rightKnee);
            Vector3 forward = (Foot(rightToes, mirror) - Foot(rightFoot, mirror)).normalized;
            Vector3 right = CalculateNormal(Get(rightHip), KneePole(rightKnee, mirror), Foot(rightFoot, mirror));
            Vector3 up = Vector3.Cross(forward, right);
            DrawForward(c, forward);
            DrawRight(c, right);
            DrawUp(c, up);
            SetRotation(bone, forward, up);
        }

        private Vector3 KneePole(PoseLandmarks knee, bool left)
        {
            if (!kneePoleConfiguration.usePole) return Get(knee);

            return Get(knee) + GetForward(HumanBodyBones.Hips) * kneePoleConfiguration.forwardDistance
                + GetRight(HumanBodyBones.Hips) * kneePoleConfiguration.lateralDistance * (left ? -1 : 1);
        }
        private Vector3 Knee(PoseLandmarks knee, bool left)
        {
            return Get(knee) + GetRight(HumanBodyBones.Hips) * kneeSpacingBias * (left ? -1 : 1);
        }
        private Vector3 Foot(PoseLandmarks foot, bool left)
        {
            return Get(foot) + GetRight(HumanBodyBones.Hips) * kneeSpacingBias * .6f * (left ? -1 : 1);
        }
        private Vector3 ElbowPole(PoseLandmarks elbow, bool left)
        {
            if (!elbowPoleConfiguration.usePole) return Get(elbow);

            return Get(elbow) - GetForward(HumanBodyBones.Hips) * elbowPoleConfiguration.forwardDistance
                + GetRight(HumanBodyBones.Hips) * elbowPoleConfiguration.lateralDistance * (left ? -1 : 1);
        }

        protected Vector3 Get(PoseLandmarks index)
        {
            if (settings.Mirror)
                return ((IProvider<PoseLandmarks, Landmark>)poseProvider).Get(0, index.Flip()).Position;

            return ((IProvider<PoseLandmarks, Landmark>)poseProvider).Get(0, index).Position;
        }

        public PoseJoint Get(int group, PoseJoints index)
        {
            if (settings.Mirror) return joints.Get(System.Convert.ToInt32(index.Flip()));

            return joints.Get(System.Convert.ToInt32(index));
        }
        public PoseJoint Get(int group, int index)
        {
            return Get(group, (PoseJoints)index);
        }
        public PoseJoint GetAbsoluteJoint(int group, PoseJoints indexer)
        {
            // Get, but absolutely relative to the puppet
            return joints.Get((int)indexer);
        }

        protected override void ConstructJoint(PoseJoints joint, HumanBodyBones bone)
        {
            //if (settings.Mirror) joint = joint.Flip();

            joints.Get((int)joint).Reconstruct(bone, puppetPartMap[bone].BodyPart);
        }

        public override Dictionary<PoseJoints, HumanBodyBones> HumanMapping => mapping;
        public static readonly Dictionary<PoseJoints, HumanBodyBones> mapping = new Dictionary<PoseJoints, HumanBodyBones>()
        {
            { PoseJoints.Hips, HumanBodyBones.Hips },
            { PoseJoints.Spine, HumanBodyBones.Spine },
            { PoseJoints.Chest, HumanBodyBones.Chest },
            { PoseJoints.Neck, HumanBodyBones.Neck },
            { PoseJoints.Head, HumanBodyBones.Head },

            { PoseJoints.RightShoulder, HumanBodyBones.RightShoulder },
            { PoseJoints.RightUpperArm, HumanBodyBones.RightUpperArm },
            { PoseJoints.RightLowerArm, HumanBodyBones.RightLowerArm },
            { PoseJoints.RightHand, HumanBodyBones.RightHand },
            { PoseJoints.RightUpperLeg, HumanBodyBones.RightUpperLeg },
            { PoseJoints.RightLowerLeg, HumanBodyBones.RightLowerLeg },
            { PoseJoints.RightFoot, HumanBodyBones.RightFoot },

            { PoseJoints.LeftShoulder, HumanBodyBones.LeftShoulder },
            { PoseJoints.LeftUpperArm, HumanBodyBones.LeftUpperArm },
            { PoseJoints.LeftLowerArm, HumanBodyBones.LeftLowerArm },
            { PoseJoints.LeftHand, HumanBodyBones.LeftHand },
            { PoseJoints.LeftUpperLeg, HumanBodyBones.LeftUpperLeg },
            { PoseJoints.LeftLowerLeg, HumanBodyBones.LeftLowerLeg },
            { PoseJoints.LeftFoot, HumanBodyBones.LeftFoot }
        };

        public override Dictionary<HumanBodyBones, PoseJoints> JointMapping => reversedMapping;
        public static readonly Dictionary<HumanBodyBones, PoseJoints> reversedMapping = mapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        protected override List<PuppetBodyPart> GetPartsRange()
        {
            List<PuppetBodyPart> range = new List<PuppetBodyPart>();
            range.AddRange(centreBodyParts);
            range.AddRange(rightBodyParts);
            range.AddRange(leftBodyParts);
            return range;
        }

        protected void ConstructJoint(PoseJoint joint, HumanBodyBones bone)
        {
            joints.Get(System.Convert.ToInt32(bone))
                .Reconstruct(bone, puppetPartMap[bone].BodyPart);
        }

        [System.Serializable]
        public class PoleConfiguration
        {
            [Tooltip("Whether or not to use the pole to orientate the knees, if false this is legacy behavior.")]
            public bool usePole = true;
            [Tooltip("The forward distance from the joint to place the pole.")]
            public float forwardDistance = 0.2f;
            [Tooltip("The right/left distance from the joint to place the pole.")]
            public float lateralDistance = 0.2f;
        }
    }
}
