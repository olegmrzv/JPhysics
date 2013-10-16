namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(JSphere)), CanEditMultipleObjects]
    internal class JSphereEditor : JColliderEditor
    {
        private SerializedProperty radius;

        public override void OnEnable()
        {
            base.OnEnable();
            radius = serializedObject.FindProperty("radius");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(radius, new GUILayoutOption[0]);
            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            var target = base.target as JSphere;
            Undo.SetSnapshotTarget(target, "Modified Sphere Collider");
            var color = Handles.color;

            Handles.color = target.enabled ? ColliderHandleColor : ColliderHandleColorDisabled;

            bool enabled = GUI.enabled;
            Vector3 lossyScale = target.transform.lossyScale;
            float a = Mathf.Abs(lossyScale.x);
            float introduced12 = Mathf.Max(a, Mathf.Abs(lossyScale.y));
            float num = Mathf.Max(introduced12, Mathf.Abs(lossyScale.z));
            float f = num * target.Radius;
            Vector3 position = target.transform.TransformPoint(Vector3.zero);
            Quaternion rotation = target.transform.rotation;
            f = Mathf.Max(Mathf.Abs(f), 1E-05f);
            DrawSphere(f, position,rotation);

            if (!Event.current.shift)
            {
                GUI.enabled = false;
                Handles.color = new Color(0f, 0f, 0f, 0.001f);
            }

            float num4 = Handles.RadiusHandle(rotation, position, f, true);
            if (GUI.changed)
            {
                target.Radius = (num4 * 1f) / num;
            }
            Handles.color = color;
            GUI.enabled = enabled;
        }

        static void DrawSphere(float radius, Vector3 position, Quaternion rotation)
        {
            var vectorArray = new[] { rotation * Vector3.right, rotation * Vector3.up, rotation * Vector3.forward, rotation * -Vector3.right, rotation * -Vector3.up, rotation * -Vector3.forward };
            float num4 = radius * radius;
            Vector3 forward = position - Camera.current.transform.position;
            float sqrMagnitude = forward.sqrMagnitude;
            float f = (num4 * num4) / sqrMagnitude;
            float y = Mathf.Sqrt(num4 - f);
            Handles.DrawWireDisc(position - (((num4 * forward) / sqrMagnitude)), forward, y);
            float num6 = f / num4;
            for (int k = 0; k < 3; k++)
            {
                if (num6 < 1f)
                {
                    float b = Vector3.Angle(forward, vectorArray[k]);
                    b = 90f - Mathf.Min(b, 180f - b);
                    float num10 = Mathf.Tan(b * 0.01745329f);
                    float num11 = Mathf.Sqrt(f + ((num10 * num10) * f)) / radius;
                    if (num11 < 1f)
                    {
                        float angle = Mathf.Asin(num11) * 57.29578f;
                        Vector3 from = Vector3.Cross(vectorArray[k], forward).normalized;
                        from = Quaternion.AngleAxis(angle, vectorArray[k]) * @from;
                        DrawTwoShadedWireDisc(position, vectorArray[k], from, (90f - angle) * 2f, radius);
                    }
                    else
                    {
                        DrawTwoShadedWireDisc(position, vectorArray[k], radius);
                    }
                }
                else
                {
                    DrawTwoShadedWireDisc(position, vectorArray[k], radius);
                }
            }
        }

        static void DrawTwoShadedWireDisc(Vector3 position, Vector3 axis, float radius)
        {
            var color = Handles.color;
            var color2 = color;
            color.a *= 0.2f;
            Handles.color = color;
            Handles.DrawWireDisc(position, axis, radius);
            Handles.color = color2;
        }

        static void DrawTwoShadedWireDisc(Vector3 position, Vector3 axis, Vector3 from, float degrees, float radius)
        {
            Handles.DrawWireArc(position, axis, from, degrees, radius);
            Color color = Handles.color;
            Color color2 = color;
            color.a *= 0.2f;
            Handles.color = color;
            Handles.DrawWireArc(position, axis, from, degrees - 360f, radius);
            Handles.color = color2;
        }

    }
}
