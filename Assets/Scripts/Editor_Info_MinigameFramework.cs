#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CS_Info_MinigameFramework))]
public class Editor_Info_MinigameFramework : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle titleStyle = new(EditorStyles.boldLabel)
        {
            fontSize = 30,
            alignment = TextAnchor.MiddleCenter
        };
        GUIStyle headerStyle = new(EditorStyles.whiteLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft
        };
        GUIStyle textStyle = new(EditorStyles.whiteLabel)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleLeft
        };

        GUILayout.Space(10);

        GUILayout.Label("DO NOT\nAPPLY OVERRIDES\nTO THIS PREFAB!!!", titleStyle);

        GUILayout.Space(25);

        GUILayout.Label("The only things you should change: ", headerStyle);
        DrawUnderline(2f, 4f, 310f);

        GUILayout.Space(5);

        GUILayout.Label("- Prefabs for Human and Computer GameObjects \n" +
                        "- Minigame Startup Screen Components \n" +      
                        "- Player Start Positions \n" +
                        "- Main Camera for Game View \n" +
                        "- Cutscenes with Assets and Animations \n" +
                        "- Music for the Gameplay \n" +
                        "", textStyle);
    }

    private void DrawUnderline(float thickness, float padding, float width)
    {
        Rect controlRect = EditorGUILayout.GetControlRect(false, thickness + padding);
        float x = controlRect.x;
        float y = controlRect.y + (padding / 2f);
        Rect lineRect = new(x, y, width, thickness);
        EditorGUI.DrawRect(lineRect, UnityEngine.Color.gray);
    }
}
#endif