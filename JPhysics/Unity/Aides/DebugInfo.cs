namespace JPhysics.Unity.Aides
{
    using System.Linq;
    using UnityEngine;

    class DebugInfo : MonoBehaviour
    {
        double lastTime;

        void FixedUpdate()
        {
            lastTime = JPhysics.World.DebugTimes.Sum();
        }

        void OnGUI()
        {
            GUILayout.Label("Summary physics time: " + lastTime + " ms");
        }
    }
}
