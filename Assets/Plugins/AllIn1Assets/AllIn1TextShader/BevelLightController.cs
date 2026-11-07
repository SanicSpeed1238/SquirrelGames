using System.Collections.Generic;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public enum BevelBlending { Multiply, Additive, Replace }

	[ExecuteAlways]
	public class BevelLightController : MonoBehaviour
	{
		private static readonly int PROP_ID_GLOBAL_BEVEL_LIGHT = Shader.PropertyToID("global_BevelLight");
		private static readonly int PROP_ID_GLOBAL_BEVEL_LIGHT_COLOR = Shader.PropertyToID("global_BevelLightColor");
		private static readonly int PROP_ID_GLOBAL_BEVEL_LIGHT_RAMPS = Shader.PropertyToID("global_BevelLightRamps");
		private static readonly int PROP_ID_GLOBAL_BEVEL_SHADOW_COLOR = Shader.PropertyToID("global_BevelShadowColor");
		private static readonly int PROP_ID_GLOBAL_BEVEL_LIGHT_BLEND = Shader.PropertyToID("global_BevelLightBlend");
		private static readonly int PROP_ID_GLOBAL_EXTRUDE_LIGHT_COLOR = Shader.PropertyToID("global_ExtrudeLightColor");
		private static readonly int PROP_ID_GLOBAL_EXTRUDE_SHADOW_COLOR = Shader.PropertyToID("global_ExtrudeShadowColor");
		private static readonly int PROP_ID_GLOBAL_EXTRUDE_LIGHT_RAMP = Shader.PropertyToID("global_ExtrudeLightRamp");

		//Match the property defaults in AllIn1Text.shader
		public const float DEFAULT_LIGHT_ANGLE = 300f;
		public const float DEFAULT_LIGHT_ELEVATION = 0f;
		public const BevelBlending DEFAULT_LIGHT_BLENDING = BevelBlending.Additive;
		public static readonly Color DEFAULT_LIGHT_COLOR = new Color(0.576f, 0.576f, 0.576f, 1f);
		public const float DEFAULT_LIGHT_CUTOFF = 0.5f;
		public const float DEFAULT_LIGHT_SMOOTHNESS = 0.75f;
		public const float DEFAULT_LIGHT_INTENSITY = 1.5f;
		public const BevelBlending DEFAULT_SHADOW_BLENDING = BevelBlending.Multiply;
		public static readonly Color DEFAULT_SHADOW_COLOR = new Color(0.25f, 0.25f, 0.25f, 1f);
		public const float DEFAULT_SHADOW_CUTOFF = 0f;
		public const float DEFAULT_SHADOW_SMOOTHNESS = 0.6f;
		public static readonly Color DEFAULT_EXTRUDE_LIGHT_COLOR = new Color(1f, 1f, 1f, 1f);
		public static readonly Color DEFAULT_EXTRUDE_SHADOW_COLOR = new Color(0.4f, 0.4f, 0.5f, 1f);
		public const float DEFAULT_EXTRUDE_LIGHT_SMOOTHNESS = 1f;

		// The last one enabled or edited drives the shader; the others wait until it is disabled
		private static readonly List<BevelLightController> enabledControllers = new List<BevelLightController>();
		public static BevelLightController Active { get; private set; }

		[Range(0f, 360f)] public float lightAngle = DEFAULT_LIGHT_ANGLE;
		[Range(0f, 90f)] public float lightElevation = DEFAULT_LIGHT_ELEVATION;

		public BevelBlending lightBlending = DEFAULT_LIGHT_BLENDING;
		public Color lightColor = DEFAULT_LIGHT_COLOR;
		[Range(0f, 1f)] public float lightCutoff = DEFAULT_LIGHT_CUTOFF;
		[Range(0f, 1f)] public float lightSmoothness = DEFAULT_LIGHT_SMOOTHNESS;
		[Range(0f, 4f)] public float lightIntensity = DEFAULT_LIGHT_INTENSITY;

		public BevelBlending shadowBlending = DEFAULT_SHADOW_BLENDING;
		public Color shadowColor = DEFAULT_SHADOW_COLOR;
		[Range(0f, 1f)] public float shadowCutoff = DEFAULT_SHADOW_CUTOFF;
		[Range(0f, 1f)] public float shadowSmoothness = DEFAULT_SHADOW_SMOOTHNESS;

		//Extrude's sides take the light direction above and these colours
		public Color extrudeLightColor = DEFAULT_EXTRUDE_LIGHT_COLOR;
		public Color extrudeShadowColor = DEFAULT_EXTRUDE_SHADOW_COLOR;
		[Range(0f, 1f)] public float extrudeLightSmoothness = DEFAULT_EXTRUDE_LIGHT_SMOOTHNESS;

		private void OnEnable()
		{
			if (!enabledControllers.Contains(this)) { enabledControllers.Add(this); }
			Active = this;
			ApplyGlobals();
		}

		//Editing a controller makes it the active one, so the one being edited is always the one the shader shows
		private void OnValidate()
		{
			if (isActiveAndEnabled)
			{
				Active = this;
				ApplyGlobals();
			}
		}

		private void Update()
		{
			if (Active == this) { ApplyGlobals(); }
		}

		private void OnDisable()
		{
			enabledControllers.Remove(this);
			if (Active != this) { return; }

			Active = enabledControllers.Count > 0 ? enabledControllers[enabledControllers.Count - 1] : null;
			if (Active != null)
			{
				Active.ApplyGlobals();
			}
			else
			{
				Shader.SetGlobalVector(PROP_ID_GLOBAL_BEVEL_LIGHT, Vector4.zero);
			}
		}

		private void ApplyGlobals()
		{
			Shader.SetGlobalVector(PROP_ID_GLOBAL_BEVEL_LIGHT, new Vector4(lightAngle * Mathf.Deg2Rad, lightElevation * Mathf.Deg2Rad, 1f, 0f));

			//The intensity multiplies after the conversion, as _LightIntensity does in the shader
			Vector4 bevelLight = ToShaderColor(lightColor);
			Shader.SetGlobalVector(PROP_ID_GLOBAL_BEVEL_LIGHT_COLOR, new Vector4(bevelLight.x * lightIntensity, bevelLight.y * lightIntensity, bevelLight.z * lightIntensity, bevelLight.w));
			Shader.SetGlobalVector(PROP_ID_GLOBAL_BEVEL_LIGHT_RAMPS, new Vector4(lightCutoff, lightSmoothness, shadowCutoff, shadowSmoothness));
			Shader.SetGlobalVector(PROP_ID_GLOBAL_BEVEL_SHADOW_COLOR, ToShaderColor(shadowColor));
			Shader.SetGlobalVector(PROP_ID_GLOBAL_BEVEL_LIGHT_BLEND, new Vector4((float)lightBlending, (float)shadowBlending, 0f, 0f));
			Shader.SetGlobalVector(PROP_ID_GLOBAL_EXTRUDE_LIGHT_COLOR, ToShaderColor(extrudeLightColor));
			Shader.SetGlobalVector(PROP_ID_GLOBAL_EXTRUDE_SHADOW_COLOR, ToShaderColor(extrudeShadowColor));
			Shader.SetGlobalVector(PROP_ID_GLOBAL_EXTRUDE_LIGHT_RAMP, new Vector4(extrudeLightSmoothness, 0f, 0f, 0f));
		}

		//Material colours reach the shader in linear in a Linear project, and a global vector is sent as is, so Global matches the same values set on a material
		private static Vector4 ToShaderColor(Color color)
		{
			Color res = QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
			return res;
		}
	}
}
