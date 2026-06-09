using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    public class HandLandmarkVisualizer : LandmarkVisualizer<HandLandmarks>
    {
        [SerializeField] protected Handedness handedness;
        [SerializeField] private GameObject rHandednessPrefab;
        [SerializeField] private GameObject lHandednessPrefab;

        private static readonly List<int[]> EDGE_DEFINITIONS = new List<int[]>()
        {
            new int[] { 0, 5, 9, 13, 17, 0 },
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 5, 6, 7, 8 },
            new int[] { 9, 10, 11, 12 },
            new int[] { 13, 14, 15, 16 },
            new int[] { 17, 18, 19, 20 }
        };

        public override List<int[]> EdgeDefinitions => EDGE_DEFINITIONS;

        private static readonly HashSet<int> IGNORE_INDICES = new HashSet<int>(
            new int[]
            {
                // 1,2,3,4,5,6
            });

        public override HashSet<int> IgnoreIndicies => IGNORE_INDICES;

        public override int Group => (int)handedness;

        private Transform handednessIndicator;

        protected override void Start()
        {
            base.Start();

            if (lHandednessPrefab && rHandednessPrefab)
            {
                GameObject hand = null;
                switch (handedness)
                {
                    case Handedness.LEFT:
                        hand = Instantiate(lHandednessPrefab, Tracking4All.Instance.GENERATED.transform, true);
                        break;
                    case Handedness.RIGHT:
                        hand = Instantiate(rHandednessPrefab, Tracking4All.Instance.GENERATED.transform, true);
                        break;
                }
                handednessIndicator = hand.transform;
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (handednessIndicator)
                handednessIndicator.transform.position =
                    (Get(HandLandmarks.WRIST) + Get(HandLandmarks.INDEX1) + Get(HandLandmarks.PINKY1)) / 3f;
        }

        private Vector3 Get(HandLandmarks l)
        {
            return provider.Get((int)handedness, l).Position;
        }

        private void OnValidate()
        {
            if (visualizationMode != LandmarkVisualizationMode.Normal)
            {
                Debug.LogError("Hand solution does not currently support non-normal visualization. The presense and visibility values will always be 0.");
                visualizationMode = LandmarkVisualizationMode.Normal;
            }
        }
    }
}