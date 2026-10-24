#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomizedPlayer))]
public class Editor_SquirrelCustomized : Editor
{
    SerializedProperty customizationReference;
    SerializedProperty computerPlayer;
    SerializedProperty playerID;
    SerializedProperty hatOption;
    SerializedProperty bodyOption;
    SerializedProperty randomizeCustomizations;
    SerializedProperty squirrelHats;
    SerializedProperty squirrelBodies;
    SerializedProperty squirrelBodyParts;

    void OnEnable()
    {
        customizationReference = serializedObject.FindProperty("customizationReference");
        computerPlayer = serializedObject.FindProperty("computerPlayer");
        playerID = serializedObject.FindProperty("playerID");
        hatOption = serializedObject.FindProperty("hatOption");
        bodyOption = serializedObject.FindProperty("bodyOption");
        randomizeCustomizations = serializedObject.FindProperty("randomizeCustomizations");
        squirrelHats = serializedObject.FindProperty("squirrelHats");
        squirrelBodies = serializedObject.FindProperty("squirrelBodies");
        squirrelBodyParts = serializedObject.FindProperty("squirrelBodyParts");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(customizationReference);
        EditorGUILayout.Space(1);

        switch ((CustomizedPlayer.CustomizationReference)customizationReference.enumValueIndex)
        {
            case CustomizedPlayer.CustomizationReference.Controller:
                EditorGUILayout.PropertyField(computerPlayer);
                break;

            case CustomizedPlayer.CustomizationReference.Player:
                EditorGUILayout.PropertyField(playerID);
                break;

            case CustomizedPlayer.CustomizationReference.NPC:
                EditorGUILayout.PropertyField(randomizeCustomizations);
                EditorGUILayout.IntSlider(hatOption, 0, CustomizationList.numHatOptions - 1);
                EditorGUILayout.IntSlider(bodyOption, 0, CustomizationList.numBodyOptions - 1);
                break;
        }

        DrawHorizontalLine();

        EditorGUILayout.Space(1);
        EditorGUILayout.PropertyField(squirrelHats);
        EditorGUILayout.PropertyField(squirrelBodies);

        EditorGUILayout.Space(1);
        EditorGUILayout.PropertyField(squirrelBodyParts, true);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHorizontalLine()
    {
        float thickness = 1f;
        float padding = 20f;

        Rect rect = EditorGUILayout.GetControlRect(false, thickness + padding);
        rect.height = thickness;
        rect.y += padding;
        EditorGUI.DrawRect(rect, Color.grey);
    }
}
#endif