using UnityEngine;

namespace Tracking4All
{
    public class LandmarkLine : MonoBehaviour
    {
        public LineRenderer line;

        private GradientAlphaKey[] gradientAlphas;
        private GradientColorKey[] gradientColors;


        public void SetNewColor(Color c, int[] indices)
        {
            gradientAlphas = new GradientAlphaKey[indices.Length];
            gradientColors = new GradientColorKey[indices.Length];
            for (int i = 0; i < indices.Length; ++i)
            {
                float t = (float)i / (float)indices.Length;
                gradientAlphas[i] = new GradientAlphaKey(c.a, t);
                gradientColors[i] = new GradientColorKey(c, t);
            }
            line.colorGradient = new Gradient()
            {
                alphaKeys = gradientAlphas,
                colorKeys = gradientColors,
                colorSpace = ColorSpace.Gamma,
                mode = GradientMode.Blend
            };

            /*line.colorGradient = new Gradient()
            {
                alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(c.a, 0), new GradientAlphaKey(c.a, 1) },
                colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0), new GradientColorKey(c * .75f, 1) },
                colorSpace = ColorSpace.Gamma,
                mode = GradientMode.Blend
            };*/
        }

        public void Draw<T>(LandmarkVisualizer<T> visualizer, int[] indices, int group)
            where T : System.Enum
        {
            line.positionCount = indices.Length;
            Landmark l;
            Gradient color = line.colorGradient;

            for (int i = 0; i < indices.Length; ++i)
            {
                l = visualizer.GetLandmark(group, indices[i]);
                line.SetPosition(i, l.Position);
                gradientColors[i].color = visualizer.GetVisualizedColor(l);
            }

            color.alphaKeys = gradientAlphas;
            color.colorKeys = gradientColors;

            line.colorGradient = color;
        }
    }
}