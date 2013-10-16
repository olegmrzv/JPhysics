namespace JPhysics.Unity
{
    using Collision.Shapes;
    using Dynamics;
    using LinearMath;
    using UnityEngine;
    using Damp = Dynamics.RigidBody.DampingType;

    [AddComponentMenu("JPhysics/JRigidbody")]
    public sealed class JRigidbody : MonoBehaviour
    {
        public static float LerpFactor = .3f;

        public float Mass
        {
            get { return mass; }
            set
            {
                mass = value;
                if (Body != null) Body.Mass = mass;
            }
        }

        public bool IsStatic
        {
            get { return isStatic; }
            set
            {
                isStatic = value;
                if (Body != null) Body.IsStatic = isStatic;
            }
        }

        public bool UseGravity
        {
            get { return useGravity; }
            set
            {
                useGravity = value;
                if (Body != null) Body.AffectedByGravity = useGravity;
            }
        }

        public DampingType Damping
        {
            get { return damping; }
            set
            {
                damping = value;
                if(Body != null) Body.Damping = (Damping == DampingType.Angular)
                                   ? Damp.Angular
                                   : (Damping == DampingType.Linear)
                                         ? Damp.Linear
                                         : (Damping == DampingType.AngularAndLinear)
                                               ? Damp.Linear | Damp.Angular
                                               : Damp.None;
            }
        }

        public bool IsCompound { get; internal set; }
        public RigidBody Body { get; private set; }
        public JCollider Collider { get { return GetComponent<JCollider>(); } }

        [SerializeField]
        float mass = 1f;
        [SerializeField]
        bool isStatic, useGravity = true;
        [SerializeField]
        DampingType damping = DampingType.AngularAndLinear;
        [SerializeField]
        JCollider collider;

        Vector3 lastPosition, lp, cp;
        Quaternion lastRotation, lr, cr;

        void OnEnable()
        {
            if (IsCompound) return;
            collider = GetComponent<JCollider>();
            if (collider == null)
            {
                Debug.LogWarning("No Collider Found!");
                enabled = false;
                return;
            }
            if (collider.IsTrigger)
            {
                enabled = false;
                return;
            }
            lastPosition = transform.position;
            Body = new RigidBody(collider.Shape)
                {
                    Position = transform.position.ConvertToJVector(),
                    Orientation = transform.rotation.ConvertToJMatrix(),
                    IsStatic = isStatic,
                    Mass = mass,

                    AffectedByGravity = useGravity,
                    Damping = (Damping == DampingType.Angular)
                                  ? Damp.Angular
                                  : (Damping == DampingType.Linear)
                                        ? Damp.Linear
                                        : (Damping == DampingType.AngularAndLinear)
                                              ? Damp.Linear | Damp.Angular
                                              : Damp.None
                };
            var m = collider.Material;
            if (m != null)
                Body.Material = new Dynamics.Material
                    {
                        KineticFriction = m.dynamicFriction,
                        StaticFriction = m.staticFriction,
                        Restitution = m.bounciness
                    };
            JPhysics.AddBody(this);
        }

        void Update()
        {
            if (!collider.IsTrigger)
            {
                var pos = transform.position;
                var rot = transform.rotation;
                if (lp != pos || lr != rot)
                {
                    cp = pos;
                    cr = rot;
                    JPhysics.Correct(this);
                }
                lp = transform.position = Vector3.Lerp(pos, lastPosition, LerpFactor);
                lr = transform.rotation = Quaternion.Lerp(rot, lastRotation, LerpFactor);
            }
            else enabled = false;
        }

        public void TransformUpdate(float timestep)
        {
            lock (Body)
            {
                lastPosition = Body.Position;
                lastRotation = Body.Orientation;
            }
        }

        void OnDisable()
        {
            if (Body != null)
                JPhysics.RemoveBody(this);
        }

        void OnDestroy()
        {
            if (Body != null)
                JPhysics.RemoveBody(this);
        }

        internal void Correct()
        {
            lock (Body)
            {
                Body.Position = cp.ConvertToJVector();
                Body.Orientation = cr.ConvertToJMatrix();
            }
        }

        public void UpdateVariables()
        {
            if (Application.isPlaying)
            {
                var m = collider.Material;
                if (m != null)
                    Body.Material = new Dynamics.Material
                    {
                        KineticFriction = m.dynamicFriction,
                        StaticFriction = m.staticFriction,
                        Restitution = m.bounciness
                    };
                Body.IsStatic = IsStatic;
                Body.Mass = Mass;
                Body.Damping = (Damping == DampingType.Angular)
                   ? Damp.Angular
                   : (Damping == DampingType.Linear)
                         ? Damp.Linear
                         : (Damping == DampingType.AngularAndLinear)
                               ? Damp.Linear | Damp.Angular
                               : Damp.None;
                Body.AffectedByGravity = UseGravity;
            }
        }

        public void AddForce(float x, float y, float z)
        {
            Body.AddForce(new JVector(x, y, z));
        }

        public void AddForce(Vector3 force)
        {
            Body.AddForce(force.ConvertToJVector());
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position)
        {
            Body.AddForce(force.ConvertToJVector(), position.ConvertToJVector());
        }

        public void AddTorque(Vector3 torque)
        {
            Body.AddTorque(torque.ConvertToJVector());
        }

        public void AddImpulse(Vector3 impulse)
        {
            Body.ApplyImpulse(impulse.ConvertToJVector());
        }

        public void AddImpulse(Vector3 impulse, Vector3 relativePosition)
        {
            Body.ApplyImpulse(impulse.ConvertToJVector(), relativePosition.ConvertToJVector());
        }

        public enum DampingType
        {
            None,
            Angular,
            Linear,
            AngularAndLinear
        };
    }
}
