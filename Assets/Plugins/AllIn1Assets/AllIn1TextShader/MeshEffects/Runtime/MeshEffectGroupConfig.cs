using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	public class MeshEffectGroupConfig : ScriptableObject
	{
		public string displayName;
		public float groupDelay;
		public bool startWithRandomOffset;

		public bool isLooped;

		public GroupType groupType;
		public TraversalOrder traversalOrder;

		public EasingFunctionsSO easingFunctions;

		public MeshEffectConfig[] effectConfigs;

#if UNITY_EDITOR
		public bool unfolded;
#endif

		public virtual void PopulateStates(List<MeshEffectState> states, ref VertexGroupLayout[] vertexGroupLayouts, 
			PlaybackMode playbackMode, GroupType groupType, EffectFlag enabledEffects)
		{
			for (int i = 0; i < effectConfigs.Length; i++)
			{
				if (!effectConfigs[i].IsEnabled(groupType, enabledEffects)) { continue; }

				states.Add(effectConfigs[i].CreateState(ref vertexGroupLayouts, traversalOrder, playbackMode, groupDelay));
				if(groupType != GroupType.PER_TAGS)
				{
					effectConfigs[i].InitValues(ref vertexGroupLayouts, easingFunctions, isLooped);
				}
			}
		}

		public MeshEffectConfig FindMeshEffectConfigInArray(string effectID, MeshEffectConfig[] effectConfigs)
		{
			MeshEffectConfig res = null;

			for (int i = 0; i < effectConfigs.Length; i++)
			{
				if (effectConfigs[i].id == effectID)
				{
					res = effectConfigs[i];
					break;
				}
			}

			return res;
		}

		public MeshEffectConfig FindMeshEffectConfigByID(string effectID)
		{
			MeshEffectConfig res = null;

			for(int i = 0; i < effectConfigs.Length; i++)
			{
				if (effectConfigs[i].id == effectID)
				{
					res = effectConfigs[i];
					break;
				}
			}

			return res;
		}

		public void Invert()
		{
			for (int i = 0; i < effectConfigs.Length; i++)
			{
				effectConfigs[i].Invert();
			}

			TraversalOrder newTraversalOrder = traversalOrder;

			switch (traversalOrder)
			{
				case TraversalOrder.LEFT_TO_RIGHT:
					newTraversalOrder = TraversalOrder.RIGHT_TO_LEFT;
					break;
				case TraversalOrder.RIGHT_TO_LEFT:
					newTraversalOrder = TraversalOrder.LEFT_TO_RIGHT;
					break;
				case TraversalOrder.FROM_CENTER_TO_OUTWARD:
					newTraversalOrder = TraversalOrder.FROM_OUTWARD_TO_CENTER;
					break;
				case TraversalOrder.FROM_OUTWARD_TO_CENTER:
					newTraversalOrder = TraversalOrder.FROM_CENTER_TO_OUTWARD;
					break;
			}

			traversalOrder = newTraversalOrder;
		}

		public void CopyValuesFrom(MeshEffectGroupConfig from)
		{
			effectConfigs = new MeshEffectConfig[from.effectConfigs.Length];

			groupType = from.groupType;
			groupDelay = from.groupDelay;
			traversalOrder = from.traversalOrder;
			easingFunctions = from.easingFunctions;
			isLooped = from.isLooped;

			for (int i = 0; i < effectConfigs.Length; i++)
			{
				MeshEffectConfig meshEffectConfig = new MeshEffectConfig();
				meshEffectConfig.CopyFrom(from.effectConfigs[i]);
				effectConfigs[i] = meshEffectConfig;
			}
		}

#if UNITY_EDITOR
		public void AddMissingEffects(MeshEffectCollection effectDatabase, MeshEffectConfigCollection configCollection)
		{
			for (int i = 0; i < effectDatabase.effects.Length; i++)
			{
				MeshEffectInfo meshEffectInfo = effectDatabase.effects[i];

				MeshEffectConfig meshEffecConfig = FindMeshEffectConfigByID(meshEffectInfo.id);
				if(meshEffecConfig == null)
				{
					MeshEffectConfig newConfig = meshEffectInfo.CreateEffectConfig();

					ArrayUtility.Add(ref effectConfigs, newConfig);
				}
			}
		}

		public void RefreshEffectProperties(MeshEffectCollection meshEffectCollection, MeshEffectConfig[] oldEffectConfigs)
		{
			for(int i = 0; i < meshEffectCollection.effects.Length; i++)
			{
				MeshEffectInfo meshEffectInfo = meshEffectCollection.effects[i];
				MeshEffectConfig oldEffectConfig = FindMeshEffectConfigInArray(meshEffectInfo.id, oldEffectConfigs);

				RefreshEffectProperties(meshEffectCollection.effects[i], oldEffectConfig);
			}
		}

		public void RefreshEffectProperties(MeshEffectInfo meshEffectInfo, MeshEffectConfig oldEffectConfig)
		{
			MeshEffectConfig meshEffectConfig = FindMeshEffectConfigByID(meshEffectInfo.id);

			if (meshEffectConfig != null)
			{
				meshEffectInfo.ConfigureOrderedProperties(meshEffectConfig);

				if (oldEffectConfig != null)
				{
					meshEffectConfig.AssignMatchedProperties(oldEffectConfig);

					meshEffectConfig.enabled = oldEffectConfig.enabled;
					meshEffectConfig.easeFunctionID = oldEffectConfig.easeFunctionID;
				}
			}
		}
#endif
	}
}