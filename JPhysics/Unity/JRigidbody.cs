namespace JPhysics.Unity
{
    using Collision.Shapes;
    using Dynamics;
    using LinearMath;
    using UnityEngine;
    using Damp = Dynamics.RigidBody.DampingType;

    public abstract class JRigidbody : MonoBehaviour
    {
        public static float LerpFactor = .3f;

        public float Mass = 1f;
        public bool IsStatic, UseGravity = true;
        public DampingType Damping = DampingType.AngularAndLinear;

        public PhysicMaterial Material;

        [HideInInspector]
        public bool IsCompound;
        public RigidBody Body;
        public Shape Shape;

        Vector3 lastPosition, lp, cp;
        Quaternion lastRotation, lr, cr;

        protected virtual void Awake()
        {
            if (IsCompound) return;
            lastPosition = transform.position;
            Body = new RigidBody(Shape)
                {
                    Position = transform.position.ConvertToJVector(),
                    Orientation = transform.rotation.ConvertToJMatrix(),
                    IsStatic = IsStatic,
                    Mass = Mass,
                    AffectedByGravity = UseGravity,
                    Damping = (Damping == DampingType.Angular)
                                  ? RigidBody.DampingType.Angular
                                  : (Damping == DampingType.Linear)
                                        ? RigidBody.DampingType.Linear
                                        : (Damping == DampingType.AngularAndLinear)
                                              ? RigidBody.DampingType.Linear | RigidBody.DampingType.Angular
                                              : RigidBody.DampingType.None
                };
            if (Material != null)
                Body.Material = new Dynamics.Material
                    {
                        KineticFriction = Material.dynamicFriction,
                        StaticFriction = Material.staticFriction,
                        Restitution = Material.bounciness
                    };
            JPhysics.AddBody(this);
        }

        void Update()
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


        public void TransformUpdate(float timestep)
        {
            lock (Body)
            {
                lastPosition = Body.Position.ConvertToVector3();
                lastRotation = Body.Orientation.ConvertToQuaternion();
            }
        }

        void OnDestroy()
        {
            JPhysics.RemoveBody(this);
        }

        public void Correct()
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
                if (Material != null)
                    Body.Material = new Dynamics.Material
                    {
                        KineticFriction = Material.dynamicFriction,
                        StaticFriction = Material.staticFriction,
                        Restitution = Material.bounciness
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

        public enum DampingType
        {
            None,
            Angular,
            Linear,
            AngularAndLinear
        };
    }
}
