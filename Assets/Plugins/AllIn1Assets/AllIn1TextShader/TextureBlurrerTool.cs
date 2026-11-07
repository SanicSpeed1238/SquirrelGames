using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	public class TextureBlurrerTool
	{
#if UNITY_EDITOR
		private const string ComputeAssetPath = "Assets/Plugins/AllIn1Assets/AllIn1TextShader/Shaders/AllIn1TextBlur.compute";
		private ComputeShader _computeShader;
		private bool _computeLoadAttempted;
#endif

		public Texture2D FastBlur(Texture2D image, int radius, int iterations, TextureFormat textureFormat)
		{
			if (image == null || radius < 1 || iterations < 1) return image;
			radius = Mathf.Clamp(radius, 1, 64);

#if UNITY_EDITOR
			if (ShouldUseCompute())
			{
				try
				{
					return FastBlurGPU(image, radius, iterations, textureFormat);
				}
				catch (Exception e)
				{
					Debug.LogWarning($"[TextureBlurrerTool] Compute path failed, falling back to CPU: {e.Message}");
				}
			}
#endif
			return FastBlurCPU(image, radius, iterations, textureFormat);
		}

#if UNITY_EDITOR
		private bool ShouldUseCompute()
		{
			if (!SystemInfo.supportsComputeShaders) return false;
			if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32)) return false;
			return LoadComputeAsset() != null;
		}

		private ComputeShader LoadComputeAsset()
		{
			if (_computeLoadAttempted) return _computeShader;
			_computeLoadAttempted = true;
			_computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(ComputeAssetPath);
			if (_computeShader == null)
			{
				Debug.LogWarning($"[TextureBlurrerTool] Compute shader not found at {ComputeAssetPath}. Falling back to CPU path.");
			}
			return _computeShader;
		}

		private Texture2D FastBlurGPU(Texture2D image, int radius, int iterations, TextureFormat textureFormat)
		{
			ComputeShader cs = LoadComputeAsset();
			int w = image.width;
			int h = image.height;

			float[] weights = BuildGaussianWeights(radius);

			ComputeBuffer weightsBuffer = null;
			RenderTexture rtA = null;
			RenderTexture rtB = null;
			RenderTexture prevActive = RenderTexture.active;

			try
			{
				weightsBuffer = new ComputeBuffer(weights.Length, sizeof(float));
				weightsBuffer.SetData(weights);

				rtA = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32) { enableRandomWrite = true };
				rtB = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32) { enableRandomWrite = true };
				rtA.Create();
				rtB.Create();

				int hKernel = cs.FindKernel("BlurHorizontal");
				int vKernel = cs.FindKernel("BlurVertical");

				cs.SetInt("Width", w);
				cs.SetInt("Height", h);
				cs.SetInt("Radius", radius);

				int groupsX = Mathf.CeilToInt(w / 32f);
				int groupsY = Mathf.CeilToInt(h / 32f);

				Texture currentSource = image;

				for (int i = 0; i < iterations; ++i)
				{
					cs.SetTexture(hKernel, "InputTex", currentSource);
					cs.SetTexture(hKernel, "OutputTex", rtA);
					cs.SetBuffer(hKernel, "Weights", weightsBuffer);
					cs.Dispatch(hKernel, groupsX, h, 1);

					cs.SetTexture(vKernel, "InputTex", rtA);
					cs.SetTexture(vKernel, "OutputTex", rtB);
					cs.SetBuffer(vKernel, "Weights", weightsBuffer);
					cs.Dispatch(vKernel, w, groupsY, 1);

					currentSource = rtB;
				}

				Texture2D result = new Texture2D(w, h, textureFormat, false);
				RenderTexture.active = rtB;
				result.ReadPixels(new Rect(0, 0, w, h), 0, 0);
				result.Apply();
				return result;
			}
			finally
			{
				RenderTexture.active = prevActive;
				weightsBuffer?.Release();
				if (rtA != null) rtA.Release();
				if (rtB != null) rtB.Release();
			}
		}
#endif

		private Texture2D FastBlurCPU(Texture2D image, int radius, int iterations, TextureFormat textureFormat)
		{
			int w = image.width;
			int h = image.height;

			byte[] src;
			try
			{
				src = image.GetRawTextureData();
			}
			catch (Exception e)
			{
				Debug.LogError($"[TextureBlurrerTool] CPU fallback could not read source texture (is it Read/Write enabled?): {e.Message}");
				return image;
			}

			if (src == null || src.Length != w * h)
			{
				Debug.LogError($"[TextureBlurrerTool] CPU fallback expects single-channel Alpha8 source (1 byte/pixel); got {(src?.Length ?? 0)} bytes for {w}x{h}.");
				return image;
			}

			float[] weights = BuildGaussianWeights(radius);
			byte[] bufA = src;
			byte[] bufB = new byte[src.Length];

			for (int it = 0; it < iterations; ++it)
			{
				BlurHorizontalCPU(bufA, bufB, w, h, radius, weights);
				BlurVerticalCPU(bufB, bufA, w, h, radius, weights);
			}

			Texture2D result = new Texture2D(w, h, textureFormat, false);
			result.SetPixelData(bufA, 0);
			result.Apply();
			return result;
		}

		private static void BlurHorizontalCPU(byte[] src, byte[] dst, int w, int h, int radius, float[] weights)
		{
			for (int y = 0; y < h; ++y)
			{
				int row = y * w;
				for (int x = 0; x < w; ++x)
				{
					float sum = 0f;
					for (int t = -radius; t <= radius; ++t)
					{
						int sx = x + t;
						if (sx < 0) sx = 0;
						else if (sx >= w) sx = w - 1;
						sum += src[row + sx] * weights[t + radius];
					}
					int v = (int)(sum + 0.5f);
					if (v < 0) v = 0;
					else if (v > 255) v = 255;
					dst[row + x] = (byte)v;
				}
			}
		}

		private static void BlurVerticalCPU(byte[] src, byte[] dst, int w, int h, int radius, float[] weights)
		{
			for (int x = 0; x < w; ++x)
			{
				for (int y = 0; y < h; ++y)
				{
					float sum = 0f;
					for (int t = -radius; t <= radius; ++t)
					{
						int sy = y + t;
						if (sy < 0) sy = 0;
						else if (sy >= h) sy = h - 1;
						sum += src[sy * w + x] * weights[t + radius];
					}
					int v = (int)(sum + 0.5f);
					if (v < 0) v = 0;
					else if (v > 255) v = 255;
					dst[y * w + x] = (byte)v;
				}
			}
		}

		private static float[] BuildGaussianWeights(int radius)
		{
			int count = 2 * radius + 1;
			float[] w = new float[count];
			float sigma = Mathf.Max(0.0001f, radius * 0.5f);
			float twoSigmaSq = 2f * sigma * sigma;
			float sum = 0f;
			for (int i = 0; i < count; ++i)
			{
				int t = i - radius;
				w[i] = Mathf.Exp(-(t * t) / twoSigmaSq);
				sum += w[i];
			}
			float inv = 1f / sum;
			for (int i = 0; i < count; ++i) w[i] *= inv;
			return w;
		}
	}
}
