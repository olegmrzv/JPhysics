namespace JPhysics.Unity
{
    using System;
    using UnityEngine;

    [ExecuteInEditMode]
    class JAssistant : MonoBehaviour
    {
        public event Action OnApplicationExit, StartEvent;
        float lastScale;

        void Start()
        {
            if (StartEvent != null) StartEvent();
        }

        void Update()
        {
            if (lastScale != Time.timeScale)
                JPhysics.TimeScale = lastScale = Time.timeScale;
        }

        void OnApplicationQuit()
        {
            if(OnApplicationExit != null) OnApplicationExit();
        }
    }
}
