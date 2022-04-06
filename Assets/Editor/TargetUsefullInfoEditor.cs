using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TargetCollisionManager))]
public class TargetUsefullInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();
        DrawDefaultInspector();

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Debug Variables");

        TargetCollisionManager targetManagerScript = (TargetCollisionManager)target;

        if (Application.isPlaying) {

            EditorGUILayout.Vector3Field("Initial Position", targetManagerScript._initial_position);

            EditorGUILayout.Vector3Field("Initial Rotation", targetManagerScript._initial_rotation.eulerAngles);



        }
    }
}
