using UnityEngine;

namespace Tracking4All
{
    public class LandmarkLine : MonoBehaviour
    {
        public LineRenderer line;

        public void SetColor(Color c)
        {
            line.colorGradient = new Gradient()
            {
                alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(c.a, 0), new GradientAlphaKey(c.a, 1) },
                colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0), new GradientColorKey(c * .75f, 1) },
                colorSpace = ColorSpace.Gamma,
                mode = GradientMode.Blend
            };
        }

        public void Draw<T>(IProvider<T, Landmark> provider, int[] indices, int group)
            where T : System.Enum
        {
            line.positionCount = indices.Length;
            Landmark l;
            for (int i = 0; i < indices.Length; ++i)
            {
                l = provider.Get(group, indices[i]);
                line.SetPosition(i, l.Position);
            }
        }
    }
}