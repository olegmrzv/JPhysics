namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    abstract class JRigidbodyEditor : Editor
    {
        protected SerializedProperty isStatic;
        protected SerializedProperty mass;

        public virtual void OnEnable()
        {
            mass = serializedObject.FindProperty("Mass");
            isStatic = serializedObject.FindProperty("IsStatic");
        }

        public override void OnInspectorGUI()
        {
            base.serializedObject.Update();
            EditorGUILayout.PropertyField(isStatic, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(mass, new GUILayoutOption[0]);
            base.serializedObject.ApplyModifiedProperties();
        }
    }
}
