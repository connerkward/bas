using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tracking4All;

namespace Tracking4All
{
    /// <summary>
    /// Calculates joints given pose landmarks provider, provides joints as output.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class Puppet : MonoBehaviour, IJointProvider<PoseJoints>
    {
        [SerializeField] protected LandmarkProvider<PoseLandmarks> poseProvider;
        [SerializeField] protected PuppetBodyPart[] centreBodyParts;
        [SerializeField] protected bool generateEmptyCentreParts; // editor only flags to auto generate the parts given the hierarchy.
        [SerializeField] protected PuppetBodyPart[] rightBodyParts;
        [SerializeField] protected bool generateEmptyRightParts;
        [SerializeField] protected PuppetBodyPart[] leftBodyParts;
        [SerializeField] protected bool generateEmptyLeftParts;

        private Dictionary<HumanBodyBones, PuppetBodyPart> puppetPartMap = new Dictionary<HumanBodyBones, PuppetBodyPart>();
        private Table<PuppetJoint> joints = new Table<PuppetJoint>(Helpers.GetLength(typeof(PoseJoints)));
        private void SetJoint(PoseJoints joint, HumanBodyBones bone)
        {
            joints.Get((int)joint).Reconstruct(bone, puppetPartMap[bone].BodyPart);
            //joints.Set((int)joint, new PuppetJoint(bone, puppetPartMap[bone].BodyPart)); // Avatars might have already hooked the reference...
        }

        public int DataCount => joints.Count;

        public float TimeSinceLastUpdate => lastUpdateTime;
        protected float lastUpdateTime;

        public event IProvider<PoseJoints, PuppetJoint>.GroupUpdated OnJointsUpdated;

        private void Awake()
        {
            List<PuppetBodyPart> range = new List<PuppetBodyPart>();
            range.AddRange(centreBodyParts);
            range.AddRange(rightBodyParts);
            range.AddRange(leftBodyParts);
            foreach (var p in range)
            {
                puppetPartMap.Add(p.Correspondence, p);
            }

            SetJoint(PoseJoints.Hips, HumanBodyBones.Hips);
            SetJoint(PoseJoints.Spine, HumanBodyBones.Spine);
            SetJoint(PoseJoints.Chest, HumanBodyBones.Chest);
            SetJoint(PoseJoints.Neck, HumanBodyBones.Neck);
            SetJoint(PoseJoints.Head, HumanBodyBones.Head);

            SetJoint(PoseJoints.RightShoulder, HumanBodyBones.RightShoulder);
            SetJoint(PoseJoints.RightUpperArm, HumanBodyBones.RightUpperArm);
            SetJoint(PoseJoints.RightLowerArm, HumanBodyBones.RightLowerArm);
            SetJoint(PoseJoints.RightHand, HumanBodyBones.RightHand);
            SetJoint(PoseJoints.RightUpperLeg, HumanBodyBones.RightUpperLeg);
            SetJoint(PoseJoints.RightLowerLeg, HumanBodyBones.RightLowerLeg);
            SetJoint(PoseJoints.RightFoot, HumanBodyBones.RightFoot);

            SetJoint(PoseJoints.LeftShoulder, HumanBodyBones.LeftShoulder);
            SetJoint(PoseJoints.LeftUpperArm, HumanBodyBones.LeftUpperArm);
            SetJoint(PoseJoints.LeftLowerArm, HumanBodyBones.LeftLowerArm);
            SetJoint(PoseJoints.LeftHand, HumanBodyBones.LeftHand);
            SetJoint(PoseJoints.LeftUpperLeg, HumanBodyBones.LeftUpperLeg);
            SetJoint(PoseJoints.LeftLowerLeg, HumanBodyBones.LeftLowerLeg);
            SetJoint(PoseJoints.LeftFoot, HumanBodyBones.LeftFoot);
        }
        private void OnEnable()
        {
            if (!poseProvider.HasInterface)
            {
                Logger.LogWarning("Pose provider is not assigned or is not valid.",gameObject.name);
                return;
            }
            
            poseProvider.OnLandmarksUpdated += PoseProvider_OnLandmarksUpdated;
        }
        private void OnDisable()
        {
            if (!poseProvider.HasInterface) return;

            poseProvider.OnLandmarksUpdated -= PoseProvider_OnLandmarksUpdated;
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
        /// <summary>
        /// Editor utility to generate the body parts given the input constraints.
        /// </summary>
        /// <param name="containsConstraint">Child must contain these strings.</param>
        /// <param name="notContainsConstaint">Child must not contain these strings.</param>
        /// <returns></returns>
        private PuppetBodyPart[] GetBodyParts(string[] containsConstraint, string[] notContainsConstaint)
        {
            List<PuppetBodyPart> parts = new List<PuppetBodyPart>();
            Transform[] children = Helpers.GetAllChildrenAndNotSelf(transform);
            foreach (var c in children)
            {
                print(c.name);
                bool passesConstaint = true;
                if (containsConstraint != null)
                {
                    foreach (string s in containsConstraint)
                    {
                        if (!string.IsNullOrWhiteSpace(s) && !c.name.Contains(s))
                        {
                            passesConstaint = false;
                            break;
                        }
                    }
                }
                if (!passesConstaint) continue;
                print("Passes contains");
                if (notContainsConstaint != null)
                {
                    foreach (string s in notContainsConstaint)
                    {
                        if (!string.IsNullOrWhiteSpace(s) && c.name.Contains(s))
                        {
                            passesConstaint = false;
                            break;
                        }
                    }
                }
                if (!passesConstaint) continue;
                print("Passes not contains");

                parts.Add(new PuppetBodyPart(c.name, c, HumanBodyBones.Hips));
            }

            return parts.ToArray();
        }

        private void PoseProvider_OnLandmarksUpdated(int group)
        {
            CalculateTransforms();
            OnJointsUpdated?.Invoke(group);
            lastUpdateTime = Helpers.GetTime();
        }

        /// <summary>
        /// Calculate the transforms from the pose data.
        /// </summary>
        protected virtual void CalculateTransforms()
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

        /// <summary>
        /// Reset the puppet.
        /// </summary>
        /// <param name="pauseUpdates">Optionally pause the puppet from further updates until it's enabled again.</param>
        public void ResetPuppet(bool pauseUpdates = false)
        {
            for(int i = 0; i < joints.Count; ++i)
            {
                joints.Get(i).Reset();
            }
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
        private void CalculateElbow(HumanBodyBones bone,  PoseLandmarks shoulder, PoseLandmarks elbow, PoseLandmarks wrist, Vector3 centre, bool special = false)
        {
            Vector3 armBone = centre;
            Vector3 armForward = (Get(elbow) - Get(shoulder)).normalized;
            Vector3 armRight = (special ? -1 : 1) * CalculateNormal(Get(wrist), Get(elbow), Get(shoulder));
            Vector3 armUp = (special ? -1 : 1) * Vector3.Cross(armForward, armRight);
            DrawForward(armBone, armForward);
            DrawRight(armBone, armRight);
            DrawUp(armBone, armUp);

            SetRotation(bone, -armRight, armUp);
        }
        private void CalculateShoulder(HumanBodyBones bone,  PoseLandmarks targetShoulder, PoseLandmarks otherShoulder, PoseLandmarks targetHip, bool special)
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
        private void CalculateForearm(HumanBodyBones bone,  PoseLandmarks wrist, PoseLandmarks elbow, PoseLandmarks shoulder, bool special = false)
        {
            Vector3 forearmBone = Get(wrist);
            Vector3 forearmForward = (Get(wrist) - Get(elbow)).normalized;
            Vector3 forearmRight = (special ? -1 : 1) * CalculateNormal(Get(wrist), Get(elbow), Get(shoulder));
            Vector3 forearmUp = (special ? -1 : 1) * Vector3.Cross(forearmForward, forearmRight);
            DrawForward(forearmBone, forearmForward);
            DrawRight(forearmBone, forearmRight);
            DrawUp(forearmBone, forearmUp);

            SetRotation(bone, -forearmRight, forearmUp);
        }
        private void CalculateHand(HumanBodyBones bone,  PoseLandmarks wrist, PoseLandmarks index, PoseLandmarks pinky, bool special = false)
        {
            Vector3 handBone = Get(wrist);
            Vector3 handForward = (Get(wrist) - (Get(index) + Get(pinky)) / 2f).normalized;
            Vector3 handUp = (special ? -1 : 1) * CalculateNormal(Get(pinky), Get(index), Get(wrist));
            Vector3 handRight = (special ? -1 : 1) * Vector3.Cross(handForward, handUp);
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
            SetRotation(bone, torsoForward,torsoUp);
        }
        private void CalculateUpperLeg(HumanBodyBones bone,  PoseLandmarks rightHip, PoseLandmarks rightKnee, PoseLandmarks rightFoot, bool mirror = false)
        {
            Vector3 c = Get(rightHip);
            Vector3 forward = (Get(rightKnee) - Get(rightHip)).normalized;
            Vector3 right = CalculateNormal(Get(rightHip), Get(rightKnee), Get(rightFoot));
            Vector3 up = Vector3.Cross(forward, right);
            DrawForward(c, forward);
            DrawRight(c, right);
            DrawUp(c, up);
            SetRotation(bone, up, -forward);
        }
        private void CalculateLowerLeg(HumanBodyBones bone,  PoseLandmarks rightHip, PoseLandmarks rightKnee, PoseLandmarks rightFoot, bool mirror = false)
        {
            Vector3 c = Get(rightKnee);
            Vector3 forward = (Get(rightFoot) - Get(rightKnee)).normalized;
            Vector3 right = CalculateNormal(Get(rightHip), Get(rightKnee), Get(rightFoot));
            Vector3 up = Vector3.Cross(forward, right);
            DrawForward(c, forward);
            DrawRight(c, right);
            DrawUp(c, up);
            SetRotation(bone, up, -forward);
        }
        private void CalculateFoot(HumanBodyBones bone,  PoseLandmarks rightHip, PoseLandmarks rightKnee, PoseLandmarks rightFoot, PoseLandmarks rightToes, bool mirror = false)
        {
            Vector3 c = Get(rightKnee);
            Vector3 forward = (Get(rightToes) - Get(rightFoot)).normalized;
            Vector3 right = CalculateNormal(Get(rightHip), Get(rightKnee), Get(rightFoot));
            Vector3 up = Vector3.Cross(forward, right);
            DrawForward(c, forward);
            DrawRight(c, right);
            DrawUp(c, up);
            SetRotation(bone, forward, up);
        }

        private void SetRotation(HumanBodyBones t, Vector3 forward, Vector3 up)
        {
            puppetPartMap[t].BodyPart.rotation = Quaternion.LookRotation(forward, up);
        }
        private void SetRotation(HumanBodyBones t, Quaternion rot)
        {
            puppetPartMap[t].BodyPart.rotation = rot;
        }
        private Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 u = p2 - p1;
            Vector3 v = p3 - p1;
            Vector3 n = new Vector3((u.y * v.z - u.z * v.y), (u.z * v.x - u.x * v.z), (u.x * v.y - u.y * v.x));
            float nl = Mathf.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);
            return new Vector3(n[0] / nl, n[1] / nl, n[2] / nl);
        }

        private void DrawForward(Vector3 c, Vector3 v)
        {
            Gizmos.color = Color.blue;
            DrawRay(c, v);
        }
        private void DrawRight(Vector3 c, Vector3 v)
        {
            Gizmos.color = Color.red;
            DrawRay(c, v);
        }
        private void DrawUp(Vector3 c, Vector3 v)
        {
            Gizmos.color = Color.green;
            DrawRay(c, v);
        }
        private void DrawRay(Vector3 c, Vector3 v)
        {
            //Gizmos.DrawRay(c, v); // Replace with whatever method of visualizing axis.
        }

        protected Vector3 Get(PoseLandmarks index)
        {
            return ((IProvider<PoseLandmarks, Landmark>)poseProvider).Get(0, index).Position;
        }
        public PuppetJoint Get(int group, PoseJoints index)
        {
            return joints.Get((int)index);
        }
        public PuppetJoint Get(int group, int index)
        {
            return joints.Get(index);
        }

        /// <summary>
        /// Returns the first puppet object found.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static Puppet GetAnyPuppet(FindObjectsInactive condition = FindObjectsInactive.Exclude)
        {
            return GameObject.FindFirstObjectByType<Puppet>(condition);
        }
    }
}
