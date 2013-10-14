namespace JPhysics.Unity
{
    using Collision.Shapes;
    using UnityEngine;

    [AddComponentMenu("JPhysics/Colliders/JSphere")]
    public class JSphere : JRigidbody
    {
        public float Radius = 0.5f;

        protected override void Awake()
        {
            Shape = new SphereShape(Radius);
            base.Awake();
        }
    }
}
