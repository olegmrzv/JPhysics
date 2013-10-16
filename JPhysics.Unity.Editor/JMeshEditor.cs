namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(JMesh)), CanEditMultipleObjects]
    internal class JMeshEditor : JColliderEditor
    {
        private SerializedProperty convex;

        public override void OnEnable()
        {
            base.OnEnable();
            convex = serializedObject.FindProperty("Convex");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(convex, new GUILayoutOption[0]);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
