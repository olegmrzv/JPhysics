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

        Vector3 lastPosition;
        Quaternion lastRotation;

        const float LerpCof = 0.25f;

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
            JPhysics.AddBody(Body, TransformCallback);
        }

        void Update()
        {
            var pos = transform.position;
            var rot = transform.rotation;
            //if (lastPosition != pos && lastRotation != rot)
            //{
            //    Body.Position = pos.ConvertToJVector();
            //    Body.Orientation = rot.ConvertToJMatrix();
            //    Body.inactiveTime = 0.0f;
            //}

            //if (!Body.IsActive) return;

                transform.position = Vector3.Lerp(pos, lastPosition, LerpCof);
                transform.rotation = Quaternion.Lerp(rot, lastRotation, LerpCof);
        }

        void TransformCallback(Vector3 p, Quaternion r)
        {
            lastPosition = p;
            lastRotation = r;
        }

        public void AddForce()
        {
            
        }
    }
}
