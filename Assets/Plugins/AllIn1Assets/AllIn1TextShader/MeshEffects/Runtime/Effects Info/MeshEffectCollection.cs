using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class MeshEffectCollection : ScriptableObject
	{
		public MeshEffectInfo[] effects;

		public MeshEffectInfo FindInfoByID(string id, out int idx)
		{
			MeshEffectInfo res = null;
			idx = -1;

			for (int i = 0; i < effects.Length; i++)
			{
				if (string.Equals(effects[i].id, id, System.StringComparison.OrdinalIgnoreCase))
				{
					res = effects[i];
					idx = i;
					break;
				}
			}

			return res;
		}

		public MeshEffectInfo FindInfoByID(string id)
		{
			int idx;
			MeshEffectInfo res = FindInfoByID(id, out idx);
			return res;
		}

		public MeshEffectConfig[] CreateMeshEffectConfigs()
		{
			MeshEffectConfig[] res = new MeshEffectConfig[effects.Length];

			for(int i = 0; i < res.Length; i++)
			{
				MeshEffectInfo effectInfo = effects[i];
				res[i] = effectInfo.CreateEffectConfig();
			}

			return res;
		}
	}
}