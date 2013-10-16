namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CanEditMultipleObjects, CustomEditor(typeof(JRigidbody))]
    class JRigidbodyEditor : Editor
    {
        SerializedProperty isStatic, mass, grav, damp;

        public void OnEnable()
        {
            mass = serializedObject.FindProperty("mass");
            isStatic = serializedObject.FindProperty("isStatic");
            grav = serializedObject.FindProperty("useGravity");
            damp = serializedObject.FindProperty("damping");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(isStatic, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(mass, new GUILayoutOption[0]);

            EditorGUILayout.PropertyField(grav, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(damp, new GUILayoutOption[0]);
            if (serializedObject.ApplyModifiedProperties())
            {
                (target as JRigidbody).UpdateVariables();
            }
        }
    }
}
