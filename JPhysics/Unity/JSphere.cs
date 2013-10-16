namespace JPhysics.Unity
{
    using Collision.Shapes;
    using UnityEngine;

    [AddComponentMenu("JPhysics/Colliders/JSphere")]
    public class JSphere : JCollider
    {
        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                ((SphereShape) Shape).Radius = radius;
            }
        }

        [SerializeField]
        float radius = .5f;

        protected override Shape MakeShape()
        {
            return new SphereShape(radius);
        }
    }
}
