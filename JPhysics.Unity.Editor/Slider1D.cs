namespace JPhysics.Unity.Editor
{
    using UnityEditor;
    using UnityEngine;

    //WARNING: This code is taken from UnityEditor, only God himself knows how it works

    internal class Slider1D
    {
        private static Vector2 s_CurrentMousePosition;
        private static Vector2 s_StartMousePosition;
        private static Vector3 s_StartPosition;

        internal static Vector3 Do(int id, Vector3 position, Vector3 direction, float size, Handles.DrawCapFunction drawFunc, float snap)
        {
            Event current = Event.current;
            switch (current.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if ((((HandleUtility.nearestControl == id) && (current.button == 0)) || ((GUIUtility.keyboardControl == id) && (current.button == 2))) && (GUIUtility.hotControl == 0))
                    {
                        int num2 = id;
                        GUIUtility.keyboardControl = num2;
                        GUIUtility.hotControl = num2;
                        s_CurrentMousePosition = s_StartMousePosition = current.mousePosition;
                        s_StartPosition = position;
                        Undo.CreateSnapshot();
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    return position;

                case EventType.MouseUp:
                    if ((GUIUtility.hotControl == id) && ((current.button == 0) || (current.button == 2)))
                    {
                        GUIUtility.hotControl = 0;
                        Undo.RegisterSnapshot();
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(0);
                    }
                    return position;

                case EventType.MouseMove:
                case EventType.KeyUp:
                case EventType.ScrollWheel:
                    return position;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        s_CurrentMousePosition += current.delta;
                        float num = Handles.SnapValue(HandleUtility.CalcLineTranslation(s_StartMousePosition, s_CurrentMousePosition, s_StartPosition, direction), snap);
                        Vector3 vector = Handles.matrix.MultiplyVector(direction);
                        Vector3 v = Handles.matrix.MultiplyPoint(s_StartPosition) + ((Vector3)(vector * num));
                        position = Handles.matrix.inverse.MultiplyPoint(v);
                        GUI.changed = true;
                        current.Use();
                    }
                    return position;

                case EventType.KeyDown:
                    if ((GUIUtility.hotControl == id) && (current.keyCode == KeyCode.Escape))
                    {
                        position = s_StartPosition;
                        GUIUtility.hotControl = 0;
                        GUI.changed = true;
                        current.Use();
                    }
                    return position;

                case EventType.Repaint:
                    {
                        Color white = Color.white;
                        if ((id == GUIUtility.keyboardControl) && GUI.enabled)
                        {
                            white = Handles.color;
                            Handles.color = new Color(0.9647059f, 0.9490196f, 0.1960784f, 0.89f);
                        }
                        drawFunc(id, position, Quaternion.LookRotation(direction), size);
                        if (id == GUIUtility.keyboardControl)
                        {
                            Handles.color = white;
                        }
                        return position;
                    }
                case EventType.Layout:
                    if (drawFunc != new Handles.DrawCapFunction(Handles.ArrowCap))
                    {
                        HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(position, size * 0.2f));
                        return position;
                    }
                    HandleUtility.AddControl(id, HandleUtility.DistanceToLine(position, position + ((Vector3)(direction * size))));
                    HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(position + ((Vector3)(direction * size)), size * 0.2f));
                    return position;
            }
            return position;
        }
    }
}
