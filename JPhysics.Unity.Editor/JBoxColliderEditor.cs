namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CanEditMultipleObjects, CustomEditor(typeof (JBoxCollider))]
    internal class JBoxColliderEditor : Editor
    {
        readonly BoxEditor boxEditor = new BoxEditor(true);
        SerializedProperty size;

        public void OnEnable()
        {
            size = serializedObject.FindProperty("Size");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(size, new GUILayoutOption[0]);
            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            var target = base.target as JBoxCollider;
            Undo.SetSnapshotTarget(target, "Modified JBox Collider");
            var center = new Vector3();
            Vector3 size = target.Size;
            var color = new Color(145f, 244f, 139f, 210f) / 255f;
            if (!target.enabled)
            {
                color = new Color(84f, 200f, 77f, 140f) / 255f;
            }
            if (boxEditor.OnSceneGUI(target.transform, color, ref center, ref size))
            {
                target.Size = size;
            }
        }
    }
}
