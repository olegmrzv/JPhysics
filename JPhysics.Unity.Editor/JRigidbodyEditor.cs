namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    abstract class JRigidbodyEditor : Editor
    {
        protected SerializedProperty isStatic;
        protected SerializedProperty mass;

        protected static Color ColliderHandleColor = new Color(145f, 244f, 139f, 210f) / 255f;
        protected static Color ColliderHandleColorDisabled = new Color(84f, 200f, 77f, 140f) / 255f;

        public virtual void OnEnable()
        {
            mass = serializedObject.FindProperty("Mass");
            isStatic = serializedObject.FindProperty("IsStatic");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(isStatic, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(mass, new GUILayoutOption[0]);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
