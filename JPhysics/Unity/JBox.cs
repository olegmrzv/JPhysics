namespace JPhysics.Unity
{
    using Collision.Shapes;
    using UnityEngine;

    [AddComponentMenu("JPhysics/Colliders/JBox"), SerializePrivateVariables]
    public sealed class JBox : JCollider
    {
        public Vector3 Size
        {
            get { return size; }
            set
            {
                size = value;
                ((BoxShape)Shape).Size = Vector3.Scale(size, transform.localScale).ConvertToJVector();
            }
        }
        
        private Vector3 size = Vector3.one;

        protected override Shape MakeShape()
        {
            return new BoxShape((Vector3.Scale(size, transform.localScale).ConvertToJVector()));
        }
    }
}
