namespace JPhysics.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Dynamics;
    using UnityEngine;

    public static class JPhysics
    {
        public readonly static World World = new World();
        public static float TimeScale = 1f;
        public static bool Multithread = true;

        static DateTime now;

        static float timestep = 15;
        static int steps = 75;

        static readonly Vector3 V = new Vector3();
        static readonly Quaternion Q = new Quaternion();
        static readonly Dictionary<RigidBody, PhysicsCallback> BodDict = new Dictionary<RigidBody, PhysicsCallback>();
        static readonly Dictionary<RigidBody, PhysicsCorrect> CorDict = new Dictionary<RigidBody, PhysicsCorrect>();

        static readonly Thread thread;
        static readonly GameObject go;
        static readonly Stopwatch sw = new Stopwatch();

        static JSettings settings;

        static JPhysics()
        {
            settings = Resources.Load("JSettings") as JSettings;
            SetValues();
            go = new GameObject("JPhysics") {hideFlags = HideFlags.HideAndDontSave};
            var a = go.AddComponent<JAssistant>();
            a.OnApplicationExit += OnApplicationExit;
            a.StartEvent += Start;
            thread = new Thread(Logic) {IsBackground = true};
        }

        static void SetValues()
        {
            Multithread = settings.Multithreading;
            timestep = settings.Timestep;
            steps = settings.Steps;
        }

        static void Start()
        {
            thread.Start();
        }

        static void Logic()
        {
            while (true)
            {
                var t = timestep/TimeScale;
                var del = (float)sw.ElapsedMilliseconds;
                if (del > t) UnityEngine.Debug.LogWarning("Multithread " + Multithread +" Physics calculation exceed timestep " + "Delta: " + del + "ms Timestep: " + t + "ms");
                sw.Reset();
                Thread.Sleep((int)(del < t ? t - del : 0));
                sw.Start();
                CorrectTransforms();
                World.Step(1f / steps, Multithread);
                UpdateTransforms();
                sw.Stop();
            }
        }

        static void CorrectTransforms()
        {
            lock (CorDict)
            {
                foreach (var physicsCorrect in CorDict)
                {
                    var body = physicsCorrect.Key;
                    var correct = physicsCorrect.Value();
                    body.Position = ((Vector3) correct[0]).ConvertToJVector();
                    body.Orientation = ((Quaternion) correct[1]).ConvertToJMatrix();
                    body.inactiveTime = 0;
                }
                CorDict.Clear();
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

        public static void RemoveBody(RigidBody body)
        {
            World.RemoveBody(body);
            BodDict.Remove(body);
        }

        public static void CorrectTransform(RigidBody body, PhysicsCorrect correct)
        {
            lock (CorDict)
            {
                if(CorDict.ContainsKey(body)) CorDict.Remove(body);
                CorDict.Add(body, correct);
            }
        }

        static void OnApplicationExit()
        {
            thread.Abort();
            UnityEngine.Object.DestroyImmediate(go);
        }

    }

    public delegate void PhysicsCallback(Vector3 p, Quaternion r);
    public delegate object[] PhysicsCorrect();
}
