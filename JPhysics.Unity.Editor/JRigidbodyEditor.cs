namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    abstract class JRigidbodyEditor : Editor
    {
        protected SerializedProperty isStatic, mass,grav, mat, damp;

        protected static readonly Color ColliderHandleColor = new Color(145f, 244f, 139f, 210f) / 255f;
        protected static readonly Color ColliderHandleColorDisabled = new Color(84f, 200f, 77f, 140f) / 255f;

        public virtual void OnEnable()
        {
            mass = serializedObject.FindProperty("Mass");
            isStatic = serializedObject.FindProperty("IsStatic");
            grav = serializedObject.FindProperty("UseGravity");
            mat = serializedObject.FindProperty("Material");
            damp = serializedObject.FindProperty("Damping");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(isStatic, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(mass, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(mat, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(grav, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(damp, new GUILayoutOption[0]);
            if (serializedObject.ApplyModifiedProperties())
            {
                (target as JRigidbody).UpdateVariables();
            }
        }
    }
}
