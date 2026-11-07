using System.Collections.Generic;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public abstract class MeshEffectInfo : ScriptableObject
	{
		public string id;
		public string effectName;

		public PropertyStringInfo easeFunctionID;

		public MeshEffectConfig CreateEffectConfig()
		{
			MeshEffectConfig res = new MeshEffectConfig();

			res.id = id;
			res.effectInfo = this;
			res.easeFunctionID = easeFunctionID.defaultValue;

			ConfigureOrderedProperties(res);

			return res;
		}

		public abstract void ConfigureOrderedProperties(MeshEffectConfig target);

		public abstract MeshEffectState CreateState();

		public abstract void Invert(MeshEffectConfig target);

		public abstract EffectFlag GetEffectFlag();

		public void ConfigureParsedGroup(ref TextParsedGroup parsedGroup, List<ParsedParameter> parsedParameters, EasingFunctionsSO easeFunctionCollection)
		{
			// What the tag leaves out keeps this effect's defaults, the values a new config gets in the inspector
			VertexGroupLayout defaults = new VertexGroupLayout();
			ConfigureVertexGroupLayout(ref defaults, CreateEffectConfig().orderedProperties, FindParsedEase(parsedParameters, easeFunctionCollection));
			CopyEffectValues(ref defaults, ref parsedGroup);

			for (int i = 0; i < parsedParameters.Count; i++)
			{
				SetupParsedParameter(ref parsedGroup, parsedParameters[i], easeFunctionCollection);
			}
		}

		private EaseFunctionSO FindParsedEase(List<ParsedParameter> parsedParameters, EasingFunctionsSO easeFunctionCollection)
		{
			EaseFunctionSO res = null;

			for (int i = 0; i < parsedParameters.Count; i++)
			{
				string easeID = parsedParameters[i].stringValue;
				if (parsedParameters[i].paramName != easeFunctionID.tagBinding || string.IsNullOrEmpty(easeID)) { continue; }

				res = easeFunctionCollection.FindEaseFunctionByID(easeID);
				if (res == null)
				{
					Debug.LogWarning($"All In 1 Text: unknown ease '{easeID}' in a <{id}> tag, {easeFunctionID.defaultValue} is used");
				}
			}

			if (res == null)
			{
				res = easeFunctionCollection.FindEaseFunctionByID(easeFunctionID.defaultValue);
			}

			// A custom collection without this effect's default still gets an ease, never null
			if (res == null && easeFunctionCollection.easeFunctions.Length > 0)
			{
				res = easeFunctionCollection.easeFunctions[0];
			}

			return res;
		}

		private void CopyEffectValues(ref VertexGroupLayout from, ref TextParsedGroup to)
		{
			switch (GetEffectFlag())
			{
				case EffectFlag.Fade:				to.alphaStateValues = from.alphaStateValues; break;
				case EffectFlag.InitialRotation:	to.initialRotationStateValues = from.initialRotationStateValues; break;
				case EffectFlag.Scale:				to.scaleStateValues = from.scaleStateValues; break;
				case EffectFlag.Offset:				to.offsetStateValues = from.offsetStateValues; break;
				case EffectFlag.Pendulum:			to.pendulumStateValues = from.pendulumStateValues; break;
				case EffectFlag.Shake:				to.shakeStateValues = from.shakeStateValues; break;
				case EffectFlag.Rainbow:			to.rainbowStateValues = from.rainbowStateValues; break;
				case EffectFlag.Swing:				to.swingStateValues = from.swingStateValues; break;
				case EffectFlag.Rotation:			to.rotationStateValues = from.rotationStateValues; break;
				case EffectFlag.Dangle:				to.dangleStateValues = from.dangleStateValues; break;
				case EffectFlag.Slide:				to.slideStateValues = from.slideStateValues; break;
				case EffectFlag.VerticalMovement:	to.verticalMovementValues = from.verticalMovementStateValues; break;
			}
		}

		protected abstract void SetupParsedParameter(ref TextParsedGroup parsedGroup, ParsedParameter parsedParameter, EasingFunctionsSO easeFunctionCollection);

		public void ConfigureVertexGroupLayoutCollection(ref VertexGroupLayout[] vertexGroupLayouts, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction, bool isLooped)
		{
			for(int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				VertexGroupLayout vertexGroupLayout = vertexGroupLayouts[i];

				ConfigureVertexGroupLayout(ref vertexGroupLayout, propertyInstances, easeFunction);
				if (isLooped)
				{
					vertexGroupLayout.InitAtRandomPoint();
				}

				vertexGroupLayouts[i] = vertexGroupLayout;

			}
		}

		public abstract void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction);
	}
}