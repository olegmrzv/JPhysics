namespace JPhysics.Unity
{
    using Collision.Shapes;
    using UnityEngine;

    [AddComponentMenu("JPhysics/Colliders/JCapsule")]
    public sealed class JCapsule : JRigidbody
    {
        public float Radius = 0.5f, Height = 1f;

        protected override void Awake()
        {
            Shape = new CapsuleShape(Height, Radius);
            base.Awake();
        }
    }
}
