namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CanEditMultipleObjects, CustomEditor(typeof (JBox))]
    internal class JBoxEditor : JRigidbodyEditor
    {
        readonly BoxEditor boxEditor = new BoxEditor(true);
        SerializedProperty size;

        public override void OnEnable()
        {
            base.OnEnable();
            size = serializedObject.FindProperty("Size");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(size, new GUILayoutOption[0]);
            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            var target = base.target as JBox;
            Undo.SetSnapshotTarget(target, "Modified JBox Collider");
            var center = new Vector3();
            Vector3 size = target.Size;
            var color = ColliderHandleColor;
            if (!target.enabled)
            {
                color = ColliderHandleColorDisabled;
            }
            if (boxEditor.OnSceneGUI(target.transform, color, ref center, ref size))
            {
                target.Size = size;
            }
        }
    }
}
