namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CanEditMultipleObjects, CustomEditor(typeof(JCapsule))]
    class JCapsuleEditor : JColliderEditor
    {
        SerializedProperty radius, height, direction;

        public override void OnEnable()
        {
            base.OnEnable();
            radius = serializedObject.FindProperty("radius");
            height = serializedObject.FindProperty("height");
            //direction = serializedObject.FindProperty("Direction");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            //EditorGUILayout.PropertyField(base.m_IsTrigger, new GUILayoutOption[0]);
            //EditorGUILayout.PropertyField(base.m_Material, new GUILayoutOption[0]);
            //EditorGUILayout.PropertyField(this.m_Center, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(radius, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(height, new GUILayoutOption[0]);
            //EditorGUILayout.PropertyField(direction, new GUILayoutOption[0]);
            serializedObject.ApplyModifiedProperties();
        }

        //public void OnSceneGUI()
        //{
        //    var target = (JCapsule) base.target;
        //    Color color = Handles.color;
        //    if (target.enabled)
        //    {
        //        Handles.color = ColliderHandleColor;
        //    }
        //    else
        //    {
        //        Handles.color = ColliderHandleColorDisabled;
        //    }
        //    bool enabled = GUI.enabled;
        //    if (!Event.current.shift)
        //    {
        //        GUI.enabled = false;
        //        Handles.color = new Color(1f, 0f, 0f, 0.001f);
        //    }
        //    Vector3 capsuleExtents = ColliderUtil.GetCapsuleExtents(target);
        //    float num = capsuleExtents.y + (2f * capsuleExtents.x);
        //    float x = capsuleExtents.x;
        //    Matrix4x4 matrix = ColliderUtil.CalculateCapsuleTransform(target);
        //    int hotControl = GUIUtility.hotControl;
        //    Vector3 localPos = (Vector3) ((Vector3.up * num) * 0.5f);
        //    float num4 = SizeHandle(localPos, Vector3.up, matrix, true);
        //    if (!GUI.changed)
        //    {
        //        num4 = SizeHandle(-localPos, Vector3.down, matrix, true);
        //    }
        //    if (GUI.changed)
        //    {
        //        float num5 = num / target.height;
        //        target.height += num4 / num5;
        //    }
        //    num4 = SizeHandle((Vector3) (Vector3.left * x), Vector3.left, matrix, true);
        //    if (!GUI.changed)
        //    {
        //        num4 = SizeHandle((Vector3) (-Vector3.left * x), -Vector3.left, matrix, true);
        //    }
        //    if (!GUI.changed)
        //    {
        //        num4 = SizeHandle((Vector3) (Vector3.forward * x), Vector3.forward, matrix, true);
        //    }
        //    if (!GUI.changed)
        //    {
        //        num4 = SizeHandle((Vector3) (-Vector3.forward * x), -Vector3.forward, matrix, true);
        //    }
        //    if (GUI.changed)
        //    {
        //        float num6 = Mathf.Max((float) (capsuleExtents.z / target.radius), (float) (capsuleExtents.x / target.radius));
        //        target.radius += num4 / num6;
        //    }
        //    if ((hotControl != GUIUtility.hotControl) && (GUIUtility.hotControl != 0))
        //    {
        //        this.m_HandleControlID = GUIUtility.hotControl;
        //    }
        //    if (GUI.changed)
        //    {
        //        target.radius = Mathf.Max(target.radius, 1E-05f);
        //        target.height = Mathf.Max(target.height, 1E-05f);
        //    }
        //    Handles.color = color;
        //    GUI.enabled = enabled;
        //}

        private static float SizeHandle(Vector3 localPos, Vector3 localPullDir, Matrix4x4 matrix, bool isEdgeHandle)
        {
            float num3;
            Vector3 rhs = matrix.MultiplyVector(localPullDir);
            Vector3 position = matrix.MultiplyPoint(localPos);
            float handleSize = HandleUtility.GetHandleSize(position);
            bool changed = GUI.changed;
            GUI.changed = false;
            Color color = Handles.color;
            float num2 = 0f;
            if (isEdgeHandle)
            {
                num2 = Mathf.Cos(0.7853982f);
            }
            if (Camera.current.isOrthoGraphic)
            {
                num3 = Vector3.Dot(-Camera.current.transform.forward, rhs);
            }
            else
            {
                Vector3 vector4 = Camera.current.transform.position - position;
                num3 = Vector3.Dot(vector4.normalized, rhs);
            }
            if (num3 < -num2)
            {
                Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, Handles.color.a * .2f);
            }
            Vector3 point = Handles.Slider(position, rhs, handleSize * 0.03f, new Handles.DrawCapFunction(Handles.DotCap), 0f);
            float num4 = 0f;
            if (GUI.changed)
            {
                num4 = HandleUtility.PointOnLineParameter(point, position, rhs);
            }
            GUI.changed |= changed;
            Handles.color = color;
            return num4;
        }
    }
}
