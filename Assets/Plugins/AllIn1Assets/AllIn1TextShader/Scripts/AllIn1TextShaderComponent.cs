using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	// No RequireComponent: TMP_Text is abstract, so Unity refused the add on any object without one. Reset gives the object its text instead
	[DisallowMultipleComponent]
	[AddComponentMenu("AllIn1TextShader/AddAllIn1TextShader")]
	public class AllIn1TextShaderComponent : PluginComponent
	{
		private const string TAG = "AllIn1TextShaderComponent";
		private const string PROP_LIGHT_ANGLE = "_LightAngle";

#if UNITY_EDITOR
		// Set by the editor's TextComponentSetup: converts a legacy UI Text or adds a new text, and says whether the object has a TMP_Text now
		public static System.Func<GameObject, bool> textSetupHook;
#endif

		public TMP_Text textComponent;
		public TextMaterialConfiguratorMaster materialConfigurator;
		public AllIn1TextTypewriter typewriter;
		public TextLayeredComponent textLayered;
		public AllIn1TextSpline spline;
		public AllIn1TextWarp warp;

		public override void AwakeImpl()
		{

		}

		protected override void OnEnableRuntime()
		{
			
		}

		protected override void OnDisableRuntime()
		{
			
		}

		public override Material GetCurrentMaterial()
		{
			return textComponent.fontSharedMaterial;
		}

		public override void SetCurrentMaterial(Material mat)
		{
			textComponent.fontSharedMaterial = mat;
		}

		public string GetFontAtlasName()
		{
			string res = textComponent.font.name;
			res += " Atlas";
			return res;
		}

		public Texture2D GetFontTexture()
		{
			Texture2D res = textComponent.font.atlasTexture;
			return res;
		}

		public TMP_FontAsset GetFontAsset()
		{
			TMP_FontAsset res = textComponent.font;
			return res;
		}

		public AllIn1TextTypewriter GetTypwriter()
		{
			return typewriter;
		}

		public TextLayeredComponent GetTextLayered()
		{
			return textLayered;
		}

		public AllIn1TextSpline GetSpline()
		{
			return spline;
		}

		public AllIn1TextWarp GetWarp()
		{
			return warp;
		}

#if UNITY_EDITOR
		protected override void OnEnableEditor()
		{
			CheckComponentReferences();
		}

		protected override void OnDisableEditor()
		{
			if (materialConfigurator != null)
			{
				materialConfigurator.Dispose();
			}
		}

		public override bool CheckValidComponent()
		{
			bool res = true;

			bool dirty = false;
			if (textComponent == null)
			{
				res = res && TryGetComponent<TMP_Text>(out textComponent);
				dirty = true;
			}

			if (dirty && res)
			{
				textComponent = GetComponent<TMP_Text>();

				EditorUtility.SetDirty(this);

				Material currentMaterial = GetCurrentMaterial();
				if (GetCurrentMaterial() != null)
				{
					EditorUtility.SetDirty(currentMaterial);
				}
			}

			return res;
		}

		// With no text and none possible the configurator stays out, and the inspector removes this component and says why
		public override void ResetImpl()
		{
			bool hasText = TryGetComponent(out TMP_Text _) || (textSetupHook != null && textSetupHook(gameObject));
			if (!hasText) { return; }

			CheckComponentReferences();
		}

		private void CheckComponentReferences()
		{
			if (textComponent == null)
			{
				textComponent = GetComponent<TMP_Text>();
			}

			typewriter = GetComponent<AllIn1TextTypewriter>();
			spline = GetComponent<AllIn1TextSpline>();
			warp = GetComponent<AllIn1TextWarp>();

			if (textComponent != null)
			{
				CheckMaterialConfigurator();
			}
		}

		private void CheckMaterialConfigurator()
		{
			if (materialConfigurator == null) 
			{
				bool success = gameObject.TryGetComponent<TextMaterialConfiguratorMaster>(out materialConfigurator);
				if (!success)
				{
					materialConfigurator = gameObject.AddComponent<TextMaterialConfiguratorMaster>();

				}
			}
		}

		public override void InitMaterial(Material mat)
		{
			materialConfigurator.InitMaterial(mat);
		}

		// The base copies every matching property from the previous material, which on a new text is TMP's, whose light angle is in radians
		public override void NewCleanMaterial(Material defaultMaterial)
		{
			Material previousMat = GetCurrentMaterial();
			base.NewCleanMaterial(defaultMaterial);

			Material newMat = GetCurrentMaterial();
			if (TextMaterialConfiguratorMaster.IsTMPLightAngle(previousMat, newMat) && defaultMaterial.HasProperty(PROP_LIGHT_ANGLE))
			{
				newMat.SetFloat(PROP_LIGHT_ANGLE, defaultMaterial.GetFloat(PROP_LIGHT_ANGLE));
			}
		}

		public override void CleanMaterial(Shader newShader)
		{
			Material previousMat = GetCurrentMaterial();
			base.CleanMaterial(newShader);

			Material newMat = GetCurrentMaterial();
			int lightAngleIndex = newShader.FindPropertyIndex(PROP_LIGHT_ANGLE);
			if (TextMaterialConfiguratorMaster.IsTMPLightAngle(previousMat, newMat) && lightAngleIndex >= 0)
			{
				newMat.SetFloat(PROP_LIGHT_ANGLE, newShader.GetPropertyDefaultFloatValue(lightAngleIndex));
			}
		}

		public override void RemoveChildComponents()
		{
			
		}

		public void SetBlurredAtlas(Texture blurredAtlasTex)
		{
			materialConfigurator.SetBlurredAtlas(blurredAtlasTex);
		}

		public string GetFontAssetPath()
		{
			string res = AssetDatabase.GetAssetPath(textComponent.font);
			return res;
		}
#endif

	}
}