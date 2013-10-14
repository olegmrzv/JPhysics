namespace JPhysics.Unity
{
    using System;
    using UnityEngine;
    public class JSettings : ScriptableObject
    {
        [HideInInspector]
        public MultithreadSettings Multithread;
        public WorldSettings World;
        public CollisionSystemSettings CollisionSystem;
        public InactivityThresholdSettings InactivityThreshold;
        public RigidbodySettings Rigidbody;

        [Serializable]
        public class CollisionSystemSettings
        {
            public CollisionSystemEnum CollisionSystem = CollisionSystemEnum.SAP;
            public bool EnableSpeculativeContacts = true;
        }

        [Serializable]
        public class WorldSettings
        {
            public bool AllowDeactivation = true;
            public float Timestep = .02f;
        }

        [Serializable]
        public class MultithreadSettings
        {
            public bool Mode = false;
            public int ThreadsPerProcessor = 1;
        }

        [Serializable]
        public class InactivityThresholdSettings
        {
            public float MinAngularVelocity = .1f;
            public float MinLinearVelocity = .1f;
            public float MinSleepingTime = 1f;
        }

        [Serializable]
        public enum CollisionSystemEnum
        {
            SAP, PersistentSAP, Brute
        }

        [Serializable]
        public class RigidbodySettings
        {
            public float LerpFactor = .3f;
        }
    }
}
