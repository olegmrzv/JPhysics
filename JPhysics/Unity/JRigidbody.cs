namespace JPhysics.Unity
{
    using Collision.Shapes;
    using Dynamics;
    using UnityEngine;

    public abstract class JRigidbody : MonoBehaviour
    {
        public float Mass = 1f;
        public bool IsStatic;

        [HideInInspector]
        public bool IsCompound;
        public RigidBody Body;
        public Shape Shape;

        readonly Vector3 v = new Vector3();
        readonly Quaternion q = new Quaternion();
        Vector3 lastPosition;
        Quaternion lastRotation;

        void Start()
        {
            if (IsCompound) return;
            lastPosition = transform.position;
            Body = new RigidBody(Shape)
            {
                Position = transform.position.ConvertToJVector(),
                Orientation = transform.rotation.ConvertToJMatrix(),
                IsStatic = IsStatic,
                Mass = Mass
            };
            JPhysics.AddBody(Body);
        }

        void Update()
        {
            var pos = transform.position;
            var rot = transform.rotation;
            if (lastPosition != pos)
            {
                Body.position = pos.ConvertToJVector();
                Body.inactiveTime = 0.0f;
            }
            if (lastRotation != rot)
            {
                Body.orientation = rot.ConvertToJMatrix();
                Body.inactiveTime = 0.0f;
            }

            if (!Body.IsActive) return;
            lastPosition = transform.position = Body.Position.ConvertToVector3(v);
            lastRotation = transform.rotation = Body.Orientation.ConvertToQuaternion(q);
        }

        public void AddForce()
        {
            
        }
    }
}
