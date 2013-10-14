namespace JPhysics.Unity.Aides
{
    using UnityEngine;

    [AddComponentMenu("JPhysics/Aides/Debug Info")]
    class DebugInfo : MonoBehaviour
    {
        double lastTime;
        string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
            if (Input.GetKeyDown(KeyCode.R)) Application.LoadLevel(Application.loadedLevel);
            lastTime = JPhysics.Instance.LastWorldCalculationTime;
        }

        void OnGUI()
        {
            GUILayout.Label("Summary physics time: " + lastTime + " ms");
            GUILayout.Label("Count objects: " + JPhysics.Instance.World.RigidBodies.Count);
            GUILayout.Label("Version JPhysics: " + version);
        }
    }
}
