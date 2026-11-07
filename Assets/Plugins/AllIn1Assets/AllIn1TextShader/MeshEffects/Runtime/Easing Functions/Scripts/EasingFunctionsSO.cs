using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class EasingFunctionsSO : ScriptableObject
	{
		public EaseFunctionSO[] easeFunctions;

		public EaseFunctionSO FindEaseFunctionByID(string id, out int index)
		{
			EaseFunctionSO res = null;
			index = 0;
			for(int i = 0; i < easeFunctions.Length; i++)
			{
				if (easeFunctions[i].id == id)
				{
					res = easeFunctions[i];
					index = i;
					break;
				}
			}

			return res;
		}

		public EaseFunctionSO FindEaseFunctionByID(string id)
		{
			int index;
			EaseFunctionSO res = FindEaseFunctionByID(id, out index);
			return res;
		}

		public string[] GetEaseFunctionNamesArray()
		{
			string[] res = new string[easeFunctions.Length];

			for(int i = 0; i < easeFunctions.Length; i++)
			{
				res[i] = easeFunctions[i].displayName;
			}

			return res;
		}

		public EaseFunctionSO GetEaseFunctionByIndex(int index)
		{
			EaseFunctionSO res = easeFunctions[index];
			return res;
		}

		public int GetIndexByID(string id)
		{
			int res;
			FindEaseFunctionByID(id, out res);

			return res;
		}

		public string GetIDByIndex(int index)
		{
			string res = easeFunctions[index].id;
			return res;
		}
	}
}