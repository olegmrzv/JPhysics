namespace JPhysics.Unity
{
    using UnityEngine;
    public class JSettings : ScriptableObject
    {
        public bool Multithreading = true;
        public float Timestep = 15;
        public int Steps = 75;
    }
}
