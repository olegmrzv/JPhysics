namespace JPhysics.Unity
{
    using Collision.Shapes;

    public class JSphere : JRigidbody
    {
        public float Radius = 0.5f;

        private void Awake()
        {
            Shape = new SphereShape(Radius);
        }
    }
}
