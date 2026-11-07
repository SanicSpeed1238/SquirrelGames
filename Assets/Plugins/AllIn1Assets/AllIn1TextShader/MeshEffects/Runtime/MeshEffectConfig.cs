using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public class MeshEffectConfig
	{
		public string id;
		public bool enabled;
		public MeshEffectInfo effectInfo;

		public string easeFunctionID;

		// Set by Invert on runtime copies, so the inverted effect plays its ease backwards
		[System.NonSerialized] public bool mirrorEase;

		public PropertyInstance[] orderedProperties;

#if UNITY_EDITOR
		public PropertyInfo[] propertyDefinitions;
		public bool unfolded; //bool property used when PER_TAGS option is enabled
#endif


		public MeshEffectState CreateState(ref VertexGroupLayout[] vertexGroupLayouts, TraversalOrder traversalOrder, PlaybackMode playbackMode, float groupDelay)
		{
			MeshEffectState res = effectInfo.CreateState();
			return res;
		}

		public void Invert()
		{
			effectInfo.Invert(this);
			mirrorEase = !mirrorEase;
		}

		public void CopyFrom(MeshEffectConfig from)
		{
			id = from.id;
			effectInfo = from.effectInfo;
			enabled = from.enabled;
			easeFunctionID = from.easeFunctionID;
			mirrorEase = from.mirrorEase;
			orderedProperties = new PropertyInstance[from.orderedProperties.Length];

			for (int i = 0; i < orderedProperties.Length; i++)
			{
				orderedProperties[i].CopyFrom(from.orderedProperties[i]);
			}
		}

		public int FindPropertyIndex(string propertyName)
		{
			int res = -1;

			for(int i = 0; i < orderedProperties.Length; i++)
			{
				if (orderedProperties[i].propertyName == propertyName)
				{
					res = i;
					break;
				}
			}

			return res;
		}

		public ref PropertyInstance FindPropertyRef(string propertyName)
		{
			int propertyIndex = FindPropertyIndex(propertyName);
			return ref orderedProperties[propertyIndex];
		}

		public int FindPropertyIndexInArray(PropertyInstance[] target, string propertyName)
		{
			int res = -1;

			for (int i = 0; i < target.Length; i++)
			{
				if (target[i].propertyName == propertyName)
				{
					res = i;
					break;
				}
			}

			return res;
		}

		public PropertyInstance FindProperty(string propertyName)
		{
			int propertyIndex = FindPropertyIndex(propertyName);
			return orderedProperties[propertyIndex];
		}

		public float GetFloat(string propertyName)
		{
			int propertyIndex = FindPropertyIndex(propertyName);

			float res = orderedProperties[propertyIndex].floatValue;
			return res;
		}

		public Vector3 GetVector3(string propertyName)
		{
			int propertyIndex = FindPropertyIndex(propertyName);

			Vector3 res = orderedProperties[propertyIndex].vec3Value;
			return res;
		}

		public Vector2 GetVector2(string propertyName)
		{
			int propertyIndex = FindPropertyIndex(propertyName);

			Vector2 res = orderedProperties[propertyIndex].vec2Value;
			return res;
		}

		public bool IsEnabled(GroupType groupType, EffectFlag enabledEffects)
		{
			bool res = false;
			if(groupType == GroupType.PER_TAGS)
			{
				res = (effectInfo.GetEffectFlag() & enabledEffects) == effectInfo.GetEffectFlag();
			}
			else
			{
				res = this.enabled;
			}

			return res;
		}

		public void InitValues(ref VertexGroupLayout[] vertexGroupLayouts, EasingFunctionsSO easeFunctionCollection, bool isLooped)
		{
			EaseFunctionSO easeFunction = easeFunctionCollection.FindEaseFunctionByID(easeFunctionID);
			// A curve missing from the collection (deleted, or lost in an update) plays as its first one, Linear, instead of throwing every frame
			if (easeFunction == null && easeFunctionCollection.easeFunctions.Length > 0)
			{
				easeFunction = easeFunctionCollection.easeFunctions[0];
			}
			if (mirrorEase && easeFunction != null)
			{
				easeFunction = easeFunction.GetMirrored();
			}
			effectInfo.ConfigureVertexGroupLayoutCollection(ref vertexGroupLayouts, orderedProperties, easeFunction, isLooped);
		}

#if UNITY_EDITOR
		public PropertyInstance[] CreateOrderedPropertiesCopy()
		{
			PropertyInstance[] res = new PropertyInstance[orderedProperties.Length];

			for(int i = 0; i < orderedProperties.Length; i++)
			{
				PropertyInstance propInstance = new PropertyInstance();
				propInstance.CopyFrom(orderedProperties[i]);
				res[i] = propInstance;
			}

			return res;
		}

		public void AssignMatchedProperties(MeshEffectConfig oldMeshEffectConfig)
		{
			AssignMatchedProperties(oldMeshEffectConfig.orderedProperties);
		}

		public void AssignMatchedProperties(PropertyInstance[] properties)
		{
			for(int i = 0; i < this.orderedProperties.Length; i++)
			{
				PropertyInstance currentProperty = this.orderedProperties[i];

				int oldPropertyIndex = FindPropertyIndexInArray(properties, currentProperty.propertyName);

				if (oldPropertyIndex >= 0)
				{
					PropertyInstance oldProperty = properties[oldPropertyIndex];

					currentProperty.CopyValuesFrom(oldProperty);

					this.orderedProperties[i] = currentProperty;
				}
			}
		}

		public MeshEffectConfig DeepCopy()
		{
			MeshEffectConfig res = new MeshEffectConfig();

			res.id = this.id;
			res.enabled = this.enabled;
			res.effectInfo = this.effectInfo;
			res.easeFunctionID = this.easeFunctionID;
			res.mirrorEase = this.mirrorEase;

			res.orderedProperties = new PropertyInstance[this.orderedProperties.Length];
			for(int i = 0; i < res.orderedProperties.Length; i++)
			{
				PropertyInstance propertyInstance = new PropertyInstance();
				propertyInstance.CopyFrom(this.orderedProperties[i]);
				res.orderedProperties[i] = propertyInstance;
			}

			return res;
		}

		public string GetTagStr()
		{
			string res = string.Empty;

			res += $"<{id};";

			// A tag starts from the effect's defaults, so the ease only needs writing when it differs
			if (effectInfo != null && easeFunctionID != effectInfo.easeFunctionID.defaultValue)
			{
				res += $"{effectInfo.easeFunctionID.tagBinding}:{easeFunctionID};";
			}

			for (int i = 0; i < orderedProperties.Length; i++)
			{
				res += $"{orderedProperties[i].tagBinding}:{orderedProperties[i].GetTagValue()}";

				if(i < orderedProperties.Length - 1)
				{
					res += ";";
				}
			}

			res += ">";
			res += "YOUR_TEXT";
			res += $"</>";

			return res;
		}
#endif
	}
}