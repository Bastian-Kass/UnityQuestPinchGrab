using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DebugWindow : EditorWindow
{
    [MenuItem ("Window/DebugWindow")]

    public static void  ShowWindow () {
        EditorWindow.GetWindow(typeof(DebugWindow));
    }

    void OnGUI () {

        if (Application.isPlaying) {

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Debug Variables");

            GameManagerScript gameManagerScript = FindObjectOfType<GameManagerScript>();
            DebugManager debugManagerScript = FindObjectOfType<DebugManager>();

            EditorGUILayout.LabelField("Slow Motion", debugManagerScript.SlowMode.ToString());

            if(GUILayout.Button("CheatMode"))
            {
                gameManagerScript.ToggleCheatMode();
            }

            if(GUILayout.Button("SlowMode"))
            {
                debugManagerScript.ToggleSlowMotion();
            }

            if(GUILayout.Button("Throw Ball"))
            {
                debugManagerScript.ThrowDebugBall();
            }

        } else{
            EditorGUILayout.LabelField("Start the game to look at controls");

        }
    }
}
