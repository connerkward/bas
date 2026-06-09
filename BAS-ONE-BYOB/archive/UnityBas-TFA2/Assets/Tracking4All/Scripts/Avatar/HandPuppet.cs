// HandPuppet
// (C) 2024 G8gaming Ltd.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Calculates joints using a mixture of regular and normalized hand landmarks.
    /// </summary>
    public class HandPuppet : PuppetBase<HandJoints, HandJoint>,
        IHandJointProvider<HandJoints>
    {
        [Header("General")]
        [SerializeField] private Handedness handedness;
        [SerializeField] protected LandmarkProvider<HandLandmarks> handProvider;
        [SerializeField] protected PuppetJointProvider<PoseJoints, PoseJoint> optionalPosePuppet;
        [SerializeField] protected PuppetBodyPart[] centreBodyParts;
        [Header("Constraints")]
        [SerializeField] private PitchYawConstraint[] digitConstraints;
        [Header("Editor Only")]
        [SerializeField] protected bool generateEmptyCentreParts; // editor only flags to auto generate the parts given the hierarchy.
        [SerializeField] protected bool editFlipParts; // editor only flag to auto flip the humanbodybone values

        protected override Table<HandJoint> Joints => joints;
        private Dictionary<HandJoints, PitchYawConstraint> digitConstraintsMap = new Dictionary<HandJoints, PitchYawConstraint>();

        protected override void Awake()
        {
            base.Awake();

            foreach (var d in digitConstraints)
            {
                digitConstraintsMap.Add(d.joint, d);
            }
        }
        protected void Start()
        {
            Transform originalParent = transform.parent;
            if (optionalPosePuppet.HasInterface)
            {
                switch (handedness)
                {
                    case Handedness.RIGHT:
                        transform.SetParent(
                            optionalPosePuppet.GetAbsoluteJoint(0, PoseJoints.RightHand).PuppetJointTransform,
                            true);
                        break;

                    case Handedness.LEFT:
                        transform.SetParent(
                            optionalPosePuppet.GetAbsoluteJoint(0, PoseJoints.LeftHand).PuppetJointTransform,
                            true);
                        break;
                }

                if (transform.parent != originalParent && transform.parent != null)
                    transform.localPosition = Vector3.zero;
            }
        }

        protected override void HookProviders()
        {
            if (CanHookProvider(handProvider))
            {
                handProvider.OnLandmarksUpdated += HandProvider_OnLandmarksUpdated;
            }
        }
        protected override void ReleaseProviders()
        {
            if (CanHookProvider(handProvider))
            {
                handProvider.OnLandmarksUpdated += HandProvider_OnLandmarksUpdated;
            }
        }
        private void OnValidate()
        {
            if (generateEmptyCentreParts && centreBodyParts.Length == 0)
            {
                Logger.LogInfo("Generating parts.");
                centreBodyParts = GetBodyParts(new string[] { "Joint" }, new string[] { });
            }
            if (editFlipParts && centreBodyParts.Length != 0)
            {
                int c = 0;
                Logger.LogInfo("Modifying parts.");
                foreach (var part in centreBodyParts)
                {
                    object parse = null;

                    HumanBodyBones v = part.Correspondence;
                    string s = v.ToString();
                    if (s.Contains("Right")) s = s.Replace("Right", "Left");
                    else if (s.Contains("Left")) s = s.Replace("Left", "Right");

                    if (System.Enum.TryParse(typeof(HumanBodyBones), s, out parse))
                    {
                        print(s + " to " + parse.ToString());
                        part.SetCorrespondence((HumanBodyBones)parse);
                        ++c;
                    }
                    else
                    {
                        Logger.LogError("Failed to parse a flip for " + s);
                    }
                }
                Logger.LogInfo("Modified " + c + " parts.");
            }

            generateEmptyCentreParts = false;
            editFlipParts = false;
        }

        private void HandProvider_OnLandmarksUpdated(int group)
        {
            Tick((int)handedness);
        }

        protected override void CalculateTransforms()
        {
            Vector3 wrist = Get(HandLandmarks.WRIST);
            Vector3 index0 = Get(HandLandmarks.INDEX2);
            Vector3 ring0 = Get(HandLandmarks.RING2);
            Vector3 middle0 = Get(HandLandmarks.MIDDLE1);

            Vector3 handForwardCentre = Helpers.Average(middle0, ring0, index0);
            Vector3 handForward = (handForwardCentre - wrist);
            Vector3 handUp = CalculateNormal(wrist, index0, ring0);
            Vector3 handRight = Vector3.Cross(handForward, handUp);
            Vector3 handLeft = -handRight;

            switch (handedness)
            {
                case Handedness.RIGHT:
                    CalculatePalm(HumanBodyBones.RightHand, HandLandmarks.WRIST,
                        HandLandmarks.INDEX2, HandLandmarks.RING2, HandLandmarks.MIDDLE1);
                    CalculateFingerChain(HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal,
                        HandLandmarks.INDEX1, HandLandmarks.INDEX2, HandLandmarks.INDEX3, HandLandmarks.INDEX4, true);
                    CalculateFingerChain(HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal,
                        HandLandmarks.MIDDLE1, HandLandmarks.MIDDLE2, HandLandmarks.MIDDLE3, HandLandmarks.MIDDLE4, true);
                    CalculateFingerChain(HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal,
                        HandLandmarks.RING1, HandLandmarks.RING2, HandLandmarks.RING3, HandLandmarks.RING4, true);
                    CalculateFingerChain(HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal,
                        HandLandmarks.PINKY1, HandLandmarks.PINKY2, HandLandmarks.PINKY3, HandLandmarks.PINKY4, true);
                    CalculateThumbChain(HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal,
                        HandLandmarks.THUMB1, HandLandmarks.THUMB2, HandLandmarks.THUMB3, HandLandmarks.THUMB4, true);
                    break;

                case Handedness.LEFT:
                    CalculatePalm(HumanBodyBones.LeftHand, HandLandmarks.WRIST,
                        HandLandmarks.INDEX2, HandLandmarks.RING2, HandLandmarks.MIDDLE1);
                    CalculateFingerChain(HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal,
                        HandLandmarks.INDEX1, HandLandmarks.INDEX2, HandLandmarks.INDEX3, HandLandmarks.INDEX4, false);
                    CalculateFingerChain(HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal,
                        HandLandmarks.MIDDLE1, HandLandmarks.MIDDLE2, HandLandmarks.MIDDLE3, HandLandmarks.MIDDLE4, false);
                    CalculateFingerChain(HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal,
                        HandLandmarks.RING1, HandLandmarks.RING2, HandLandmarks.RING3, HandLandmarks.RING4, false);
                    CalculateFingerChain(HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal,
                        HandLandmarks.PINKY1, HandLandmarks.PINKY2, HandLandmarks.PINKY3, HandLandmarks.PINKY4, false);
                    CalculateThumbChain(HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal,
                        HandLandmarks.THUMB1, HandLandmarks.THUMB2, HandLandmarks.THUMB3, HandLandmarks.THUMB4, false);
                    break;
            }
        }

        private Vector3 handUp, handForward, handRight;

        private void CalculatePalm(HumanBodyBones palm, HandLandmarks wrist, HandLandmarks i0, HandLandmarks r0, HandLandmarks m0)
        {
            Vector3 handForwardCentre = Helpers.Average(Get(m0), Get(r0), Get(i0));
            Vector3 handForward = (handForwardCentre - Get(wrist));
            Vector3 handUp = CalculateNormal(Get(wrist), Get(i0), Get(r0));
            Vector3 handRight = Vector3.Cross(handForward, handUp);

            if (handedness == Handedness.LEFT) handUp *= -1;

            this.handUp = handUp;
            this.handForward = handForward;
            this.handRight = handRight;

            Vector3 centre = Get(wrist);
            DrawForward(centre, handForward);
            DrawUp(centre, handUp);
            DrawRight(centre, handRight);
            SetRotation(palm, handForward, handUp);
        }
        private void CalculateFingerChain(HumanBodyBones bone0, HumanBodyBones bone1, HumanBodyBones bone2,
            HandLandmarks zero, HandLandmarks one, HandLandmarks two, HandLandmarks three, bool rightHand)
        {
            Vector3 forward01 = (Get(one) - Get(zero)).normalized;
            Vector3 forward12 = (Get(two) - Get(one)).normalized;
            Vector3 forward23 = (Get(three) - Get(two)).normalized;

            Vector3 right = rightHand ? handRight : -handRight;

            Vector3 up01 = Vector3.Cross(right, forward01);
            Vector3 up12 = Vector3.Cross(right, forward12);
            Vector3 up23 = Vector3.Cross(right, forward23);

            SetLocalRotation(
                bone0,
                AxisAngleYaw(handForward, forward01, handUp, bone0),
                AxisAnglePitch(handForward, forward01, -right, bone0)); // base of finger

            SetLocalRotation(
                bone1,
                AxisAngleYaw(forward01, forward12, handUp, bone1),
                AxisAnglePitch(forward01, forward12, -right, bone1));

            SetLocalRotation(
                bone2,
                AxisAngleYaw(forward12, forward23, handUp, bone2),
                AxisAnglePitch(forward12, forward23, -right, bone2));
        }
        private void CalculateThumbChain(HumanBodyBones bone0, HumanBodyBones bone1, HumanBodyBones bone2,
            HandLandmarks zero, HandLandmarks one, HandLandmarks two, HandLandmarks three, bool rightHand)
        {
            Vector3 forward01 = (Get(one) - Get(zero)).normalized;
            Vector3 forward12 = (Get(two) - Get(one)).normalized;
            Vector3 forward23 = (Get(three) - Get(two)).normalized;

            HandJoint thumbBase = joints.Get((int)JointMapping[bone0]);

            float sign = rightHand ? -1 : 1;

            forward01 =
                Quaternion.AngleAxis(
                    sign * AxisAnglePitch(thumbBase.BindingForward, forward01, sign * thumbBase.BindingRight, bone0),
                    thumbBase.BindingRight) // pitch
                * Quaternion.AngleAxis(
                    sign * AxisAngleYaw(thumbBase.BindingForward, forward01, sign * thumbBase.BindingUp, bone0),
                    thumbBase.BindingUp) // yaw
                * thumbBase.BindingForward;
            forward12 =
                Quaternion.AngleAxis(
                    sign * AxisAnglePitch(forward01, forward12, sign * thumbBase.BindingRight, bone1),
                    thumbBase.BindingRight) // pitch
                * Quaternion.AngleAxis(
                    sign * AxisAngleYaw(forward01, forward12, sign * thumbBase.BindingUp, bone1),
                    thumbBase.BindingUp) // yaw
                * forward01;
            forward23 =
                Quaternion.AngleAxis(
                    sign * AxisAnglePitch(forward12, forward23, sign * thumbBase.BindingRight, bone2),
                    thumbBase.BindingRight) // pitch
                * Quaternion.AngleAxis(
                    sign * AxisAngleYaw(forward12, forward23, sign * thumbBase.BindingUp, bone2),
                    thumbBase.BindingUp) // yaw
                * forward12;

            Vector3 up = rightHand ? -handRight : handRight;

            Vector3 right01 = Vector3.Cross(forward01, up);
            Vector3 right12 = Vector3.Cross(forward12, up);
            Vector3 right23 = Vector3.Cross(forward23, up);

            SetRotation(bone0, forward01, up);
            SetRotation(bone1, forward12, up);
            SetRotation(bone2, forward23, up);
        }

        private float AxisAnglePitch(Vector3 relativeDirection, Vector3 direction, Vector3 rotationAxis,
           HumanBodyBones bone)
        {
            PitchYawConstraint constraint = null;
            if (digitConstraintsMap.ContainsKey(JointMapping[bone]) == false)
            {
                return AxisAngle(relativeDirection, direction, rotationAxis, -360, 360);
            }
            constraint = digitConstraintsMap[JointMapping[bone]];

            float offset = joints.Get((int)JointMapping[bone]).RestPitch;
            return AxisAngle(relativeDirection, direction, rotationAxis,
                constraint.pitchMinDeviation + offset, constraint.pitchMaxDeviation + offset);
        }
        private float AxisAngleYaw(Vector3 relativeDirection, Vector3 direction, Vector3 rotationAxis,
            HumanBodyBones bone)
        {
            PitchYawConstraint constraint = null;
            if (digitConstraintsMap.ContainsKey(JointMapping[bone]) == false)
            {
                return AxisAngle(relativeDirection, direction, rotationAxis, -360, 360);
            }
            constraint = digitConstraintsMap[JointMapping[bone]];

            float offset = joints.Get((int)JointMapping[bone]).RestYaw;
            return AxisAngle(relativeDirection, direction, rotationAxis,
                constraint.yawMinDeviation + offset, constraint.yawMaxDeviation + offset);
        }
        private float AxisAngle(Vector3 relativeDirection, Vector3 direction, Vector3 rotationAxis,
            float minimumAngle, float maximumAngle)
        {
            relativeDirection = Vector3.ProjectOnPlane(relativeDirection, rotationAxis);
            direction = Vector3.ProjectOnPlane(direction, rotationAxis);

            float angle = Vector3.SignedAngle(relativeDirection, direction, rotationAxis);
            angle = Mathf.Clamp(angle, minimumAngle, maximumAngle);
            return angle;
        }

        protected void SetLocalRotation(HumanBodyBones t, float yaw, float pitch)
        {
            if (float.IsNaN(yaw) || float.IsNaN(pitch)) return;

            joints.Get((int)JointMapping[t]).SetRotation(yaw, pitch);
        }

        protected Vector3 Get(HandLandmarks index)
        {
            // Flipping hands (puppet left becomes your right).
            if (settings.Provider.Mirror)
                return ((IProvider<HandLandmarks, Landmark>)handProvider).Get((int)handedness.Flip(), index).Position;

            return ((IProvider<HandLandmarks, Landmark>)handProvider).Get((int)handedness, index).Position;
        }

        public HandJoint Get(int group, HandJoints index)
        {
            return joints.Get((int)index);
        }

        public HandJoint Get(int group, int index)
        {
            return joints.Get(index);
        }

        protected override List<PuppetBodyPart> GetPartsRange()
        {
            return new List<PuppetBodyPart>(centreBodyParts);
        }

        protected override void ConstructJoint(HandJoints joint, HumanBodyBones bone)
        {
            //if (settings.Mirror) joint = joint.Flip();

            joints.Get((int)joint).Reconstruct(bone, puppetPartMap[bone].BodyPart, handedness);
        }

        public static float GetFingerPitchSign(Handedness handedness)
        {
            switch (handedness)
            {
                case Handedness.LEFT:
                    return -1;
                case Handedness.RIGHT:
                    return 1;
            }
            return 0;
        }
        public static float GetFingerYawSign(Handedness handedness)
        {
            // Towards the thumb is positive
            switch (handedness)
            {
                case Handedness.LEFT:
                    return 1;
                case Handedness.RIGHT:
                    return -1;
            }
            return 0;
        }

        public override Dictionary<HandJoints, HumanBodyBones> HumanMapping => GetMapping(handedness);
        private static readonly Dictionary<HandJoints, HumanBodyBones> rightHandMapping = new Dictionary<HandJoints, HumanBodyBones>()
        {
            { HandJoints.Wrist, HumanBodyBones.RightHand },
            { HandJoints.Index0, HumanBodyBones.RightIndexProximal },
            { HandJoints.Index1, HumanBodyBones.RightIndexIntermediate },
            { HandJoints.Index2, HumanBodyBones.RightIndexDistal },
            { HandJoints.Middle0, HumanBodyBones.RightMiddleProximal },
            { HandJoints.Middle1, HumanBodyBones.RightMiddleIntermediate },
            { HandJoints.Middle2, HumanBodyBones.RightMiddleDistal },
            { HandJoints.Ring0, HumanBodyBones.RightRingProximal },
            { HandJoints.Ring1, HumanBodyBones.RightRingIntermediate },
            { HandJoints.Ring2, HumanBodyBones.RightRingDistal },
            { HandJoints.Pinky0, HumanBodyBones.RightLittleProximal },
            { HandJoints.Pinky1, HumanBodyBones.RightLittleIntermediate },
            { HandJoints.Pinky2, HumanBodyBones.RightLittleDistal },
            { HandJoints.Thumb0, HumanBodyBones.RightThumbProximal },
            { HandJoints.Thumb1, HumanBodyBones.RightThumbIntermediate },
            { HandJoints.Thumb2, HumanBodyBones.RightThumbDistal }
        };
        private static readonly Dictionary<HandJoints, HumanBodyBones> leftHandMapping = new Dictionary<HandJoints, HumanBodyBones>()
        {
            { HandJoints.Wrist, HumanBodyBones.LeftHand },
            { HandJoints.Index0, HumanBodyBones.LeftIndexProximal },
            { HandJoints.Index1, HumanBodyBones.LeftIndexIntermediate },
            { HandJoints.Index2, HumanBodyBones.LeftIndexDistal },
            { HandJoints.Middle0, HumanBodyBones.LeftMiddleProximal },
            { HandJoints.Middle1, HumanBodyBones.LeftMiddleIntermediate },
            { HandJoints.Middle2, HumanBodyBones.LeftMiddleDistal },
            { HandJoints.Ring0, HumanBodyBones.LeftRingProximal },
            { HandJoints.Ring1, HumanBodyBones.LeftRingIntermediate },
            { HandJoints.Ring2, HumanBodyBones.LeftRingDistal },
            { HandJoints.Pinky0, HumanBodyBones.LeftLittleProximal },
            { HandJoints.Pinky1, HumanBodyBones.LeftLittleIntermediate },
            { HandJoints.Pinky2, HumanBodyBones.LeftLittleDistal },
            { HandJoints.Thumb0, HumanBodyBones.LeftThumbProximal },
            { HandJoints.Thumb1, HumanBodyBones.LeftThumbIntermediate },
            { HandJoints.Thumb2, HumanBodyBones.LeftThumbDistal }
        };
        public static Dictionary<HandJoints, HumanBodyBones> GetMapping(Handedness handedness)
        {
            switch (handedness)
            {
                case Handedness.RIGHT:
                    return rightHandMapping;
                case Handedness.LEFT:
                    return leftHandMapping;
            }

            throw new NotImplementedException();
        }

        public override Dictionary<HumanBodyBones, HandJoints> JointMapping => GetMappingReversed();
        private static readonly Dictionary<HumanBodyBones, HandJoints> reversedRightHandMapping = rightHandMapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        private static readonly Dictionary<HumanBodyBones, HandJoints> reversedLeftHandMapping = leftHandMapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        private Dictionary<HumanBodyBones, HandJoints> GetMappingReversed()
        {
            switch (handedness)
            {
                case Handedness.RIGHT:
                    return reversedRightHandMapping;
                case Handedness.LEFT:
                    return reversedLeftHandMapping;
            }

            Logger.LogError("Unable to handle handedness.", gameObject.name);
            return null;
        }

        public HandJoint GetAbsoluteJoint(int group, HandJoints indexer)
        {
            return joints.Get((int)indexer);
        }

        [System.Serializable]
        public class PitchYawConstraint
        {
            public HandJoints joint;

            public float yawMaxDeviation = 20;
            public float yawMinDeviation = -20;

            public float pitchMaxDeviation = 90;
            public float pitchMinDeviation = -10;
        }
    }
}