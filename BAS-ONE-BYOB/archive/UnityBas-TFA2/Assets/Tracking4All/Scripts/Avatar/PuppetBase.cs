// PuppetBase
// (C) 2024 G8gaming Ltd.
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    [DefaultExecutionOrder(-1)]
    /// <summary>
    /// The base class for a puppet.
    /// </summary>
    /// <typeparam name="INDEXER"></typeparam>
    /// <typeparam name="JOINT_DATA"></typeparam>
    public abstract class PuppetBase<INDEXER, JOINT_DATA> : MonoBehaviour
        where INDEXER : System.Enum
        where JOINT_DATA : PuppetJoint, new()
    {
        [SerializeField] protected AdapterSettingsProvider settings;
        [SerializeField] protected HumanBodyBones rootBone;
        [SerializeField] protected bool freezeRelativeOrientation = false;

        public abstract Dictionary<INDEXER, HumanBodyBones> HumanMapping { get; }
        public abstract Dictionary<HumanBodyBones, INDEXER> JointMapping { get; }

        protected abstract Table<JOINT_DATA> Joints { get; }

        protected Dictionary<HumanBodyBones, PuppetBodyPart> puppetPartMap = new Dictionary<HumanBodyBones, PuppetBodyPart>();
        protected Table<JOINT_DATA> joints = new Table<JOINT_DATA>(Helpers.GetLength(typeof(INDEXER)));
        protected virtual void SetJoint(INDEXER joint, HumanBodyBones bone)
        {
            ConstructJoint(joint, bone);
            /*joints.Get(System.Convert.ToInt32(joint))
                .Reconstruct(bone, puppetPartMap[bone].BodyPart);*/
        }
        protected abstract void ConstructJoint(INDEXER joint, HumanBodyBones bone);

        public event IProvider<INDEXER, JOINT_DATA>.GroupUpdated OnJointsUpdated;
        public int DataCount => joints.Count;
        public float LastUpdateTime => lastUpdateTime;
        protected float lastUpdateTime;

        protected bool initialized;

        protected Transform originalParent;

        protected virtual void Awake()
        {
            originalParent = transform.parent;

            List<PuppetBodyPart> range = GetPartsRange();
            foreach (var p in range)
            {
                puppetPartMap.Add(p.Correspondence, p);
            }

            foreach (var kp in HumanMapping)
            {
                SetJoint(kp.Key, kp.Value);
            }
        }
        protected abstract List<PuppetBodyPart> GetPartsRange();
        /// <summary>
        /// Editor utility to generate the body parts given the input constraints.
        /// </summary>
        /// <param name="containsConstraint">Child must contain these strings.</param>
        /// <param name="notContainsConstaint">Child must not contain these strings.</param>
        /// <returns></returns>
        protected PuppetBodyPart[] GetBodyParts(string[] containsConstraint, string[] notContainsConstaint)
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

        protected virtual void OnEnable()
        {
            HookProviders();
            /*if (!poseProvider.HasInterface)
            {
                Logger.LogWarning("Pose provider is not assigned or is not valid.", gameObject.name);
                return;
            }

            poseProvider.OnLandmarksUpdated += PoseProvider_OnLandmarksUpdated;*/

            initialized = true;
        }
        protected abstract void HookProviders();
        protected bool CanHookProvider<T>(InterfaceProvider<T> provider)
            where T : class
        {
            if (!provider.HasInterface)
            {
                Logger.LogWarning("Pose provider is not assigned or is not valid.", gameObject.name);
                return false;
            }

            return true;

            // provider.OnLandmarksUpdated += PoseProvider_OnLandmarksUpdated;
        }
        protected virtual void OnDisable()
        {
            /*if (!poseProvider.HasInterface) return;

            poseProvider.OnLandmarksUpdated -= PoseProvider_OnLandmarksUpdated;*/
            ReleaseProviders();

            initialized = false;
        }
        protected abstract void ReleaseProviders();

        protected virtual void Tick(int group)
        {
            Transform root = puppetPartMap[rootBone].BodyPart;
            Quaternion relative = root.localRotation;

            CalculateTransforms();

            if (freezeRelativeOrientation) root.localRotation = relative;

            OnJointsUpdated?.Invoke(group);
            lastUpdateTime = Helpers.GetTime();
        }
        protected abstract void CalculateTransforms();

        protected Quaternion GetRawLookRotation(Vector3 forward, Vector3 up)
        {
            return Quaternion.LookRotation(forward, up);
        }

        protected void SetRotation(HumanBodyBones t, Vector3 forward, Vector3 up)
        {
            puppetPartMap[t].BodyPart.rotation = GetRawLookRotation(forward, up);
        }
        protected void SetRotation(HumanBodyBones t, Quaternion rot)
        {
            puppetPartMap[t].BodyPart.rotation = rot;
        }
        protected Vector3 GetForward(HumanBodyBones t)
        {
            return puppetPartMap[t].BodyPart.forward;
        }
        protected Vector3 GetRight(HumanBodyBones t)
        {
            return puppetPartMap[t].BodyPart.right;
        }
        protected Vector3 GetUp(HumanBodyBones t)
        {
            return puppetPartMap[t].BodyPart.up;
        }
        protected Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 u = p2 - p1;
            Vector3 v = p3 - p1;
            Vector3 n = new Vector3((u.y * v.z - u.z * v.y), (u.z * v.x - u.x * v.z), (u.x * v.y - u.y * v.x));
            float nl = Mathf.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);
            return new Vector3(n[0] / nl, n[1] / nl, n[2] / nl);
        }

        protected void DrawForward(Vector3 c, Vector3 v)
        {
            Gizmos.color = Color.blue;
            DrawRay(c, v);
        }
        protected void DrawRight(Vector3 c, Vector3 v)
        {
            Gizmos.color = Color.red;
            DrawRay(c, v);
        }
        protected void DrawUp(Vector3 c, Vector3 v)
        {
            Gizmos.color = Color.green;
            DrawRay(c, v);
        }
        protected void DrawRay(Vector3 c, Vector3 v)
        {
            // Gizmos.DrawRay(c, v); // Replace with whatever method of visualizing axis.
        }

        /// <summary>
        /// Reset the puppet.
        /// </summary>
        /// <param name="pauseUpdates">Optionally pause the puppet from further updates until it's enabled again.</param>
        public void ResetPuppet(bool pauseUpdates = false)
        {
            for (int i = 0; i < joints.Count; ++i)
            {
                joints.Get(i).Reset();
            }
        }

        public void DisposeProviderData(int group)
        {
            // TODO: what should this do? 
        }

        /// <summary>
        /// Returns the first puppet type matching.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static PuppetBase<INDEXER, JOINT_DATA> GetPotentialPuppet(FindObjectsInactive condition = FindObjectsInactive.Exclude)
        {
            return GameObject.FindFirstObjectByType<PuppetBase<INDEXER, JOINT_DATA>>(condition);
        }
    }
}