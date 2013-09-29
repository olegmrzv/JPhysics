namespace JPhysics.Unity
{
    using Collision.Shapes;
    using UnityEngine;

    [RequireComponent(typeof(TerrainCollider))]
    public class JTerrain : JRigidbody
    {
        private void Awake()
        {
            var data = GetComponent<TerrainCollider>().terrainData;
            var hs = data.GetHeights(0, 0, data.heightmapWidth, data.heightmapHeight);
            var temp = new float[hs.GetLength(0), hs.GetLength(1)];
            for (var i0 = 0; i0 < hs.GetLength(0); i0++)
                for (var i1 = 0; i1 < hs.GetLength(1); i1++)
                {
                    hs[i0, i1] *= (data.heightmapHeight / data.heightmapResolution) * 100;
                    temp[i1, i0] = hs[i0, i1];
                }
            Shape = new TerrainShape(temp, data.heightmapScale.x, data.heightmapScale.z);
        }
    }
}
