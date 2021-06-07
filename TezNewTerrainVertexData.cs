#define UseThread1

using System;
using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewTerrainVertexData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public TezNewTerrainFace terrainFace;
        public string name;

        public void createMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "Cell";
            mesh.vertices = this.vertices;
            mesh.triangles = this.triangles;
            mesh.RecalculateNormals();

            GameObject go = terrainFace.gameObject;

            var mf = go.GetComponent<MeshFilter>();
            mf.mesh = mesh;
        }
    }
}
