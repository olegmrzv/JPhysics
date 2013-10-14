namespace JPhysics.Unity
{
    using Collision.Shapes;
    using UnityEngine;

    [AddComponentMenu("JPhysics/Colliders/JBox")]
    public sealed class JBox : JRigidbody
    {
        public Vector3 Size = Vector3.one;

        protected override void Awake()
        {
            Shape = new BoxShape((Vector3.Scale(Size,transform.localScale).ConvertToJVector()));
            base.Awake();
        }
    }
}
