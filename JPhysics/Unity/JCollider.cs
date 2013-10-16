namespace JPhysics.Unity
{
    using System.Collections.Generic;
    using Collision;
    using Collision.Shapes;
    using Dynamics;
    using LinearMath;
    using UnityEngine;

    public abstract class JCollider : MonoBehaviour
    {
        public bool IsTrigger;
        public event TriggerCollision TriggerEnter, TriggerStay, TriggerExit;

        public Shape Shape
        {
            get
            {
                return shape ?? (shape = MakeShape());
            }
        }

        public PhysicMaterial Material
        {
            get { return material; }
            set
            {
                material = value;
                if (Rigidbody != null && Rigidbody.Body != null)
                {
                    Rigidbody.Body.Material = new Dynamics.Material
                    {
                        KineticFriction = material.dynamicFriction,
                        StaticFriction = material.staticFriction,
                        Restitution = material.bounciness
                    };
                }
            }
        }

        public JRigidbody Rigidbody
        {
            get { return rigidbody ?? (rigidbody = GetComponent<JRigidbody>()); }
        }

        readonly Dictionary<RigidBody, JRigidbody> collidingBodies = new Dictionary<RigidBody, JRigidbody>(); 

        JRigidbody rigidbody;
        [SerializeField]
        Shape shape;
        Shape cloneShape;

        JVector position;
        JMatrix orientation;
        Transform cacheTransform;

        [SerializeField]
        PhysicMaterial material;


        void Awake()
        {
            cacheTransform = transform;
            
            if (Rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<JRigidbody>();
                rigidbody.Damping = JRigidbody.DampingType.None;
                rigidbody.UseGravity = false;
                rigidbody.IsStatic = true;
                rigidbody.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }

            cloneShape = MakeShape();
            JPhysics.Instance.PostStep += Loop;
        }

        protected virtual void Update()
        {
            position = cacheTransform.position.ConvertToJVector();
            orientation = cacheTransform.rotation.ConvertToJMatrix();
        }

        void Loop()
        {
            if (IsTrigger)
            {
                JMatrix otherOrientation;
                JVector otherPosition, point, normal;
                float penetration;

                foreach (var body in JPhysics.Bodies)
                {
                    if (body.Key.Shape is Multishape) continue;

                    otherPosition = body.Key.Position;
                    otherOrientation = body.Key.Orientation;
                    bool collide = XenoCollide.Detect(cloneShape, body.Key.Shape, ref orientation, ref otherOrientation,
                                                        ref position, ref otherPosition, out point, out normal,
                                                        out penetration);

                    if (collide && !collidingBodies.ContainsKey(body.Key))
                    {
                        collidingBodies.Add(body.Key, body.Value);
                        if (TriggerEnter != null) TriggerEnter(body.Value, point, normal, penetration);
                    }
                    else if (!collide && collidingBodies.ContainsKey(body.Key))
                    {
                        collidingBodies.Remove(body.Key);
                        if (TriggerExit != null) TriggerExit(body.Value, point, normal, penetration);
                    }
                    else if(collide && collidingBodies.ContainsKey(body.Key))
                        if (TriggerStay != null) TriggerStay(body.Value, point, normal, penetration);
                }
            }
        }

        protected abstract Shape MakeShape();

        public delegate void TriggerCollision(JRigidbody body, Vector3 point, Vector3 normal, float penetration);
    }
}
