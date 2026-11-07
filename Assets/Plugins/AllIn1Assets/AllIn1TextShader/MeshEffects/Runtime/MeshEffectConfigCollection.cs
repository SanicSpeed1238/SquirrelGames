using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	public class MeshEffectConfigCollection : ScriptableObject
	{
		public MeshEffectGroupConfig[] configs;

		public void SetConfigNames(ref string[] configNames)
		{
			if (configs == null)
			{
				configNames = new string[0];
			}
			else
			{
				if (configNames == null || configNames.Length != configs.Length)
				{
					configNames = new string[configs.Length];
				}

				for (int i = 0; i < configs.Length; i++)
				{
					configNames[i] = configs[i].displayName;
				}
			}
		}

		public int FindIndex(MeshEffectGroupConfig hideConfig)
		{
			int res = -1;

			if (configs != null)
			{
				for (int i = 0; i < configs.Length; i++)
				{
					if (configs[i] == hideConfig)
					{
						res = i;
						break;
					}
				}
			}
			return res;
		}

		public MeshEffectGroupConfig GetConfig(int index)
		{
			MeshEffectGroupConfig res = null;

			if (configs != null)
			{
				if (index >= 0 && index <= configs.Length - 1)
				{
					res = configs[index];
				}
			}

			return res;
		}


#if UNITY_EDITOR
		public MeshEffectGroupConfig CreateMeshEffectGroupConfig(string configFolderPath, string assetPrefix)
		{
			MeshEffectGroupConfig res = ScriptableObject.CreateInstance<MeshEffectGroupConfig>();

			// Deleted presets free their slot, so the name taken from the count can already be on disk, and CreateAsset would overwrite it
			int configIndex = configs.Length;
			string configName = $"{assetPrefix}_{configIndex}";
			string path = GetConfigPath(configFolderPath, configName);
			while (File.Exists(path))
			{
				configIndex++;
				configName = $"{assetPrefix}_{configIndex}";
				path = GetConfigPath(configFolderPath, configName);
			}

			res.displayName = configName;
			
			ArrayUtility.Add(ref configs, res);

			EditorUtility.SetDirty(this);

			AssetDatabase.CreateAsset(res, path);

			return res;
		}

		//Runtime assembly, so EditorUtils is out of reach and Path.Combine needs fixing up by hand
		private static string GetConfigPath(string configFolderPath, string configName)
		{
			string res = Path.Combine(configFolderPath, $"{configName}.asset").Replace('\\', '/');
			return res;
		}

		public void RegisterConfig(MeshEffectGroupConfig config)
		{
			ArrayUtility.Add(ref configs, config);
			EditorUtility.SetDirty(this);
		}

		public void DeleteConfig(MeshEffectGroupConfig config)
		{
			int index = FindIndex(config);
			if (index < 0) { return; }

			ArrayUtility.RemoveAt(ref configs, index);
			EditorUtility.SetDirty(this);

			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(config));
		}

		// A preset deleted from the Project window leaves a missing entry behind
		public void RemoveMissingConfigs()
		{
			if (configs == null) { return; }

			bool removed = false;
			for (int i = configs.Length - 1; i >= 0; i--)
			{
				if (configs[i] == null)
				{
					ArrayUtility.RemoveAt(ref configs, i);
					removed = true;
				}
			}

			if (removed)
			{
				EditorUtility.SetDirty(this);
			}
		}
#endif
	}
}