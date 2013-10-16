namespace JPhysics.Collision
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Dynamics;
    using LinearMath;
    using Shapes;

    public class CollisionSystemSAP : CollisionSystem
    {
        private readonly List<IBroadphaseEntity> bodyList = new List<IBroadphaseEntity>();
        private readonly List<IBroadphaseEntity> active = new List<IBroadphaseEntity>();

        private class BroadphaseEntityXCompare : IComparer<IBroadphaseEntity>
        {
            private float f;
            public int Compare(IBroadphaseEntity body1, IBroadphaseEntity body2)
            {
                f = body1.BoundingBox.Min.X - body2.BoundingBox.Min.X;
                return (f < 0) ? -1 : (f > 0) ? 1 : 0;
            }
        }

        private readonly BroadphaseEntityXCompare xComparer;

        private bool swapOrder = false;

        public CollisionSystemSAP()
        {
            xComparer = new BroadphaseEntityXCompare();
            detectCallback = DetectCallback;
        }

        public override bool RemoveEntity(IBroadphaseEntity body)
        {
            return bodyList.Remove(body);
        }

        public override void AddEntity(IBroadphaseEntity body)
        {
            if (bodyList.Contains(body))
                throw new ArgumentException("The body was already added to the collision system.", "body");

            bodyList.Add(body);
        }

        readonly Action<object> detectCallback;

        public override void Detect(bool multiThreaded)
        {

            bodyList.Sort(xComparer);
            active.Clear();
            if (multiThreaded)
            {

                foreach (var b in bodyList)
                {
                    AddToActiveMultithreaded(b);
                }

                threadManager.Execute();

            }
            else
            {
                foreach (var b in bodyList)
                {
                    AddToActive(b);
                }
            }
        }


        private void AddToActive(IBroadphaseEntity body)
        {
            var n = active.Count;

            var thisInactive = body.IsStaticOrInactive;

            for (var i = 0; i != n; )
            {
                var  ac = active[i];
                var acBox = ac.BoundingBox;

                if (acBox.Max.X < body.BoundingBox.Min.X)
                {
                    n--;
                    active.RemoveAt(i);
                }
                else
                {
                    var bodyBox = body.BoundingBox;

                    if (!(thisInactive && ac.IsStaticOrInactive) &&
                        (((bodyBox.Max.Z >= acBox.Min.Z) && (bodyBox.Min.Z <= acBox.Max.Z)) &&
                        ((bodyBox.Max.Y >= acBox.Min.Y) && (bodyBox.Min.Y <= acBox.Max.Y))))
                    {
                        if (RaisePassedBroadphase(ac, body))
                        {
                            if (swapOrder) Detect(body, ac);
                            else Detect(ac, body);
                            swapOrder = !swapOrder;
                        }
                    }

                    i++;
                }
            }

            active.Add(body);
        }

        private void AddToActiveMultithreaded(IBroadphaseEntity body)
        {
            var n = active.Count;

            var thisInactive = body.IsStaticOrInactive;

            for (var i = 0; i != n; )
            {
                var ac = active[i];
                var acBox = ac.BoundingBox;

                if (acBox.Max.X < body.BoundingBox.Min.X) 
                {
                    n--;
                    active.RemoveAt(i);
                }
                else
                {
                    var bodyBox = body.BoundingBox;

                    if (!(thisInactive && ac.IsStaticOrInactive) &&
                        (((bodyBox.Max.Z >= acBox.Min.Z) && (bodyBox.Min.Z <= acBox.Max.Z)) &&
                        ((bodyBox.Max.Y >= acBox.Min.Y) && (bodyBox.Min.Y <= acBox.Max.Y))))
                    {
                        if (RaisePassedBroadphase(ac, body))
                        {
                            var pair = BroadphasePair.Pool.GetNew();

                            if (swapOrder) { pair.Entity1 = body; pair.Entity2 = ac; }
                            else { pair.Entity2 = body; pair.Entity1 = ac; }
                            swapOrder = !swapOrder;

                            threadManager.AddTask(detectCallback, pair);
                        }
                    }

                    i++;
                }
            }

            active.Add(body);
        }

        private void DetectCallback(object obj)
        {


            var pair = obj as BroadphasePair;

            base.Detect(pair.Entity1, pair.Entity2);
            BroadphasePair.Pool.GiveBack(pair);


        }

        private int Compare(IBroadphaseEntity body1, IBroadphaseEntity body2)
        {
            float f = body1.BoundingBox.Min.X - body2.BoundingBox.Min.X;
            return (f < 0) ? -1 : (f > 0) ? 1 : 0;
        }

        /// <summary>
        /// Sends a ray (definied by start and direction) through the scene (all bodies added).
        /// NOTE: For performance reasons terrain and trianglemeshshape aren't checked
        /// against rays (rays are of infinite length). They are checked against segments
        /// which start at rayOrigin and end in rayOrigin + rayDirection.
        /// </summary>
        #region public override bool Raycast(JVector rayOrigin, JVector rayDirection, out JVector normal,out float fraction)
        public override bool Raycast(JVector rayOrigin, JVector rayDirection, RaycastCallback raycast, out RigidBody body, out JVector normal, out float fraction)
        {
            body = null; normal = JVector.Zero; fraction = float.MaxValue;

            JVector tempNormal; float tempFraction;
            bool result = false;

            // TODO: This can be done better in CollisionSystemPersistenSAP
            var bl = bodyList.ToArray();
            foreach (IBroadphaseEntity e in bl)
            {
                if (e is SoftBody)
                {
                    SoftBody softBody = e as SoftBody;
                    foreach (RigidBody b in softBody.VertexBodies)
                    {
                        if (this.Raycast(b, rayOrigin, rayDirection, out tempNormal, out tempFraction))
                        {
                            if (tempFraction < fraction && (raycast == null || raycast(b, tempNormal, tempFraction)))
                            {
                                body = b;
                                normal = tempNormal;
                                fraction = tempFraction;
                                result = true;
                            }
                        }
                    }
                }
                else
                {
                    RigidBody b = e as RigidBody;

                    if (Raycast(b, rayOrigin, rayDirection, out tempNormal, out tempFraction))
                    {
                        if (tempFraction < fraction && (raycast == null || raycast(b, tempNormal, tempFraction)))
                        {
                            body = b;
                            normal = tempNormal;
                            fraction = tempFraction;
                            result = true;
                        }
                    }
                }
            }

            return result;
        }
        #endregion


        /// <summary>
        /// Raycasts a single body. NOTE: For performance reasons terrain and trianglemeshshape aren't checked
        /// against rays (rays are of infinite length). They are checked against segments
        /// which start at rayOrigin and end in rayOrigin + rayDirection.
        /// </summary>
        #region public override bool Raycast(RigidBody body, JVector rayOrigin, JVector rayDirection, out JVector normal, out float fraction)
        public override bool Raycast(RigidBody body, JVector rayOrigin, JVector rayDirection, out JVector normal, out float fraction)
        {
            fraction = float.MaxValue; normal = JVector.Zero;

            if (!body.BoundingBox.RayIntersect(ref rayOrigin, ref rayDirection)) return false;

            if (body.Shape is Multishape)
            {
                Multishape ms = (body.Shape as Multishape).RequestWorkingClone();

                JVector tempNormal; float tempFraction;
                bool multiShapeCollides = false;

                JVector transformedOrigin; JVector.Subtract(ref rayOrigin, ref body.position, out transformedOrigin);
                JVector.Transform(ref transformedOrigin, ref body.invOrientation, out transformedOrigin);
                JVector transformedDirection; JVector.Transform(ref rayDirection, ref body.invOrientation, out transformedDirection);

                int msLength = ms.Prepare(ref transformedOrigin, ref transformedDirection);

                for (int i = 0; i < msLength; i++)
                {
                    ms.SetCurrentShape(i);

                    if (GJKCollide.Raycast(ms, ref body.orientation, ref body.invOrientation, ref body.position,
                        ref rayOrigin, ref rayDirection, out tempFraction, out tempNormal))
                    {
                        if (tempFraction < fraction)
                        {
                            if (useTerrainNormal && ms is TerrainShape)
                            {
                                (ms as TerrainShape).CollisionNormal(out tempNormal);
                                JVector.Transform(ref tempNormal, ref body.orientation, out tempNormal);
                                tempNormal.Negate();
                            }
                            else if (useTriangleMeshNormal && ms is TriangleMeshShape)
                            {
                                (ms as TriangleMeshShape).CollisionNormal(out tempNormal);
                                JVector.Transform(ref tempNormal, ref body.orientation, out tempNormal);
                                tempNormal.Negate();
                            }

                            normal = tempNormal;
                            fraction = tempFraction;
                            multiShapeCollides = true;
                        }
                    }
                }

                ms.ReturnWorkingClone();
                return multiShapeCollides;
            }
            return (GJKCollide.Raycast(body.Shape, ref body.orientation, ref body.invOrientation, ref body.position,
                                       ref rayOrigin, ref rayDirection, out fraction, out normal));
        }
        #endregion

    }
}
