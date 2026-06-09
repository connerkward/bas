using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    public class PoseLandmarkVisualizer : LandmarkVisualizer<PoseLandmarks>
    {
        [SerializeField] private GameObject optionalHeadPrefab;
        [SerializeField] private Transform optionalGroundTransform;
        [SerializeField] private bool hideHandLandmarks = false;

        public override int Group => 0;

        private static readonly List<int[]> EDGE_DEFINITIONS = new List<int[]>()
        {
            new int[] { 12, 24, 23, 11, 12 },
            new int[] { 12, 14, 16 },
            new int[] { 11, 13, 15 },
            new int[] { 23, 25, 27, 29, 31, 27 },
            new int[] { 24, 26, 28, 30, 32, 28 },
        };

        public override List<int[]> EdgeDefinitions => EDGE_DEFINITIONS;

        private static readonly HashSet<int> IGNORE_INDICES_NORMAL = new HashSet<int>(
            new int[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            });
        private static readonly HashSet<int> IGNORE_INDICES_HIDE_HANDS = new HashSet<int>(
            new int[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                15,17,19, 21,
                16,18,20,22
            });

        public override HashSet<int> IgnoreIndicies => hideHandLandmarks ? IGNORE_INDICES_HIDE_HANDS : IGNORE_INDICES_NORMAL;

        private GameObject headInstance;

        protected override void Start()
        {
            base.Start();
            if (optionalHeadPrefab) headInstance = Instantiate(optionalHeadPrefab, Tracking4All.Instance.GENERATED.transform, true);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (provider.IsAlive)
            {
                if (optionalHeadPrefab)
                {
                    headInstance.transform.position =
                        (GetPoint(PoseLandmarks.LEFT_EAR) + GetPoint(PoseLandmarks.RIGHT_EAR)) / 2f;
                    Vector3 up = Helpers.CalculateNormal(GetPoint(PoseLandmarks.NOSE), GetPoint(PoseLandmarks.LEFT_EAR),
                        GetPoint(PoseLandmarks.RIGHT_EAR));
                    Vector3 forward = Quaternion.AngleAxis(90,
                        (GetPoint(PoseLandmarks.LEFT_EAR) - GetPoint(PoseLandmarks.RIGHT_EAR)).normalized) * up;
                    headInstance.transform.rotation = Quaternion.LookRotation(forward, up);
                }
            }

            if (optionalGroundTransform)
            {
                float minY = Mathf.Infinity;
                for (int i = 0; i < provider.Provider.DataCount; ++i)
                {
                    minY = Mathf.Min(minY, provider.Provider.Get(0, i).Position.y);
                }
                Vector3 pos = optionalGroundTransform.transform.position;
                pos.y = minY - .2f;
                optionalGroundTransform.transform.position = Vector3.Lerp(optionalGroundTransform.transform.position,
                    pos, Time.deltaTime * 1f);
            }
        }

        private Vector3 GetPoint(PoseLandmarks l)
        {
            return provider.Provider.Get(0, l).Position;
        }
    }
}