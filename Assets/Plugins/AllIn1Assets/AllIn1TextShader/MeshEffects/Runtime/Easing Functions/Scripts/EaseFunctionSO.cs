using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class EaseFunctionSO : ScriptableObject
	{
		public string id;
		public string displayName;
		public AnimationCurve curve;

		[System.NonSerialized] private bool isMirror;
		[System.NonSerialized] private EaseFunctionSO mirrored;

		public float Evaluate(float t)
		{
			float res = isMirror ? 1f - curve.Evaluate(1f - t) : curve.Evaluate(t);
			return res;
		}

		// The same curve played backwards, so a hide inverted from a reveal retraces it (an overshoot pop becomes an anticipation)
		public EaseFunctionSO GetMirrored()
		{
			if (mirrored == null)
			{
				mirrored = CreateInstance<EaseFunctionSO>();
				mirrored.hideFlags = HideFlags.DontSave;
				mirrored.id = id;
				mirrored.displayName = displayName;
				mirrored.curve = curve;
				mirrored.isMirror = true;
			}

			return mirrored;
		}
	}
}