namespace JPhysics.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Collision;
    using Dynamics;
    using UnityEngine;

    class JPhysics
    {
        public readonly static World World = new World(new CollisionSystemPersistentSAP());

        static DateTime now;

        const int Timestep = 20;
        const int Steps = 50;

        static readonly Vector3 V = new Vector3();
        static readonly Quaternion Q = new Quaternion();
        static readonly Dictionary<RigidBody, PhysicsCallback> BodDict = new Dictionary<RigidBody, PhysicsCallback>();

        static Thread thread;

        static JPhysics()
        {
            var go = new GameObject("JPhysics") {hideFlags = HideFlags.HideAndDontSave};
            go.AddComponent<JAssistant>().OnApplicationExit += OnApplicationExit;
            thread = new Thread(Logic) {IsBackground = true};
            thread.Start();
        }

        static void Logic()
        {
            now = DateTime.Now;
            while (true)
            {
                var del = (int)(DateTime.Now - now).TotalMilliseconds;
                if (del > Timestep) Debug.LogWarning("Physics calculation exceed timestep " + "Delta: " + del + " Timestep: " + Timestep);
                Thread.Sleep(del < Timestep ? Timestep - del : 1);
                now = DateTime.Now;
                World.Step(1f / Steps, true);
                UpdateTransforms();

            }
        }

        static void UpdateTransforms()
        {
            foreach (var body in BodDict)
            {
                body.Value(body.Key.position.ConvertToVector3(V), body.Key.orientation.ConvertToQuaternion(Q));
            }
        }

        public static void AddBody(RigidBody body, PhysicsCallback call)
        {
            World.AddBody(body);
            BodDict.Add(body, call);
        }

        static void OnApplicationExit()
        {
            thread.Abort();
        }

    }

    public delegate void PhysicsCallback(Vector3 p, Quaternion r);
}
