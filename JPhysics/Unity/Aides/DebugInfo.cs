namespace JPhysics.Unity.Aides
{
    using System.Linq;
    using UnityEngine;

    class DebugInfo : MonoBehaviour
    {
        double lastTime;
        string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        void FixedUpdate()
        {
            lastTime = JPhysics.World.DebugTimes.Sum();

        }

        void OnGUI()
        {
            GUILayout.Label("Summary physics time: " + lastTime + " ms");
            GUILayout.Label("Version JPhysics: " + version);
        }
    }
}
