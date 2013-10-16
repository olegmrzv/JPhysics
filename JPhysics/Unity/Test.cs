namespace JPhysics.Unity
{
    using System.Collections.Generic;
    using UnityEngine;

    //TEST CLASS FOR SOMETHING

    class Test : MonoBehaviour
    {
        [SerializeField]
        public List<List<JRigidbody>> rigid = new List<List<JRigidbody>>();
        [SerializeField]
        public List<JRigidbody[]> rigid2 = new List<JRigidbody[]>();
    }
}
