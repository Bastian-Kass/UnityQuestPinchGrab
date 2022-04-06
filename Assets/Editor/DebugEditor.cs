using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugManager))]
public class GameBallInspector : Editor
{
    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();
        DrawDefaultInspector();

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Debug Variables");

        DebugManager debugManagerScript = (DebugManager)target;

        if (Application.isPlaying) {



            EditorGUILayout.LabelField("Slow Motion", debugManagerScript.SlowMode.ToString());

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
