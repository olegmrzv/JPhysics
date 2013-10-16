namespace JPhysics.Unity
{
    using Collision.Shapes;
    using UnityEngine;

    [AddComponentMenu("JPhysics/Colliders/JCapsule")]
    public sealed class JCapsule : JCollider
    {
        public float Radius
        {
            get { return radius; }
            set 
            {
                radius = value;
                var s = ((CapsuleShape) Shape);

                s.Radius = radius;
            }
        }
            
        public float Height
        {
            get { return height; }
            set
            {
                height = value;
                var s = ((CapsuleShape)Shape);
                s.Length = height;
            }
        }

        [SerializeField]
        float radius = .5f, height = 1f;

        protected override Shape MakeShape()
        {
            return new CapsuleShape(height, radius);
        }
    }
}
