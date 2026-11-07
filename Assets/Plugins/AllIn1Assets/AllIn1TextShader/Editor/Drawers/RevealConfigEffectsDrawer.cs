using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class RevealConfigEffectsDrawer
	{
		private EasingFunctionsSO s_easeFunctionCollection;
		private string[] easeFunctionNames;

		public void Draw(MeshEffectGroupConfig effectGroupConfig, MeshEffectCollection effectDatabase, 
			string[] easeFunctionNames, EasingFunctionsSO easeFunctionCollection,
			CommonStyles commonStyles, ref bool changes)
		{
			s_easeFunctionCollection = easeFunctionCollection;
			this.easeFunctionNames = easeFunctionNames;

			DrawTypweriterSection(effectGroupConfig, commonStyles);

			if(MeshEffectGroupState.GetGroupType(effectGroupConfig) == GroupType.PER_TAGS)
			{
				DrawEffectsSectionPerTags(effectGroupConfig, effectDatabase, commonStyles);
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				DrawEffectsSectionNormal(effectGroupConfig, effectDatabase, commonStyles);
				changes = changes || EditorGUI.EndChangeCheck();
			}
		}

		private void DrawTypweriterSection(MeshEffectGroupConfig effectGroupConfig, CommonStyles commonStyles)
		{
			effectGroupConfig.displayName = EditorGUILayout.TextField("Config Name", effectGroupConfig.displayName);

			// Tags only drive loops, so a reveal or hide can't pick them
			effectGroupConfig.groupType = (GroupType)EditorGUILayout.EnumPopup(new GUIContent("Group Type"), effectGroupConfig.groupType,
				groupType => effectGroupConfig.isLooped || (GroupType)groupType != GroupType.PER_TAGS, false);

			GroupType effectiveGroupType = MeshEffectGroupState.GetGroupType(effectGroupConfig);
			if (effectiveGroupType != effectGroupConfig.groupType)
			{
				EditorGUILayout.HelpBox("Tags only drive loops, so this config plays Per Character.", MessageType.Warning);
			}

			// A loop starts every group at once, so neither the order nor the pace applies to it
			if (effectiveGroupType != GroupType.PER_TAGS && !effectGroupConfig.isLooped)
			{
				effectGroupConfig.traversalOrder = (TraversalOrder)EditorGUILayout.EnumPopup("Traversal Order", effectGroupConfig.traversalOrder);
				string groupDelayName = GetGroupDelayName(effectiveGroupType);

				int groupsPerSecond = Mathf.CeilToInt(1.0f / effectGroupConfig.groupDelay);
				groupsPerSecond = EditorGUILayout.IntSlider(groupDelayName, groupsPerSecond, 1, 200);
				effectGroupConfig.groupDelay = 1.0f / groupsPerSecond;
			}
		}

		private void DrawEffectsSectionPerTags(MeshEffectGroupConfig effectGroupConfig, MeshEffectCollection effectDatabase, CommonStyles commonStyles)
		{
			MeshEffectConfig[] effecConfigsOrdered = effectGroupConfig.effectConfigs;

			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Effects", commonStyles.bigLabel);
			for (int i = 0; i < effecConfigsOrdered.Length; i++)
			{
				DrawEffectConfigPerTags(i, effecConfigsOrdered[i], effectDatabase);
			}
			EditorGUILayout.EndVertical();
		}

		private void DrawEffectsSectionNormal(MeshEffectGroupConfig effectGroupConfig, MeshEffectCollection effectDatabase, CommonStyles commonStyles)
		{
			MeshEffectConfig[] effecConfigsOrdered = effectGroupConfig.effectConfigs;

			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Effects", commonStyles.bigLabel);
			for (int i = 0; i < effecConfigsOrdered.Length; i++)
			{
				DrawEffectConfig(i, effecConfigsOrdered[i], effectDatabase);
			}
			EditorGUILayout.EndVertical();
		}

		private void DrawEffectConfigPerTags(int index, MeshEffectConfig meshEffectConfig, MeshEffectCollection effectDatabase)
		{
			CommonStyles commonStyles = DrawersSharedConfig.s_commonStyles;

			int idx;
			MeshEffectInfo effectInfo = effectDatabase.FindInfoByID(meshEffectConfig.id, out idx);

			string label = $"{index}. {effectInfo.effectName}";


			EditorGUI.indentLevel++;
			meshEffectConfig.unfolded = EditorGUILayout.Foldout(meshEffectConfig.unfolded, label, true);
			EditorGUI.indentLevel--;
			if (meshEffectConfig.unfolded)
			{
				PropertyInstance[] propertiesOrdered = meshEffectConfig.orderedProperties;
				PropertyInstance[] effectDefaults = GetEffectDefaults(effectInfo);

				EditorGUILayout.BeginVertical(commonStyles.boxStyle);

				int selectedIndex = s_easeFunctionCollection.GetIndexByID(meshEffectConfig.easeFunctionID);
				selectedIndex = EditorGUILayout.Popup("Ease Function", selectedIndex, easeFunctionNames);
				meshEffectConfig.easeFunctionID = s_easeFunctionCollection.GetIDByIndex(selectedIndex);

				for (int i = 0; i < propertiesOrdered.Length; i++)
				{
					EditorGUILayout.BeginHorizontal();
					PropertyInstanceDrawer.Draw(ref propertiesOrdered[i], effectDefaults);
					EditorGUILayout.EndHorizontal();
				}


				EditorUIUtils.AddSpaces(2);
				string tagStr = meshEffectConfig.GetTagStr();
				EditorGUILayout.LabelField(tagStr, commonStyles.mediumLabel, GUILayout.Width(800f));
				EditorUIUtils.AddSpaces(1);
				if (GUILayout.Button("Copy Tag", GUILayout.Width(200f)))
				{
					GUIUtility.systemCopyBuffer = tagStr;
				}

				EditorGUILayout.EndVertical();
			}
		}

		private void DrawEffectConfig(int index, MeshEffectConfig meshEffectConfig, MeshEffectCollection effectDatabase)
		{
			CommonStyles commonStyles = DrawersSharedConfig.s_commonStyles;

			int idx;
			MeshEffectInfo effectInfo = effectDatabase.FindInfoByID(meshEffectConfig.id, out idx);

			string label = $"{index}. {effectInfo.effectName}";

			EditorGUILayout.BeginHorizontal();
			meshEffectConfig.enabled = EditorGUILayout.ToggleLeft(label, meshEffectConfig.enabled, commonStyles.smallLabel);
			EditorGUILayout.EndHorizontal();




			if (meshEffectConfig.enabled)
			{
				PropertyInstance[] propertiesOrdered = meshEffectConfig.orderedProperties;
				PropertyInstance[] effectDefaults = GetEffectDefaults(effectInfo);

				EditorGUILayout.BeginVertical(commonStyles.boxStyle);

				int selectedIndex = s_easeFunctionCollection.GetIndexByID(meshEffectConfig.easeFunctionID);
				selectedIndex = EditorGUILayout.Popup("Ease Function", selectedIndex, easeFunctionNames);
				meshEffectConfig.easeFunctionID = s_easeFunctionCollection.GetIDByIndex(selectedIndex);

				for (int i = 0; i < propertiesOrdered.Length; i++)
				{
					EditorGUILayout.BeginHorizontal();
					PropertyInstanceDrawer.Draw(ref propertiesOrdered[i], effectDefaults);
					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.EndVertical();
			}
		}

		private static readonly System.Collections.Generic.Dictionary<MeshEffectInfo, PropertyInstance[]> s_effectDefaults = new System.Collections.Generic.Dictionary<MeshEffectInfo, PropertyInstance[]>();

		// The values a new config of this effect gets, built once per effect
		private static PropertyInstance[] GetEffectDefaults(MeshEffectInfo effectInfo)
		{
			if (effectInfo == null) { return null; }

			PropertyInstance[] res;
			if (!s_effectDefaults.TryGetValue(effectInfo, out res))
			{
				res = effectInfo.CreateEffectConfig().orderedProperties;
				s_effectDefaults[effectInfo] = res;
			}

			return res;
		}

		private string GetGroupDelayName(GroupType groupType)
		{
			string res = string.Empty;

			switch (groupType)
			{
				case GroupType.PER_CHARACTER:
					res = "Characters per second";
					break;
				case GroupType.PER_WORD:
					res = "Words per second";
					break;
			}

			return res;
		}
	}
}