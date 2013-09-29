namespace JPhysics.Unity
{
    using UnityEngine;
    public class JSettings : ScriptableObject
    {
        public bool Multithreading = true;
        public int ThreadsPerProcessor = 1;
        public float Timestep = 15;
        public int Steps = 75;
    }
}
