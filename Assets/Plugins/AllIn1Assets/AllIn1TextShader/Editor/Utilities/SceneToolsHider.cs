using System.Collections.Generic;
using UnityEditor;

namespace AllIn1Assets.AllIn1TextShader
{
	// Several inspectors can hide the transform gizmo at once (a spline and a warp cage on one object); it comes back only when none of them wants it hidden
	public static class SceneToolsHider
	{
		private static readonly HashSet<UnityEngine.Object> owners = new HashSet<UnityEngine.Object>();

		public static void SetHidden(UnityEngine.Object owner, bool hidden)
		{
			bool changed = hidden ? owners.Add(owner) : owners.Remove(owner);
			if (changed)
			{
				owners.RemoveWhere(o => o == null);
				Tools.hidden = owners.Count > 0;
			}
		}
	}
}
