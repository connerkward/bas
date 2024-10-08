// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
  using Color = UnityEngine.Color;
#pragma warning restore IDE0065

  public class MaskOverlayAnnotation : HierarchicalAnnotation
  {
    [SerializeField] private Shader _maskShader;
    [SerializeField] private Texture2D _maskTexture;
    [SerializeField] private Color _color = Color.blue;
    [SerializeField, Range(0, 1)] private float _threshold = 0.9f;

    private GameObject _screenObject;
    private RawImage _screen;
    private Material _prevMaterial;
    private Material _material;
    private GraphicsBuffer _maskBuffer;
    private float[] _maskArray;

    private void OnEnable()
    {
      ApplyMaskTexture(_maskTexture, _color);
      ApplyThreshold(_threshold);
      if (_screenObject != null)
      {
        _screenObject.SetActive(true);
      }
    }

    private void OnDisable()
    {
      if (_prevMaterial != null)
      {
        ApplyMaterial(_prevMaterial);
      }
      if (_screenObject != null)
      {
        _screenObject.SetActive(false);
      }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
      if (!UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this))
      {
        ApplyMaskTexture(_maskTexture, _color);
        ApplyThreshold(_threshold);
      }
    }
#endif

    private void OnDestroy()
    {
      if (_maskBuffer != null)
      {
        _maskBuffer.Release();
      }
      _maskArray = null;
      _prevMaterial = null;
      _material = null;
      Destroy(_screenObject);
    }

    public void Init(RawImage screen, int width, int height)
    {
      // copy the target screen to overlay mask.
      _screenObject = new GameObject("Mask Screen");
      _screenObject.transform.SetParent(screen.transform, false);
      _screen = _screenObject.AddComponent<RawImage>();
      _screen.rectTransform.sizeDelta = screen.rectTransform.sizeDelta;
      _screen.color = new Color(1, 1, 1, 1);

      _material = new Material(_maskShader)
      {
        renderQueue = (int)RenderQueue.Transparent
      };

      _material.SetTexture("_MainTex", _screen.texture);
      ApplyMaskTexture(_maskTexture, _color);
      _material.SetInt("_Width", width);
      _material.SetInt("_Height", height);
      ApplyThreshold(_threshold);
      InitMaskBuffer(width, height);
    }

    public void SetMaskTexture(Texture2D maskTexture, Color color)
    {
      _maskTexture = maskTexture;
      _color = color;
      ApplyMaskTexture(_maskTexture, _color);
    }

    public void SetThreshold(float threshold)
    {
      _threshold = threshold;
      ApplyThreshold(_threshold);
    }

    public void Read(ImageFrame imageFrame, bool isMirrored = false)
    {
      if (imageFrame != null)
      {
        // NOTE: assume that the image is transformed properly by calculators.
        var _ = imageFrame.TryReadChannelNormalized(0, _maskArray, isMirrored);
      }
    }

    public void Read(Image image, bool isMirrored = false)
    {
      if (image != null)
      {
        // NOTE: assume that the image is transformed properly by calculators.
        var _ = image.TryReadChannelNormalized(0, _maskArray, isMirrored);
      }
    }

    public void Clear() => ApplyMaterial(_prevMaterial);

    public void Draw(ImageFrame imageFrame, bool isMirrored = false)
    {
      Read(imageFrame, isMirrored);
      Draw();
    }

    public void Draw(Image image, bool isMirrored = false)
    {
      Read(image, isMirrored);
      Draw();
    }

    public void Draw()
    {
      ApplyMaterial(_material);
      _maskBuffer.SetData(_maskArray);
    }

    private Texture2D CreateMonoColorTexture(Color color)
    {
      var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
      var textureColor = new Color32((byte)(255 * color.r), (byte)(255 * color.g), (byte)(255 * color.b), (byte)(255 * color.a));
      texture.SetPixels32(new Color32[] { textureColor });
      texture.Apply();

      return texture;
    }

    private void InitMaskBuffer(int width, int height)
    {
      if (_maskBuffer != null)
      {
        _maskBuffer.Release();
      }
      var stride = Marshal.SizeOf(typeof(float));
      _maskBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, width * height, stride);
      _material.SetBuffer("_MaskBuffer", _maskBuffer);

      _maskArray = new float[width * height];
    }

    private void ApplyMaterial(Material material)
    {
      if (_prevMaterial == null)
      {
        // backup
        _prevMaterial = _screen.material;
      }
      if (_screen.material != material)
      {
        _screen.material = material;
      }
    }

    private void ApplyMaskTexture(Texture maskTexture, Color maskColor)
    {
      if (_material != null)
      {
        _material.SetTexture("_MaskTex", maskTexture == null ? CreateMonoColorTexture(maskColor) : maskTexture);
      }
    }

    private void ApplyThreshold(float threshold)
    {
      if (_material != null)
      {
        _material.SetFloat("_Threshold", threshold);
      }
    }
  }
}
