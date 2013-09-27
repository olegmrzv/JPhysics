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

        Vector3 lastPosition, lp, cp;
        Quaternion lastRotation, lr, cr;

        const float LerpCof = 0.3f;

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
            if (lp != pos || lr != rot)
            {
                cp = pos;
                cr = rot;
                JPhysics.CorrectTransform(Body,Correct);
            }
            lp = transform.position = Vector3.Lerp(pos, lastPosition, LerpCof);
            lr = transform.rotation = Quaternion.Lerp(rot, lastRotation, LerpCof);
        }

        void TransformCallback(Vector3 p, Quaternion r)
        {
            lastPosition = p;
            lastRotation = r;
        }

        void OnDestroy()
        {
            JPhysics.RemoveBody(Body);
        }

        object[] Correct()
        {
            return new[] {(object)cp, cr};
        }
    }
}
