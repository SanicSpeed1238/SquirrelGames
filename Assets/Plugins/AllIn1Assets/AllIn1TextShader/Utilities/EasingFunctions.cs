using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public static class EasingFunctions
	{
		public static float Bell(float t)
		{
			float res = 4f * t * (1f - t);
			return res;
		}

		public static float EaseInQuint(float t)
		{
			float res = t * t * t * t * t;
			return res;
		}

		public static float EaseOutCubic(float t)
		{
			float res = 1 - Mathf.Pow(1f - t, 3);
			return res;
		}

		public static float EaseInOutSine(float t)
		{
			float res = -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
			return res;
		}

		public static float EaseOutBounce(float t)
		{
			float n1 = 7.5625f;
			float d1 = 2.75f;

			float res = 0f;
			if (t < 1 / d1)
			{
				res = n1 * t * t;
			}
			else if (t < 2 / d1)
			{
				res = n1 * (t -= 1.5f / d1) * t + 0.75f;
			}
			else if (t < 2.5 / d1)
			{
				res = n1 * (t -= 2.25f / d1) * t + 0.9375f;
			}
			else
			{
				res = n1 * (t -= 2.625f / d1) * t + 0.984375f;
			}

			return res;
		}

		public static float EaseOutBounceMirrored(float t)
		{
			float mirroredT = t < 0.5f ? t * 2f : (1f - t) * 2f;
			float res = EaseOutBounce(mirroredT);
			return res;
		}

		public static float EaseInBounce(float t)
		{
			float res = 1f - EaseOutBounce(1f - t);
			return res;
		}

		public static float EaseInBounceMirrored(float t)
		{
			float res = 0f;
			if(t < 0.5f)
			{
				float auxT = t * 2f;
				res = EaseOutCubic(auxT);
			}
			else
			{
				float auxT = (1f - t) * 2f;
				res = EaseInBounce(auxT);
			}

			//float mirroredT = t < 0.5f ? t * 2f : (1f - t) * 2f;
			
			
			//res = EaseInBounce(mirroredT);
			return res;
		}

		/*function easeInBounce(x: number): number {
return 1 - easeOutBounce(1 - x);

}*/
	}
}