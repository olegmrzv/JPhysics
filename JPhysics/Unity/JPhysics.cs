namespace JPhysics.Unity
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Collision;
    using UnityEngine;

    [AddComponentMenu("JPhysics/JPhysics Core")]
    public class JPhysics : MonoBehaviour
    {
        public static bool Multithread = false;

        public static JPhysics Instance
        {
            get
            {
                CheckInstance();
                return instance;
            }
        }

        static JPhysics instance;
        static JSettings settings;

        static readonly List<JRigidbody> bodyPool = new List<JRigidbody>() ,
                                        corPool = new List<JRigidbody>(),
                                        bodyPoolToRemove = new List<JRigidbody>();

        public World World;

        Stopwatch sw = new Stopwatch();
        
        float timeScale = 1f;
        float timestep = .02f;

        Timer timer;
        Thread thread;
        bool stopThread;
        AutoResetEvent wait = new AutoResetEvent(false);

        void OnEnable()
        {
            instance = this;
            settings = Resources.Load("JSettings") as JSettings;
            CreateWorld();
            thread = new Thread(Logic);
            thread.Start();
            timer = new Timer(Loop, null, 0, (int)(timestep*1000));
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

        void Loop(object o)
        {
            wait.Set();
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
                CorrectBodies();
                AddBodies();
                RemoveBodies();
                World.Step(timestep, Multithread);
                //if(LastWorldCalculationTime > 80) UnityEngine.Debug.Log("LOL");
                wait.WaitOne();
            }
            World.Clear();
            Thread.CurrentThread.Abort();
        }

        void AddBodies()
        {
            lock (bodyPool)
            {
                var count = bodyPool.Count;
                if (count == 0) return;
                for (var i = 0; i < count; i++)
                {
                    var b = bodyPool[i];
                    World.AddBody(b.Body);
                    World.Events.PostStep += b.TransformUpdate;
                }
                bodyPool.Clear();
            }
        }

        void CorrectBodies()
        {
            lock (corPool)
            {
                var count = corPool.Count;
                if (count == 0) return;
                for (var i = 0; i < count; i++)
                {
                    var b = corPool[i];
                    b.Correct();
                    b.Body.inactiveTime = 0;
                }
                corPool.Clear();
            }
        }

        void RemoveBodies()
        {
            lock (bodyPoolToRemove)
            {
                var count = bodyPoolToRemove.Count;
                if (count == 0) return;
                for (var i = 0; i < count; i++)
                {
                    var b = bodyPoolToRemove[i];
                    World.RemoveBody(b.Body);
                    World.Events.PostStep -= b.TransformUpdate;
                }
                bodyPoolToRemove.Clear();
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
            lock (corPool)
            {
                if (!corPool.Contains(body)) corPool.Add(body);
            }
        }

        public static void AddBody(JRigidbody body)
        {
            CheckInstance();
            lock (bodyPool)
            {
                if (!bodyPool.Contains(body)) bodyPool.Add(body);
            }
        }

        public static void RemoveBody(JRigidbody body)
        {
            lock (bodyPoolToRemove)
            {
                if (!bodyPoolToRemove.Contains(body)) bodyPoolToRemove.Add(body);
            }
        }

    }
}
