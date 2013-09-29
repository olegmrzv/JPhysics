namespace JPhysics.Unity
{
    using Collision.Shapes;

    internal abstract class JCapsule : JRigidbody
    {
        public float Radius = 0.5f, Height = 1f;
       
        private void Awake()
        {
            Shape = new CapsuleShape(Height, Radius);
        }
    }
}
