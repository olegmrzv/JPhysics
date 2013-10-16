

namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    abstract class JColliderEditor : Editor
    {
        protected static readonly Color ColliderHandleColor = new Color(145f, 244f, 139f, 210f) / 255f;
        protected static readonly Color ColliderHandleColorDisabled = new Color(84f, 200f, 77f, 140f) / 255f;

        SerializedProperty mat, isTrigger;

        public virtual void OnEnable()
        {
            isTrigger = serializedObject.FindProperty("IsTrigger");
            mat = serializedObject.FindProperty("material");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(isTrigger, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(mat, new GUILayoutOption[0]);
            if (serializedObject.ApplyModifiedProperties())
            {
                var t = target as JRigidbody;
                if(t != null)t.UpdateVariables();
            }
        }
    }
}
