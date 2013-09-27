namespace JPhysics.Unity.Aides
{
    using System.Collections.Generic;
    using System.Linq;
    using Dynamics;
    using UnityEngine;

    [AddComponentMenu("JPhysics/Aides/Debug Info")]
    class DebugInfo : MonoBehaviour
    {
        double lastTime;
        string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
            if (Input.GetKeyDown(KeyCode.M)) JPhysics.Multithread = !JPhysics.Multithread;
            if (Input.GetKeyDown(KeyCode.R)) Application.LoadLevel(Application.loadedLevel);
            lastTime = JPhysics.World.DebugTimes[(int)World.DebugType.CollisionDetect];
        }

        void OnGUI()
        {
            GUILayout.Label("Summary physics time: " + lastTime + " ms");
            GUILayout.Label("Count objects: " + JPhysics.World.RigidBodies.Count);
            GUILayout.Label("Count active objects: " + JPhysics.World.RigidBodies.ToList().Count(rigidBody => (rigidBody).IsActive));
            GUILayout.Label("Mode multithread: " + JPhysics.Multithread);
            GUILayout.Label("Version JPhysics: " + version);
        }
    }
}
