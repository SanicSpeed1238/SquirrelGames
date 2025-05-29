#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CS_UIAnimator))]
public class Editor_UIAnimator : Editor
{
    public override void OnInspectorGUI()
    {
        CS_UIAnimator script = (CS_UIAnimator)target;

        // Draw default Appear Animation option
        EditorGUILayout.LabelField("Appear Animation", EditorStyles.boldLabel);
        script.appearAnimation = (CS_UIAnimator.AppearAnimation)EditorGUILayout.EnumPopup("Animation Type", script.appearAnimation);
        EditorGUILayout.Space();

        // Show fields based on Appear Animation selection
        if (script.appearAnimation == CS_UIAnimator.AppearAnimation.Grow)
        {
            EditorGUILayout.LabelField("Grow Animation", EditorStyles.boldLabel);
            script.grow_TargetScale = EditorGUILayout.Vector3Field("Target Scale", script.grow_TargetScale);
            script.grow_Duration = EditorGUILayout.FloatField("Duration", script.grow_Duration);
            script.grow_Delay = EditorGUILayout.FloatField("Delay", script.grow_Delay);
            script.grow_EaseType = (LeanTweenType)EditorGUILayout.EnumPopup("Ease Type", script.grow_EaseType);
        }
        else if (script.appearAnimation == CS_UIAnimator.AppearAnimation.MoveIn)
        {
            EditorGUILayout.LabelField("MoveIn Animation", EditorStyles.boldLabel);
            script.moveIn_XOffset = EditorGUILayout.FloatField("Move In X Units", script.moveIn_XOffset);
            script.moveIn_YOffset = EditorGUILayout.FloatField("Move In Y Units", script.moveIn_YOffset);
            script.moveIn_Duration = EditorGUILayout.FloatField("Duration", script.moveIn_Duration);
            script.moveIn_Delay = EditorGUILayout.FloatField("Delay", script.moveIn_Delay);
            script.moveIn_EaseType = (LeanTweenType)EditorGUILayout.EnumPopup("Ease Type", script.moveIn_EaseType);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Draw default Idle Animation option
        EditorGUILayout.LabelField("Idle Animation", EditorStyles.boldLabel);
        script.idleAnimation = (CS_UIAnimator.IdleAnimation)EditorGUILayout.EnumPopup("Animation Type", script.idleAnimation);
        EditorGUILayout.Space();

        // Show fields based on Idle Animation selection
        if (script.idleAnimation == CS_UIAnimator.IdleAnimation.Floating)
        {
            EditorGUILayout.LabelField("Floating Animation", EditorStyles.boldLabel);
            script.floating_PositionOffset = EditorGUILayout.FloatField("Position Offset", script.floating_PositionOffset);
            script.floating_Duration = EditorGUILayout.FloatField("Duration", script.floating_Duration);
            script.floating_EaseType = (LeanTweenType)EditorGUILayout.EnumPopup("Ease Type", script.floating_EaseType);
        }
        else if (script.idleAnimation == CS_UIAnimator.IdleAnimation.Pulsing)
        {
            EditorGUILayout.LabelField("Pulsing Animation", EditorStyles.boldLabel);
            script.pulsing_GrowScale = EditorGUILayout.Vector3Field("Grow Scale", script.pulsing_GrowScale);
            script.pulsing_Duration = EditorGUILayout.FloatField("Duration", script.pulsing_Duration);
            script.pulsing_EaseType = (LeanTweenType)EditorGUILayout.EnumPopup("Ease Type", script.pulsing_EaseType);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
        }
    }
}
#endif