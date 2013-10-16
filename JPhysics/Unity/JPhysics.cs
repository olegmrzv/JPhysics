namespace JPhysics.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Collision;
    using Collision.Shapes;
    using Dynamics;
    using LinearMath;
    using UnityEngine;

    [AddComponentMenu("JPhysics/Core"), ExecuteInEditMode]
    public class JPhysics : MonoBehaviour
    {
        public static bool Multithread = false;
        public static readonly Dictionary<RigidBody, JRigidbody> Bodies = new Dictionary<RigidBody, JRigidbody>(); 

        public static JPhysics Instance
        {
            get
            {
                CheckInstance();
                return instance;
            }
        }

        public static bool IsInstance
        {
            get
            {
                return instance != null;
            }
        }

        public bool IsPaused;

        public event Action PostStep;

        static JPhysics instance;
        static JSettings settings;

        static readonly List<JRigidbody> BodyPool = new List<JRigidbody>() ,
                                        CorPool = new List<JRigidbody>(),
                                        BodyPoolToRemove = new List<JRigidbody>();

        public World World;

        Stopwatch sw = new Stopwatch();
        
        float lastTimeScale = 1f;
        float timestep = .02f;

        Timer timer;
        Thread thread;
        bool stopThread;
        AutoResetEvent wait = new AutoResetEvent(false);

        void Awake()
        {
            instance = this;
            settings = Resources.Load("JSettings") as JSettings;
            CreateWorld();
            thread = new Thread(Logic);
            timer = new Timer(Loop, null, 0, (int)(timestep*1000));
        }

        void Start()
        {
            thread.Start();
        }

        void OnApplicationQuit()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
                if (thread != null) stopThread = true;
            }
        }

        void OnDestroy()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
                if (thread != null) stopThread = true;
            }
        }

        void Loop(object o)
        {
            if (!IsPaused || !Application.isEditor)
            {
                wait.Set();
            }
        }

        void CreateWorld()
        {
            CollisionSystem collision = null;
            switch (settings.CollisionSystem.CollisionSystem)
            {
                case JSettings.CollisionSystemEnum.SAP: collision = new CollisionSystemSAP(); break;
                case JSettings.CollisionSystemEnum.PersistentSAP: collision = new CollisionSystemPersistentSAP(); break;
                case JSettings.CollisionSystemEnum.Brute: collision = new CollisionSystemBrute(); break;
            }
            collision.EnableSpeculativeContacts = settings.CollisionSystem.EnableSpeculativeContacts;
            World = new World(collision);
            World.SetInactivityThreshold(settings.InactivityThreshold.MinAngularVelocity,
                                            settings.InactivityThreshold.MinLinearVelocity,
                                                    settings.InactivityThreshold.MinSleepingTime);
            World.AllowDeactivation = settings.World.AllowDeactivation;
            Multithread = settings.Multithread.Mode;
            ThreadManager.ThreadsPerProcessor = settings.Multithread.ThreadsPerProcessor;
            timestep = settings.World.Timestep;
            JRigidbody.LerpFactor = settings.Rigidbody.LerpFactor;
        }

        void Logic()
        {
            while (!stopThread)
            {
                wait.WaitOne();
                CorrectBodies();
                AddBodies();
                RemoveBodies();
                World.Step(timestep, Multithread);
                if (PostStep != null) PostStep();

            }
            World.Clear();
            Thread.CurrentThread.Abort();
        }

        void AddBodies()
        {
            lock (BodyPool)
            {
                var count = BodyPool.Count;
                if (count == 0) return;
                for (var i = 0; i < count; i++)
                {
                    var b = BodyPool[i];
                    World.AddBody(b.Body);
                    World.Events.PostStep += b.TransformUpdate;
                }
                BodyPool.Clear();
            }
        }

        void CorrectBodies()
        {
            lock (CorPool)
            {
                var count = CorPool.Count;
                if (count == 0) return;
                for (var i = 0; i < count; i++)
                {
                    var b = CorPool[i];
                    b.Correct();
                    b.Body.inactiveTime = 0;
                }
                CorPool.Clear();
            }
        }

        void RemoveBodies()
        {
            lock (BodyPoolToRemove)
            {
                var count = BodyPoolToRemove.Count;
                if (count == 0) return;
                for (var i = 0; i < count; i++)
                {
                    var b = BodyPoolToRemove[i];
                    World.RemoveBody(b.Body);
                    World.Events.PostStep -= b.TransformUpdate;
                }
                BodyPoolToRemove.Clear();
            }
        }

        void Update()
        {
            var t = Time.timeScale;
            if (lastTimeScale != t)
            {
                UnityEngine.Debug.Log(t);
                timer.Change(0, (int)(timestep*1000*t));
                lastTimeScale = t;
            }
        }

        static void CheckInstance()
        {
            if (instance == null && Application.isPlaying)
            {
                instance = new GameObject("JPhysics", typeof(JPhysics)).GetComponent<JPhysics>();
                instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Debug.Log("Phisics Manager missing. New manager Instance has been created.");
            }
        }

        public static void Correct(JRigidbody body)
        {
            lock (CorPool)
            {
                if (!CorPool.Contains(body)) CorPool.Add(body);
            }
        }

        public static void AddBody(JRigidbody body)
        {
            CheckInstance();
            lock (BodyPool)
            {
                if (!BodyPool.Contains(body)) BodyPool.Add(body);
            }
            lock (Bodies) if (!Bodies.ContainsValue(body)) Bodies.Add(body.Body, body);
        }

        public static void RemoveBody(JRigidbody body)
        {
            lock (BodyPoolToRemove)
            {
                if (!BodyPoolToRemove.Contains(body)) BodyPoolToRemove.Add(body);
            }
            lock (Bodies) if (!Bodies.ContainsValue(body)) Bodies.Remove(body.Body);
        }

        //TODO make this threadsafe. Dangerous code
        public static bool Raycast(Vector3 origin, Vector3 direction, out JRigidbody body, out Vector3 normal, out float distance)
        {
            body = null;
            normal = Vector3.zero;
            distance = 0;
            if (IsInstance)
            {
                RigidBody b;
                JVector n;
                var v =  Instance.World.CollisionSystem.Raycast(origin.ConvertToJVector(), direction.ConvertToJVector(),
                                                              null, out b, out n, out distance);
                if (b != null && Bodies.ContainsKey(b))
                {
                    body = Bodies[b];
                    if (body != null)
                    {
                        normal = n;
                        return v;
                    }
                }
                return false;
            }
            return false;
        }

        public static JRigidbody[] OverlapMesh(Shape shape, Vector3 position, Quaternion rotation)
        {
            var list = new List<JRigidbody>();
            JMatrix orientation = rotation.ConvertToJMatrix(), otherOrientation;
            JVector pos = position.ConvertToJVector(),otherPosition, point, normal;
            float penetration;
            foreach (var body in Bodies)
            {
                if (body.Key.Shape is Multishape) continue;

                otherPosition = body.Key.Position;
                otherOrientation = body.Key.Orientation;
                bool collide = XenoCollide.Detect(shape, body.Key.Shape, ref orientation, ref otherOrientation,
                                                  ref pos, ref otherPosition, out point, out normal,
                                                  out penetration);
                if(collide) list.Add(body.Value);
            }
            return list.ToArray();
        }

    }
}
