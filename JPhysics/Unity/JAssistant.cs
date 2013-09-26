namespace JPhysics.Unity
{
    using System;
    using UnityEngine;

    class JAssistant : MonoBehaviour
    {
        public event Action OnApplicationExit;

        void OnApplicationQuit()
        {
            if(OnApplicationExit != null) OnApplicationExit();
        }
    }
}
