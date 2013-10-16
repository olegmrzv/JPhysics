namespace JPhysics.Unity.Aides
{
    using UnityEngine;

    class OverlapTest : MonoBehaviour
    {
        JCollider col;

        void Awake()
        {
            col = GetComponent<JCollider>();
        }
        
        void Update()
        {
            foreach (var jb in JPhysics.OverlapMesh(col.Shape, transform.position, transform.rotation))
            {
                Debug.Log(jb.name);
            }
        }
    }
}
