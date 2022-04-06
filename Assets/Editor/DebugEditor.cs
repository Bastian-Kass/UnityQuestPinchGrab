using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugManager))]
public class GameBallInspector : Editor
{

    public GameManagerScript gmScript;

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();
        DrawDefaultInspector();

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Debug Variables");

        DebugManager debugManagerScript = (DebugManager)target;

        if (Application.isPlaying) {

            GameManagerScript manager = FindObjectOfType<GameManagerScript>();

            EditorGUILayout.LabelField("Slow Motion", debugManagerScript.SlowMode.ToString());

            if(GUILayout.Button("CheatMode"))
            {
                manager.ToggleCheatMode();
            }

            if(GUILayout.Button("SlowMode"))
            {
                debugManagerScript.ToggleSlowMotion();
            }

            EditorGUILayout.Separator();
            if(GUILayout.Button("Throw Ball"))
            {
                debugManagerScript.ThrowDebugBall();
            }

        }
    }
}
