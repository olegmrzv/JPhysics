using JPhysics.Unity;
using UnityEngine;

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

        JVector position;
        JMatrix orientation;
        Transform cacheTransform;

        [SerializeField]
        PhysicMaterial material;

        void OnEnable()
        {
            shape = MakeShape();
            cacheTransform = transform;
            
            if (Rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<JRigidbody>();
                rigidbody.Damping = JRigidbody.DampingType.None;
                rigidbody.UseGravity = false;
                rigidbody.IsStatic = true;
                rigidbody.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }
        }

        protected virtual void Update()
        {
            position = cacheTransform.position.ConvertToJVector();
            orientation = cacheTransform.rotation.ConvertToJMatrix();
        }

        void FixedUpdate()
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
                    bool collide = XenoCollide.Detect(shape, body.Key.Shape, ref orientation, ref otherOrientation,
                                                        ref position, ref otherPosition, out point, out normal,
                                                        out penetration);
                    var i = new TriggerInfo
                    {
                        Body = body.Value,
                        Point = point,
                        Normal = normal,
                        Penetration = penetration
                    };
                    if (collide && !collidingBodies.ContainsKey(body.Key))
                    {
                        collidingBodies.Add(body.Key, body.Value);
                        if (TriggerEnter != null) TriggerEnter(i);
                    }
                    else if (!collide && collidingBodies.ContainsKey(body.Key))
                    {
                        collidingBodies.Remove(body.Key);
                        if (TriggerExit != null) TriggerExit(i);
                    }
                    else if (collide && collidingBodies.ContainsKey(body.Key))
                        if (TriggerStay != null) TriggerStay(i);
                }
            }
        }

        protected abstract Shape MakeShape();


    }

    public delegate void TriggerCollision(TriggerInfo info);

    public class TriggerInfo
    {
        public JRigidbody Body;
        public Vector3 Point;
        public Vector3 Normal;
        public float Penetration;
    }
}
