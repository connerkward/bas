// HolisticHandsModifier
// (C) 2024 G8gaming Ltd.
using System;
using UnityEngine;

namespace Tracking4All
{
    public class HolisticHandsModifier : Modifier,
        ILandmarkModifier
    {
        [SerializeField] protected Handedness handedness;
        [Header("Providers")]
        [SerializeField] protected AdapterSettingsProvider adapterSettings;
        [SerializeField] protected LandmarkProvider<PoseLandmarks> poseProvider;
        [Header("Source Providers")] // the providers that are providing to the above (i.e. the 'raw data').
        [SerializeField] protected LandmarkProvider<HandLandmarks> handSourceProvider;
        [SerializeField] protected LandmarkProvider<PoseLandmarks> poseSourceProvider;
        [Header("Smoothing")]
        [SerializeField] protected float fingerSmoothFrequency = 40;
        [SerializeField] protected float handOrientationFrequency = 40;
        // set the below higher to be more visually appealing in poor conditions (recommend: .4 - .7)
        [SerializeField] protected float minimumPoseBlend = 0.7f;

        private Handedness currentHandedness
        {
            get
            {
                // if (adapterSettings.Mirror) return handedness.Flip();
                return handedness;
            }
        }
        private int HandednessInt => (int)currentHandedness;

        private const int TRACKING_DESYNC_THRESHOLD = 3;

        private Vector3 handForward, handUp, handRight;
        private Quaternion handWorldRotation;
        private Quaternion lastHandWorldRotation;

        private Vector3 poseForward, poseUp;
        private Quaternion poseWorldRotation;

        private OneEuroFilterVector3[] filters = new OneEuroFilterVector3[Helpers.GetLength(typeof(HandLandmarks))];
        private Vector3[] dataPositionTargets = new Vector3[Helpers.GetLength(typeof(HandLandmarks))];

        [SerializeField] private OneEuroFilterQuaternion rotationFilter = new OneEuroFilterQuaternion(240);
        [SerializeField] private OneEuroFilterQuaternion handWorldRotFilter = new OneEuroFilterQuaternion(30);

        private float blendToBody = 0f;
        private Quaternion handRelativeRotation = Quaternion.identity;
        private Quaternion handOrientation;

        private bool isHandLost = false;
        private int pCount, hCount;

        private bool ready;

        private void Awake()
        {
            if (!adapterSettings.HasInterface)
            {
                Logger.LogError("The adapter settings must be set.", gameObject.name);
            }

            for (int i = 0; i < filters.Length; i++)
            {
                filters[i] = new OneEuroFilterVector3(fingerSmoothFrequency);
            }
        }
        private void OnEnable()
        {
            poseSourceProvider.OnLandmarksUpdated += PoseSourceProvider_OnLandmarksUpdated;
            handSourceProvider.OnLandmarksUpdated += HandSourceProvider_OnLandmarksUpdated;
            ready = true;
        }
        private void OnDisable()
        {
            poseSourceProvider.OnLandmarksUpdated -= PoseSourceProvider_OnLandmarksUpdated;
            handSourceProvider.OnLandmarksUpdated -= HandSourceProvider_OnLandmarksUpdated;
            ready = false;
        }

        private void PoseSourceProvider_OnLandmarksUpdated(int group)
        {
            if (!ready || Enabled) return;

            ++pCount;

            if (Mathf.Abs(pCount - hCount) >= TRACKING_DESYNC_THRESHOLD)
            {
                isHandLost = true;
            }
        }
        private void HandSourceProvider_OnLandmarksUpdated(int group)
        {
            if (!ready || Enabled) return;

            if (group != HandednessInt) return;

            if (Mathf.Abs(pCount - hCount) >= TRACKING_DESYNC_THRESHOLD)
            {
                isHandLost = false;
                pCount = 0;
                hCount = 0;
            }

            ++hCount;
        }

        public override void PreCalculate(float deltaTime, int dataCount)
        {
            base.PreCalculate(deltaTime, dataCount);

            // Calculate alignment for hand.
            CalculateHandDirections();
            handWorldRotation = handWorldRotFilter.Filter(Quaternion.LookRotation(handUp, handForward), deltaTime);

            // Calculate alignment for pose.
            Handedness h = currentHandedness;
            if (adapterSettings.Mirror) h = h.Flip();
            switch (h)
            {
                case Handedness.RIGHT:
                    CalculatePoseDirections(PoseLandmarks.RIGHT_WRIST, PoseLandmarks.RIGHT_ELBOW, PoseLandmarks.RIGHT_SHOULDER);
                    break;

                case Handedness.LEFT:
                    CalculatePoseDirections(PoseLandmarks.LEFT_WRIST, PoseLandmarks.LEFT_ELBOW, PoseLandmarks.LEFT_SHOULDER);
                    break;
            }
            poseWorldRotation = Quaternion.LookRotation(-poseUp, poseForward);

            // Calculate blend factor.
            Vector3 bodyDown = (
                (GetPose(PoseLandmarks.RIGHT_HIP) + GetPose(PoseLandmarks.LEFT_HIP)) / 2f
                - (GetPose(PoseLandmarks.RIGHT_SHOULDER) + GetPose(PoseLandmarks.LEFT_SHOULDER)) / 2f).normalized;
            float d = Vector3.Dot(bodyDown, poseForward);
            float target = Mathf.InverseLerp(-0.1f, 0.7f, d);
            if (d < -.1f) target = 0;
            target = Mathf.Lerp(minimumPoseBlend, 1, target);
            blendToBody = Mathf.Lerp(blendToBody, target, deltaTime * 20f);

            // Rules for hand rotation behavior.
            if (isHandLost)
            {
                // Align to pose.
                handRelativeRotation =
                    Quaternion.LookRotation(Vector3.Cross(handRight, poseForward), poseForward)
                    * Quaternion.Inverse(poseWorldRotation);
            }
            else if (Quaternion.Angle(handWorldRotation, lastHandWorldRotation) > 0.01f)
            {
                // Only update on changes to allow relative movement.
                handRelativeRotation = handWorldRotation * Quaternion.Inverse(poseWorldRotation);
            }

            rotationFilter.UpdateParams(handOrientationFrequency);
            handRelativeRotation = rotationFilter.Filter(handRelativeRotation, deltaTime);

            // Final rotation for hand, blended between the hand infered and body infered orientation.
            handOrientation = Quaternion.Lerp(handRelativeRotation * poseWorldRotation, poseWorldRotation, blendToBody);
        }
        public void Modify(int dataIndex, ref Landmark current, ref Landmark target, ref bool stayAlive, float deltaTime)
        {
            // Align to pose world wrist.
            Vector3 invariantPoint =
                Quaternion.Inverse(handWorldRotation)
                * (target.Position - GetHandSource(HandLandmarks.WRIST));

            // invariantPoint = Vector3.Slerp(dataPositionTargets[dataIndex],invariantPoint, Time.deltaTime*16f);

            // Smooth fingers.
            filters[dataIndex].UpdateParams(fingerSmoothFrequency);
            invariantPoint = filters[dataIndex].Filter(invariantPoint, deltaTime);

            dataPositionTargets[dataIndex] = invariantPoint;

            current.Position = (handOrientation * invariantPoint) + GetPoseWrist();

            // Just keep hands alive for now.
            // TODO: sync issues between pose and hand, probably should make a holistic solution instead...
            stayAlive = true;
        }
        public override void PostCalculate(float deltaTime)
        {
            base.PostCalculate(deltaTime);

            lastHandWorldRotation = handWorldRotation;
        }

        private void CalculatePoseDirections(PoseLandmarks wrist, PoseLandmarks elbow, PoseLandmarks shoulder)
        {
            poseForward = (GetPose(wrist) - GetPose(elbow)).normalized;
            poseUp = CalculateNormal(
                GetPose(shoulder),
                GetPose(elbow),
                GetPose(wrist));
        }
        private void CalculateHandDirections()
        {
            Vector3 forward = (GetHandSource(HandLandmarks.INDEX1) + GetHandSource(HandLandmarks.PINKY1)) / 2f;
            handForward = (forward - GetHandSource(HandLandmarks.WRIST)).normalized;
            Vector3 up = CalculateNormal(
                GetHandSource(HandLandmarks.WRIST),
                GetHandSource(HandLandmarks.INDEX2),
                GetHandSource(HandLandmarks.RING2));
            handUp = (up - GetHandSource(HandLandmarks.WRIST));
            handRight = Vector3.Cross(handForward, handUp);

            // if (handedness == Handedness.LEFT) handUp *= -1;
        }

        protected Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 u = p2 - p1;
            Vector3 v = p3 - p1;
            Vector3 n = new Vector3((u.y * v.z - u.z * v.y), (u.z * v.x - u.x * v.z), (u.x * v.y - u.y * v.x));
            float nl = Mathf.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);
            return new Vector3(n[0] / nl, n[1] / nl, n[2] / nl);
        }

        private Vector3 GetHandSource(HandLandmarks l)
        {
            return handSourceProvider.Get(HandednessInt, l).Position;
        }
        private Vector3 GetPoseWrist()
        {
            Handedness h = handedness;
            if (adapterSettings.Mirror) h = h.Flip();

            switch (h)
            {
                case Handedness.LEFT:
                    return poseProvider.Get(0, PoseLandmarks.LEFT_WRIST).Position;
                case Handedness.RIGHT:
                    return poseProvider.Get(0, PoseLandmarks.RIGHT_WRIST).Position;
            }

            throw new NotImplementedException();
        }
        private Vector3 GetPose(PoseLandmarks l)
        {
            return poseProvider.Get(0, l).Position;
        }
    }
}