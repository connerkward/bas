using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Must be attached to a ILandmarkProvider and will produce a visualization of the landmarks.
    /// <para>Can be used to preview non-thead safe landmark providers as well.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LandmarkVisualizer<T> : MonoBehaviour
        where T : System.Enum
    {
        [SerializeField] protected LandmarkProvider<T> provider;
        [SerializeField] protected LandmarkVisual landmarkPrefab;
        [SerializeField] protected LandmarkLine landmarkEdgePrefab;
        [SerializeField] protected Color lineColor = Color.blue;
        [SerializeField] protected float landmarkVisualScaleMultiplier = 1;

        private static int Count = System.Enum.GetValues(typeof(T)).Length;

        public abstract int Group { get; }
        public abstract List<int[]> EdgeDefinitions { get; }
        public abstract HashSet<int> IgnoreIndicies { get; }

        protected bool providerDirty;
        protected LandmarkVisual[] points;
        protected LandmarkLine[] lines;

        protected virtual void Start()
        {
            provider.OnLandmarksUpdated += Provider_OnLandmarksUpdated;

            string[] names = System.Enum.GetNames(typeof(T));
            points = new LandmarkVisual[names.Length];
            Transform parent = new GameObject(transform.name + " Visualizer").transform;
            parent.SetParent(Tracking4All.Instance.GENERATED.transform, true);

            for (int i = 0; i < points.Length; ++i)
            {
                if (IgnoreIndicies.Contains(i)) continue;
                points[i] = CreateLandmark(names[i], parent);
                points[i].transform.localScale *= landmarkVisualScaleMultiplier;
            }

            lines = new LandmarkLine[EdgeDefinitions.Count];
            for (int i = 0; i < lines.Length; ++i)
            {
                lines[i] = CreateEdge(EdgeDefinitions[i].Length.ToString(), parent);
                lines[i].SetColor(lineColor);
            }
        }

        protected virtual void LateUpdate()
        {
            for (int i = 0; i < points.Length; ++i)
            {
                if (IgnoreIndicies.Contains(i)) continue;
                points[i].UpdateLandmark(provider.Get(Group, i));
            }

            for (int i = 0; i < lines.Length; ++i)
            {
                lines[i].Draw(provider, EdgeDefinitions[i], Group);
            }

            providerDirty = false;
        }

        private void Provider_OnLandmarksUpdated(int group)
        {
            providerDirty = true;
        }

        protected virtual LandmarkVisual CreateLandmark(string n, Transform parent)
        {
            LandmarkVisual v = Instantiate(landmarkPrefab.gameObject, parent).GetComponent<LandmarkVisual>();
            v.gameObject.name = n;
            return v;
        }

        protected virtual LandmarkLine CreateEdge(string n, Transform parent)
        {
            LandmarkLine l = Instantiate(landmarkEdgePrefab.gameObject, parent).GetComponent<LandmarkLine>();
            l.gameObject.name = n;
            return l;
        }
    }
}