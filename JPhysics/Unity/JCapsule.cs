namespace JPhysics.Unity
{
    using Collision.Shapes;


    internal abstract class JCapsule : JRigidbody
    {
        public float Radius = 0.5f, Height = 1f;

        protected override void Awake()
        {
            Shape = new CapsuleShape(Height, Radius);
            base.Awake();
        }
    }
}
