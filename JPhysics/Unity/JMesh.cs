namespace JPhysics.Unity
{
    using System.Collections.Generic;
    using System.Linq;
    using Collision;
    using Collision.Shapes;
    using LinearMath;
    using UnityEngine;

    [AddComponentMenu("JPhysics/Colliders/JMesh")]
    [RequireComponent(typeof(MeshFilter))]
    public class JMesh : JRigidbody
    {
        public bool Convex;

        protected override void Awake()
        {
            var mf = GetComponent<MeshFilter>();
            var mesh = mf.mesh;
            if (mesh == null) { Debug.Log("No Mesh found!"); return; }

            if (Convex)
            {
                Shape = new ConvexHullShape(GetComponent<MeshFilter>().mesh.vertices.Select(vert => vert.ConvertToJVector()).ToList());
            }
            else
            {
                var positions = new List<JVector>();
                var indices = new List<TriangleVertexIndices>();

                var vertices = mesh.vertices;
                var count = mesh.vertices.Length;
                var scale = transform.lossyScale;
                for (var i = 0; i < count; i++)
                {
                    var v = vertices[i];
                    v.x *= scale.x;
                    v.y *= scale.y;
                    v.z *= scale.z;

                    positions.Add(new JVector(v.x, v.y, v.z));
                }

                count = mesh.triangles.Length;
                var triangles = mesh.triangles;
                for (var i = 0; i < count; i += 3)
                {
                    indices.Add(new TriangleVertexIndices(triangles[i], triangles[i + 2], triangles[i + 1]));
                }

                var octree = new Octree(positions, indices);
                octree.BuildOctree();

                Shape = new TriangleMeshShape(octree);
            }
            base.Awake();
        }
    }

}
