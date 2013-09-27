namespace JPhysics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Collision;
    using DataStructures;
    using Dynamics;
    using Dynamics.Constraints;
    using LinearMath;

    public class World
    {
        public delegate void WorldStep(float timestep);

        public enum DebugType
        {
            CollisionDetect,
            BuildIslands,
            HandleArbiter,
            UpdateContacts,
            PreStep,
            DeactivateBodies,
            IntegrateForces,
            Integrate,
            PostStep,
            ClothUpdate,
            Num
        }

        private readonly Queue<Arbiter> addedArbiterQueue = new Queue<Arbiter>();
        private readonly Action<object> arbiterCallback;
        private readonly ArbiterMap arbiterMap;
        private readonly CollisionDetectedHandler collisionDetectionHandler;
        private readonly HashSet<Constraint> constraints = new HashSet<Constraint>();

        private readonly ContactSettings contactSettings = new ContactSettings();
        private readonly double[] debugTimes = new double[(int) DebugType.Num];
        private readonly WorldEvents events = new WorldEvents();
        private readonly Action<object> integrateCallback;
        private readonly IslandManager islands = new IslandManager();
        private readonly Queue<Arbiter> removedArbiterQueue = new Queue<Arbiter>();
        private readonly Stack<Arbiter> removedArbiterStack = new Stack<Arbiter>();
        private readonly HashSet<RigidBody> rigidBodies = new HashSet<RigidBody>();
        private readonly HashSet<SoftBody> softbodies = new HashSet<SoftBody>();
        private readonly Stopwatch sw = new Stopwatch();
        private readonly ThreadManager threadManager = ThreadManager.Instance;
        private float accumulatedTime;
        private float angularDamping = 0.85f;
        private int contactIterations = 10;
        private float currentAngularDampFactor = 1.0f;
        private float currentLinearDampFactor = 1.0f;
        private float deactivationTime = 2f;
        private JVector gravity = new JVector(0, -9.81f, 0);

        private float inactiveAngularThresholdSq = 0.1f;
        private float inactiveLinearThresholdSq = 0.1f;
        private float linearDamping = 0.85f;

        private int smallIterations = 4;
        private float timestep;

        public World() : this(new CollisionSystemSAP()) {}

        public World(CollisionSystem collision)
        {
            if (collision == null) throw new ArgumentNullException("collision", "The CollisionSystem can't be null.");

            arbiterCallback = ArbiterCallback;
            integrateCallback = IntegrateCallback;

            RigidBodies = new ReadOnlyHashset<RigidBody>(rigidBodies);
            Constraints = new ReadOnlyHashset<Constraint>(constraints);
            SoftBodies = new ReadOnlyHashset<SoftBody>(softbodies);

            CollisionSystem = collision;

            collisionDetectionHandler = CollisionDetected;

            CollisionSystem.CollisionDetected += collisionDetectionHandler;

            arbiterMap = new ArbiterMap();

            AllowDeactivation = true;
        }

        public ReadOnlyHashset<RigidBody> RigidBodies { get; private set; }
        public ReadOnlyHashset<Constraint> Constraints { get; private set; }
        public ReadOnlyHashset<SoftBody> SoftBodies { get; private set; }

        public WorldEvents Events
        {
            get { return events; }
        }


        public ArbiterMap ArbiterMap
        {
            get { return arbiterMap; }
        }

        public ContactSettings ContactSettings
        {
            get { return contactSettings; }
        }

        public List<CollisionIsland> Islands
        {
            get { return islands; }
        }

        public CollisionSystem CollisionSystem { set; get; }

        public JVector Gravity
        {
            get { return gravity; }
            set { gravity = value; }
        }


        public bool AllowDeactivation { get; set; }

        public double[] DebugTimes
        {
            get { return debugTimes; }
        }

        public void AddBody(SoftBody body)
        {
            if (body == null) throw new ArgumentNullException("body", "body can't be null.");
            if (softbodies.Contains(body))
                throw new ArgumentException("The body was already added to the world.", "body");

            softbodies.Add(body);
            CollisionSystem.AddEntity(body);

            events.RaiseAddedSoftBody(body);

            foreach (Constraint constraint in body.EdgeSprings)
                AddConstraint(constraint);

            foreach (SoftBody.MassPoint massPoint in body.VertexBodies)
            {
                events.RaiseAddedRigidBody(massPoint);
                rigidBodies.Add(massPoint);
            }
        }

        public bool RemoveBody(SoftBody body)
        {
            if (!softbodies.Remove(body)) return false;

            CollisionSystem.RemoveEntity(body);

            events.RaiseRemovedSoftBody(body);

            foreach (Constraint constraint in body.EdgeSprings)
                RemoveConstraint(constraint);

            foreach (SoftBody.MassPoint massPoint in body.VertexBodies)
                RemoveBody(massPoint, true);

            return true;
        }


        public void ResetResourcePools()
        {
            IslandManager.Pool.ResetResourcePool();
            Arbiter.Pool.ResetResourcePool();
            Contact.Pool.ResetResourcePool();
        }


        public void Clear()
        {
            foreach (RigidBody body in rigidBodies)
            {
                CollisionSystem.RemoveEntity(body);

                if (body.island != null)
                {
                    body.island.ClearLists();
                    body.island = null;
                }

                body.connections.Clear();
                body.arbiters.Clear();
                body.constraints.Clear();

                events.RaiseRemovedRigidBody(body);
            }

            foreach (SoftBody body in softbodies)
            {
                CollisionSystem.RemoveEntity(body);
            }

            rigidBodies.Clear();

            foreach (Constraint constraint in constraints)
            {
                events.RaiseRemovedConstraint(constraint);
            }
            constraints.Clear();

            softbodies.Clear();

            islands.RemoveAll();

            arbiterMap.Clear();

            ResetResourcePools();
        }


        public void SetDampingFactors(float angularDamping, float linearDamping)
        {
            if (angularDamping < 0.0f || angularDamping > 1.0f)
                throw new ArgumentException("Angular damping factor has to be between 0.0 and 1.0", "angularDamping");

            if (linearDamping < 0.0f || linearDamping > 1.0f)
                throw new ArgumentException("Linear damping factor has to be between 0.0 and 1.0", "linearDamping");

            this.angularDamping = angularDamping;
            this.linearDamping = linearDamping;
        }


        public void SetInactivityThreshold(float angularVelocity, float linearVelocity, float time)
        {
            if (angularVelocity < 0.0f)
                throw new ArgumentException("Angular velocity threshold has to " +
                                            "be larger than zero", "angularVelocity");

            if (linearVelocity < 0.0f)
                throw new ArgumentException("Linear velocity threshold has to " +
                                            "be larger than zero", "linearVelocity");

            if (time < 0.0f)
                throw new ArgumentException("Deactivation time threshold has to " +
                                            "be larger than zero", "time");

            inactiveAngularThresholdSq = angularVelocity*angularVelocity;
            inactiveLinearThresholdSq = linearVelocity*linearVelocity;
            deactivationTime = time;
        }


        public void SetIterations(int iterations, int smallIterations)
        {
            if (iterations < 1)
                throw new ArgumentException("The number of collision " +
                                            "iterations has to be larger than zero", "iterations");

            if (smallIterations < 1)
                throw new ArgumentException("The number of collision " +
                                            "iterations has to be larger than zero", "smallIterations");

            contactIterations = iterations;
            this.smallIterations = smallIterations;
        }


        public bool RemoveBody(RigidBody body)
        {
            return RemoveBody(body, false);
        }

        private bool RemoveBody(RigidBody body, bool removeMassPoints)
        {
            if (!removeMassPoints && body.IsParticle) return false;

            if (!rigidBodies.Remove(body)) return false;

            foreach (Arbiter arbiter in body.arbiters)
            {
                arbiterMap.Remove(arbiter);
                events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
            }

            foreach (Constraint constraint in body.constraints)
            {
                constraints.Remove(constraint);
                events.RaiseRemovedConstraint(constraint);
            }

            CollisionSystem.RemoveEntity(body);

            islands.RemoveBody(body);

            events.RaiseRemovedRigidBody(body);

            return true;
        }


        public void AddBody(RigidBody body)
        {
            if (body == null) throw new ArgumentNullException("body", "body can't be null.");
            if (rigidBodies.Contains(body))
                throw new ArgumentException("The body was already added to the world.", "body");

            events.RaiseAddedRigidBody(body);

            CollisionSystem.AddEntity(body);

            rigidBodies.Add(body);
        }


        public bool RemoveConstraint(Constraint constraint)
        {
            if (!constraints.Remove(constraint)) return false;
            events.RaiseRemovedConstraint(constraint);

            islands.ConstraintRemoved(constraint);

            return true;
        }


        public void AddConstraint(Constraint constraint)
        {
            if (constraints.Contains(constraint))
                throw new ArgumentException("The constraint was already added to the world.", "constraint");

            constraints.Add(constraint);

            islands.ConstraintCreated(constraint);

            events.RaiseAddedConstraint(constraint);
        }


        public void Step(float timestep, bool multithread)
        {
            this.timestep = timestep;

            if (timestep == 0.0f) return;

            if (timestep < 0.0f) throw new ArgumentException("The timestep can't be negative.", "timestep");

            // Calculate this
            currentAngularDampFactor = (float) Math.Pow(angularDamping, timestep);
            currentLinearDampFactor = (float) Math.Pow(linearDamping, timestep);

            sw.Reset();
            sw.Start();
            events.RaiseWorldPreStep(timestep);
            foreach (RigidBody body in rigidBodies) body.PreStep(timestep);

            sw.Stop();
            debugTimes[(int) DebugType.PreStep] = sw.Elapsed.TotalMilliseconds;

            sw.Reset();
            sw.Start();
            UpdateContacts();
            sw.Stop();
            debugTimes[(int) DebugType.UpdateContacts] = sw.Elapsed.TotalMilliseconds;

            sw.Reset();
            sw.Start();
            while (removedArbiterQueue.Count > 0) islands.ArbiterRemoved(removedArbiterQueue.Dequeue());
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;

            sw.Reset();
            sw.Start();
            foreach (SoftBody body in softbodies)
            {
                body.Update(timestep);
                body.DoSelfCollision(collisionDetectionHandler);
            }
            sw.Stop();
            debugTimes[(int) DebugType.ClothUpdate] = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            CollisionSystem.Detect(multithread);
            sw.Stop();
            debugTimes[(int) DebugType.CollisionDetect] = sw.Elapsed.TotalMilliseconds;

            sw.Reset();
            sw.Start();

            while (addedArbiterQueue.Count > 0) islands.ArbiterCreated(addedArbiterQueue.Dequeue());

            sw.Stop();
            debugTimes[(int) DebugType.BuildIslands] = sw.Elapsed.TotalMilliseconds + ms;

            sw.Reset();
            sw.Start();
            CheckDeactivation();
            sw.Stop();
            debugTimes[(int) DebugType.DeactivateBodies] = sw.Elapsed.TotalMilliseconds;

            sw.Reset();
            sw.Start();
            IntegrateForces();
            sw.Stop();
            debugTimes[(int) DebugType.IntegrateForces] = sw.Elapsed.TotalMilliseconds;

            sw.Reset();
            sw.Start();
            HandleArbiter(multithread);
            sw.Stop();
            debugTimes[(int) DebugType.HandleArbiter] = sw.Elapsed.TotalMilliseconds;

            sw.Reset();
            sw.Start();
            Integrate(multithread);
            sw.Stop();
            debugTimes[(int) DebugType.Integrate] = sw.Elapsed.TotalMilliseconds;

            sw.Reset();
            sw.Start();
            foreach (RigidBody body in rigidBodies) body.PostStep(timestep);
            events.RaiseWorldPostStep(timestep);
            sw.Stop();
            debugTimes[(int) DebugType.PostStep] = sw.Elapsed.TotalMilliseconds;
        }

        public void Step(float totalTime, bool multithread, float timestep, int maxSteps)
        {
            int counter = 0;
            accumulatedTime += totalTime;

            while (accumulatedTime > timestep)
            {
                Step(timestep, multithread);

                accumulatedTime -= timestep;
                counter++;

                if (counter > maxSteps)
                {
                    accumulatedTime = 0.0f;
                    return;
                }
            }
        }

        private void UpdateArbiterContacts(Arbiter arbiter)
        {
            if (arbiter.contactList.Count == 0)
            {
                lock (removedArbiterStack)
                {
                    removedArbiterStack.Push(arbiter);
                }
                return;
            }

            for (int i = arbiter.contactList.Count - 1; i >= 0; i--)
            {
                Contact c = arbiter.contactList[i];
                c.UpdatePosition();

                if (c.penetration < -contactSettings.breakThreshold)
                {
                    Contact.Pool.GiveBack(c);
                    arbiter.contactList.RemoveAt(i);
                }
                else
                {
                    JVector diff;
                    JVector.Subtract(ref c.p1, ref c.p2, out diff);
                    float distance = JVector.Dot(ref diff, ref c.normal);

                    diff = diff - distance*c.normal;
                    distance = diff.LengthSquared();

                    if (distance > contactSettings.breakThreshold*contactSettings.breakThreshold*100)
                    {
                        Contact.Pool.GiveBack(c);
                        arbiter.contactList.RemoveAt(i);
                    }
                }
            }
        }

        private void UpdateContacts()
        {
            foreach (Arbiter arbiter in arbiterMap.Arbiters)
            {
                UpdateArbiterContacts(arbiter);
            }

            while (removedArbiterStack.Count > 0)
            {
                Arbiter arbiter = removedArbiterStack.Pop();
                Arbiter.Pool.GiveBack(arbiter);
                arbiterMap.Remove(arbiter);

                removedArbiterQueue.Enqueue(arbiter);
                events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
            }
        }

        private void ArbiterCallback(object obj)
        {
            var island = obj as CollisionIsland;

            int thisIterations = island.Bodies.Count + island.Constraints.Count > 3
                                     ? contactIterations
                                     : smallIterations;

            for (int i = -1; i < thisIterations; i++)
            {
                foreach (Arbiter arbiter in island.arbiter)
                {
                    int contactCount = arbiter.contactList.Count;
                    for (int e = 0; e < contactCount; e++)
                    {
                        if (i == -1) arbiter.contactList[e].PrepareForIteration(timestep);
                        else arbiter.contactList[e].Iterate();
                    }
                }

                foreach (Constraint c in island.constraints)
                {
                    if (c.body1 != null && !c.body1.IsActive && c.body2 != null && !c.body2.IsActive)
                        continue;

                    if (i == -1) c.PrepareForIteration(timestep);
                    else c.Iterate();
                }
            }
        }

        private void HandleArbiter(bool multiThreaded)
        {
            if (multiThreaded)
            {
                foreach (CollisionIsland i in islands)
                {
                    if (i.IsActive()) threadManager.AddTask(arbiterCallback, i);
                }

                threadManager.Execute();
            }
            else
            {
                foreach (CollisionIsland i in islands)
                {
                    if (i.IsActive()) arbiterCallback(i);
                }
            }
        }

        private void IntegrateForces()
        {
            foreach (RigidBody body in rigidBodies)
            {
                if (!body.isStatic && body.IsActive)
                {
                    JVector temp;
                    JVector.Multiply(ref body.force, body.inverseMass*timestep, out temp);
                    JVector.Add(ref temp, ref body.linearVelocity, out body.linearVelocity);

                    if (!(body.isParticle))
                    {
                        JVector.Multiply(ref body.torque, timestep, out temp);
                        JVector.Transform(ref temp, ref body.invInertiaWorld, out temp);
                        JVector.Add(ref temp, ref body.angularVelocity, out body.angularVelocity);
                    }

                    if (body.affectedByGravity)
                    {
                        JVector.Multiply(ref gravity, timestep, out temp);
                        JVector.Add(ref body.linearVelocity, ref temp, out body.linearVelocity);
                    }
                }

                body.force.MakeZero();
                body.torque.MakeZero();
            }
        }

        private void IntegrateCallback(object obj)
        {
            var body = obj as RigidBody;

            JVector temp;
            JVector.Multiply(ref body.linearVelocity, timestep, out temp);
            JVector.Add(ref temp, ref body.position, out body.position);

            if (!(body.isParticle))
            {
                JVector axis;
                float angle = body.angularVelocity.Length();

                if (angle < 0.001f)
                {
                    JVector.Multiply(ref body.angularVelocity,
                                     (0.5f*timestep - (timestep*timestep*timestep)*(0.020833333333f)*angle*angle),
                                     out axis);
                }
                else
                {
                    JVector.Multiply(ref body.angularVelocity, ((float) Math.Sin(0.5f*angle*timestep)/angle), out axis);
                }

                var dorn = new JQuaternion(axis.X, axis.Y, axis.Z, (float) Math.Cos(angle*timestep*0.5f));
                JQuaternion ornA;
                JQuaternion.CreateFromMatrix(ref body.orientation, out ornA);

                JQuaternion.Multiply(ref dorn, ref ornA, out dorn);

                dorn.Normalize();
                JMatrix.CreateFromQuaternion(ref dorn, out body.orientation);
            }

            if ((body.Damping & RigidBody.DampingType.Linear) != 0)
                JVector.Multiply(ref body.linearVelocity, currentLinearDampFactor, out body.linearVelocity);

            if ((body.Damping & RigidBody.DampingType.Angular) != 0)
                JVector.Multiply(ref body.angularVelocity, currentAngularDampFactor, out body.angularVelocity);

            body.Update();


            if (CollisionSystem.EnableSpeculativeContacts || body.EnableSpeculativeContacts)
                body.SweptExpandBoundingBox(timestep);
        }

        private void Integrate(bool multithread)
        {
            if (multithread)
            {
                foreach (RigidBody body in rigidBodies)
                {
                    if (body.isStatic || !body.IsActive) continue;
                    threadManager.AddTask(integrateCallback, body);
                }

                threadManager.Execute();
            }
            else
            {
                foreach (RigidBody body in rigidBodies)
                {
                    if (body.isStatic || !body.IsActive) continue;
                    integrateCallback(body);
                }
            }
        }


        //object locker = new object();
        private void CollisionDetected(RigidBody body1, RigidBody body2, JVector point1, JVector point2, JVector normal,
                                       float penetration)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Arbiter arbiter;

            sw.Start();
            arbiterMap.LookUpArbiter(body1, body2, out arbiter);
            sw.Stop();
            if (arbiter == null)
            {

                arbiter = Arbiter.Pool.GetNew();
                arbiter.body1 = body1;
                arbiter.body2 = body2;
                arbiterMap.Add(new ArbiterKey(body1, body2), arbiter);

                addedArbiterQueue.Enqueue(arbiter);

                events.RaiseBodiesBeginCollide(body1, body2);
            }

            UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
            sw.Reset();


            Contact contact;

            if (arbiter.body1 == body1)
            {
                JVector.Negate(ref normal, out normal);
                contact = arbiter.AddContact(point1, point2, normal, penetration, contactSettings);
            }
            else
            {
                contact = arbiter.AddContact(point2, point1, normal, penetration, contactSettings);
            }

            if (contact != null) events.RaiseContactCreated(contact);
        }

        private void CheckDeactivation()
        {
            foreach (CollisionIsland island in islands)
            {
                bool deactivateIsland = true;

                if (!AllowDeactivation) deactivateIsland = false;
                else
                {
                    foreach (RigidBody body in island.bodies)
                    {
                        if (body.AllowDeactivation &&
                            (body.angularVelocity.LengthSquared() < inactiveAngularThresholdSq &&
                             (body.linearVelocity.LengthSquared() < inactiveLinearThresholdSq)))
                        {
                            body.inactiveTime += timestep;
                            if (body.inactiveTime < deactivationTime)
                                deactivateIsland = false;
                        }
                        else
                        {
                            body.inactiveTime = 0.0f;
                            deactivateIsland = false;
                        }
                    }
                }

                foreach (RigidBody body in island.bodies)
                {
                    if (body.isActive == deactivateIsland)
                    {
                        if (body.isActive)
                        {
                            body.IsActive = false;
                            events.RaiseDeactivatedBody(body);
                        }
                        else
                        {
                            body.IsActive = true;
                            events.RaiseActivatedBody(body);
                        }
                    }
                }
            }
        }

        public class WorldEvents
        {
            internal WorldEvents()
            {
            }

            internal void RaiseWorldPreStep(float timestep)
            {
                if (PreStep != null) PreStep(timestep);
            }

            internal void RaiseWorldPostStep(float timestep)
            {
                if (PostStep != null) PostStep(timestep);
            }

            internal void RaiseAddedRigidBody(RigidBody body)
            {
                if (AddedRigidBody != null) AddedRigidBody(body);
            }

            internal void RaiseRemovedRigidBody(RigidBody body)
            {
                if (RemovedRigidBody != null) RemovedRigidBody(body);
            }

            internal void RaiseAddedConstraint(Constraint constraint)
            {
                if (AddedConstraint != null) AddedConstraint(constraint);
            }

            internal void RaiseRemovedConstraint(Constraint constraint)
            {
                if (RemovedConstraint != null) RemovedConstraint(constraint);
            }

            internal void RaiseAddedSoftBody(SoftBody body)
            {
                if (AddedSoftBody != null) AddedSoftBody(body);
            }

            internal void RaiseRemovedSoftBody(SoftBody body)
            {
                if (RemovedSoftBody != null) RemovedSoftBody(body);
            }

            internal void RaiseBodiesBeginCollide(RigidBody body1, RigidBody body2)
            {
                if (BodiesBeginCollide != null) BodiesBeginCollide(body1, body2);
            }

            internal void RaiseBodiesEndCollide(RigidBody body1, RigidBody body2)
            {
                if (BodiesEndCollide != null) BodiesEndCollide(body1, body2);
            }

            internal void RaiseActivatedBody(RigidBody body)
            {
                if (ActivatedBody != null) ActivatedBody(body);
            }

            internal void RaiseDeactivatedBody(RigidBody body)
            {
                if (DeactivatedBody != null) DeactivatedBody(body);
            }

            internal void RaiseContactCreated(Contact contact)
            {
                if (ContactCreated != null) ContactCreated(contact);
            }

            public event WorldStep PreStep;
            public event WorldStep PostStep;

            public event Action<RigidBody> AddedRigidBody;
            public event Action<RigidBody> RemovedRigidBody;
            public event Action<Constraint> AddedConstraint;
            public event Action<Constraint> RemovedConstraint;
            public event Action<SoftBody> AddedSoftBody;
            public event Action<SoftBody> RemovedSoftBody;

            public event Action<RigidBody, RigidBody> BodiesBeginCollide;
            public event Action<RigidBody, RigidBody> BodiesEndCollide;
            public event Action<Contact> ContactCreated;

            public event Action<RigidBody> DeactivatedBody;
            public event Action<RigidBody> ActivatedBody;
        }
    }
}