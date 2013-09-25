namespace JPhysics.Unity
{
    using System;
    using System.Threading;
    using Dynamics;

    static class JPhysics
    {
        public readonly static World World = new World();
        public static int Count;

        static DateTime now;
        const int Wait = 10;
        const int Steps = 70;

        static JPhysics()
        {
            new Thread(Logic) { IsBackground = true }.Start();
        }

        static void Logic()
        {
            now = DateTime.Now;
            while (true)
            {
                if (!((DateTime.Now - now).TotalMilliseconds > Wait)) continue;
                World.Step(1f / Steps, true);
                now = DateTime.Now;
            }
        }

        public static void AddBody(RigidBody body)
        {
            World.AddBody(body);
            Count++;
        }
    }
}
